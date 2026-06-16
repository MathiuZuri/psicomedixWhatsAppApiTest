using System.Text.Json; 
using Clinica.API.Configurations;
using Clinica.API.Hubs;
using Clinica.Domain.DTOs.WhatsAppDto;
using Clinica.Domain.Entities;
using Clinica.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Clinica.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class WebhooksController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly WhatsAppOptions _whatsAppOptions;
    private readonly ILogger<WebhooksController> _logger;

    public WebhooksController(
        ApplicationDbContext context,
        IHubContext<ChatHub> hubContext,
        IOptions<WhatsAppOptions> whatsAppOptions,
        ILogger<WebhooksController> logger)
    {
        _context = context;
        _hubContext = hubContext;
        _whatsAppOptions = whatsAppOptions.Value;
        _logger = logger;
    }

    [HttpPost("whatsapp/{*eventType}")]
    public async Task<IActionResult> ReceiveWhatsAppEvent(
        [FromHeader(Name = "apikey")] string? incomingApiKey,
        string? eventType)
    {
        // 1. Las llaves válidas aceptadas por el sistema
        string tokenConfigurado = _whatsAppOptions.WebhookSecretToken;
        string tokenInstanciaGrafica = "E8FACB7B4B46-4998-BC8F-2666926A2F5F";

        bool esLlaveValida = !string.IsNullOrEmpty(incomingApiKey) && 
                             (incomingApiKey == tokenConfigurado || incomingApiKey == tokenInstanciaGrafica);

        // BYPASS AUTOMÁTICO EN MODO DESARROLLO LOCAL
#if DEBUG
        if (!esLlaveValida)
        {
            _logger.LogWarning("[PsicoMedix Laboratorio] Token recibido '{Recibido}' no coincide con el esperado, pero se fuerza el acceso por estar en modo DEBUG.", incomingApiKey);
            esLlaveValida = true;
        }
#endif

        if (!esLlaveValida)
        {
            _logger.LogWarning("Webhook RECHAZADO: Petición sin APIKEY válida para el evento: {Evento}", eventType);
            return Ok();
        }

        // 2. Leer el cuerpo de la petición directamente desde el Stream de red
        using var reader = new StreamReader(Request.Body);
        string rawJson = await reader.ReadToEndAsync();

        if (string.IsNullOrWhiteSpace(rawJson))
            return Ok();

        // 3. Inspeccionar el JSON usando JsonDocument
        using var doc = JsonDocument.Parse(rawJson);
        if (!doc.RootElement.TryGetProperty("event", out var eventProp) || eventProp.GetString() != "messages.upsert")
        {
            // Limpiamos de la cola de Docker los eventos 'contacts-upsert', 'contacts-update', etc.
            return Ok();
        }

        // 4. Como confirmamos que SÍ es un mensaje nuevo, lo deserializamos de forma segura a nuestro DTO
        EvolutionWebhookDto? payload;
        try
        {
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            payload = JsonSerializer.Deserialize<EvolutionWebhookDto>(rawJson, options);
            
            if (payload == null || payload.Data == null) 
                return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al deserializar el payload de messages.upsert");
            return Ok(); 
        }

        // ====================================================================
        // PROCESAMIENTO Y PERSISTENCIA DE DATOS
        // ====================================================================
        var keyData = payload.Data.Key;
        var textoMensaje = payload.Data.Message?.Conversation 
                           ?? payload.Data.Message?.ExtendedTextMessage?.Text;

        if (string.IsNullOrEmpty(textoMensaje))
            return Ok();

        var telefonoCompleto = keyData.RemoteJid.Split('@')[0];

        var yaExisteMensaje = await _context.MensajesChat
            .AnyAsync(m => m.MessageIdWhatsApp == keyData.Id);
            
        if (yaExisteMensaje)
            return Ok(new { status = "duplicate_ignored" });

        var chat = await _context.Chats
            .FirstOrDefaultAsync(c => c.TelefonoWhatsApp == telefonoCompleto);

        bool esMensajeMio = keyData.FromMe;

        if (chat == null)
        {
            chat = new Chat
            {
                TelefonoWhatsApp = telefonoCompleto,
                NombreContacto = payload.Data.PushName ?? "Contacto de WhatsApp",
                UltimoMensaje = textoMensaje,
                FechaUltimaInteraccion = DateTime.UtcNow,
                MensajesNoLeidos = esMensajeMio ? 0 : 1
            };

            var paciente = await _context.Pacientes
                .FirstOrDefaultAsync(p => p.Celular != null && p.Celular.Contains(telefonoCompleto.Substring(telefonoCompleto.Length - 9)));

            if (paciente != null)
            {
                chat.PacienteId = paciente.Id;
                chat.NombreContacto = $"{paciente.Nombres} {paciente.Apellidos}".Trim();
            }

            _context.Chats.Add(chat);
        }
        else
        {
            chat.UltimoMensaje = textoMensaje;
            chat.FechaUltimaInteraccion = DateTime.UtcNow;
            
            if (!esMensajeMio)
                chat.MensajesNoLeidos++;

            _context.Chats.Update(chat);
        }

        await _context.SaveChangesAsync();

        var nuevoMensaje = new MensajeChat
        {
            ChatId = chat.Id,
            Texto = textoMensaje,
            EsMio = esMensajeMio,
            FechaEnvio = DateTime.UtcNow,
            MessageIdWhatsApp = keyData.Id
        };

        _context.MensajesChat.Add(nuevoMensaje);
        await _context.SaveChangesAsync();

        // 5. TRANSMISIÓN EN TIEMPO REAL CON MATCH DE CASING (PascalCase para Blazor)
        var payloadSignalR = new
        {
            ChatId = chat.Id,              
            Telefono = chat.TelefonoWhatsApp, 
            Nombre = chat.NombreContacto,    
            Texto = nuevoMensaje.Texto,      
            EsMio = nuevoMensaje.EsMio,      
            Fecha = nuevoMensaje.FechaEnvio.ToString("yyyy-MM-ddTHH:mm:ssZ"), 
            MensajesNoLeidos = chat.MensajesNoLeidos 
        };

        await _hubContext.Clients.All.SendAsync("RecibirNuevoMensaje", payloadSignalR);

        _logger.LogInformation("[PsicoMedix Pro] Mensaje entrante procesado y guardado automáticamente para {Contacto}.", chat.NombreContacto);
        return Ok(new { status = "persisted_and_broadcasted" });
    }
}