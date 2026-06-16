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
        [FromHeader(Name = "apikey")] string incomingApiKey,
        [FromBody] JsonElement rawPayload, // ◄── Cambiado a JsonElement dinámico para evitar el Error 400
        string? eventType)
    {
        // 1. Validación de seguridad estricta
        if (string.IsNullOrEmpty(incomingApiKey) || incomingApiKey != _whatsAppOptions.WebhookSecretToken)
        {
            _logger.LogWarning("Intento de acceso NO AUTORIZADO al Webhook detectado.");
            return Unauthorized();
        }

        // 2. Extraer la propiedad "event" de forma dinámica y segura
        if (!rawPayload.TryGetProperty("event", out var eventProp) || eventProp.GetString() != "messages.upsert")
        {
            // Si es 'contacts-update', 'contacts-upsert', etc., respondemos 200 OK 
            // para que Docker sea feliz y procese el siguiente elemento de la cola.
            return Ok();
        }

        // 3. Como confirmamos que SÍ es un mensaje nuevo, lo deserializamos de forma segura a nuestro DTO
        EvolutionWebhookDto? payload;
        try
        {
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            payload = JsonSerializer.Deserialize<EvolutionWebhookDto>(rawPayload.GetRawText(), options);
            
            if (payload == null || payload.Data == null) 
                return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al deserializar el payload de messages.upsert");
            return Ok(); 
        }

        // ====================================================================
        // TODO TU CÓDIGO DE PERSISTENCIA ANTERIOR SIGUE EXACTAMENTE IGUAL
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

        var payloadSignalR = new
        {
            chatId = chat.Id,
            telefono = chat.TelefonoWhatsApp,
            nombre = chat.NombreContacto,
            texto = nuevoMensaje.Texto,
            esMio = nuevoMensaje.EsMio,
            fecha = nuevoMensaje.FechaEnvio.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            mensajesNoLeidos = chat.MensajesNoLeidos
        };

        await _hubContext.Clients.All.SendAsync("RecibirNuevoMensaje", payloadSignalR);

        _logger.LogInformation("[PsicoMedix Pro] Mensaje procesado con éxito.");
        return Ok(new { status = "persisted_and_broadcasted" });
    }
}