using Clinica.API.Hubs;
using Clinica.API.Services;
using Clinica.Domain.DTOs.WhatsAppDto;
using Clinica.Domain.Entities;
using Clinica.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Clinica.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Asegura que solo usuarios logueados (secretarias/psicólogos) usen el chat
public class ChatsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly INotificacionWhatsAppService _whatsAppService;
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly ILogger<ChatsController> _logger;

    public ChatsController(
        ApplicationDbContext context,
        INotificacionWhatsAppService whatsAppService,
        IHubContext<ChatHub> hubContext,
        ILogger<ChatsController> logger)
    {
        _context = context;
        _whatsAppService = whatsAppService;
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <summary>
    /// Envía un mensaje de WhatsApp a un paciente desde el panel de la clínica.
    /// </summary>
    [HttpPost("enviar")]
    public async Task<IActionResult> EnviarMensaje([FromBody] EnviarMensajeDto dto)
    {
        // 1. Validar la existencia del chat en la base de datos
        var chat = await _context.Chats
            .FirstOrDefaultAsync(c => c.Id == dto.ChatId);

        if (chat == null)
        {
            return NotFound(new { mensaje = "El chat especificado no existe." });
        }

        try
        {
            // 2. Despachar el mensaje físico a través de Evolution API
            // Si el contenedor Docker falla o el número no existe, esto lanzará una excepción
            await _whatsAppService.EnviarMensajeAsync(chat.TelefonoWhatsApp, dto.Texto);

            // 3. Registrar la burbuja en el historial local (EsMio = true)
            var nuevoMensaje = new MensajeChat
            {
                ChatId = chat.Id,
                Texto = dto.Texto,
                EsMio = true,
                FechaEnvio = DateTime.UtcNow,
                // Generamos un ID de rastreo local ya que el mensaje sale de nuestra API
                MessageIdWhatsApp = $"api_out_{Guid.NewGuid().ToString("N").Substring(0, 12)}"
            };

            // 4. Actualizar los metadatos de la cabecera del Chat
            chat.UltimoMensaje = dto.Texto;
            chat.FechaUltimaInteraccion = DateTime.UtcNow;
            chat.MensajesNoLeidos = 0; // Como estamos respondiendo, limpiamos las alertas de "no leído"

            _context.MensajesChat.Add(nuevoMensaje);
            _context.Chats.Update(chat);
            
            await _context.SaveChangesAsync();

            // 5. NOTIFICAR EN TIEMPO REAL VÍA SIGNALR
            // Emitimos el payload para que la pantalla que envió el mensaje (y cualquier otra abierta) 
            // pinten la burbuja verde a la derecha instantáneamente.
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

            return Ok(new { mensaje = "Mensaje enviado y registrado con éxito.", mensajeId = nuevoMensaje.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al intentar enviar mensaje de WhatsApp al chat {ChatId}", dto.ChatId);
            return StatusCode(500, new { mensaje = "Error interno al procesar el envío a través de WhatsApp.", detalle = ex.Message });
        }
    }

    /// <summary>
    /// Endpoint complementario para listar las conversaciones activas (Columna izquierda del WhatsApp clon)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ObtenerChatsActivos()
    {
        var chats = await _context.Chats
            .OrderByDescending(c => c.FechaUltimaInteraccion)
            .ToListAsync();

        return Ok(chats);
    }

    /// <summary>
    /// Endpoint complementario para cargar las burbujas de un chat específico al hacer clic en él
    /// </summary>
    [HttpGet("{chatId:guid}/mensajes")]
    public async Task<IActionResult> ObtenerHistorialMensajes(Guid chatId)
    {
        var mensajes = await _context.MensajesChat
            .Where(m => m.ChatId == chatId)
            .OrderBy(m => m.FechaEnvio)
            .ToListAsync();

        return Ok(mensajes);
    }
}