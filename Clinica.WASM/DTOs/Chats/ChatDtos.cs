namespace Clinica.WASM.DTOs.Chats;

public class ChatResponseDto
{
    public Guid Id { get; set; }
    public string TelefonoWhatsApp { get; set; } = string.Empty;
    public string NombreContacto { get; set; } = string.Empty;
    public string UltimoMensaje { get; set; } = string.Empty;
    public DateTime FechaUltimaInteraccion { get; set; }
    public int MensajesNoLeidos { get; set; }
    public Guid? PacienteId { get; set; }
}

public class MensajeChatResponseDto
{
    public Guid Id { get; set; }
    public Guid ChatId { get; set; }
    public string Texto { get; set; } = string.Empty;
    public bool EsMio { get; set; }
    public DateTime FechaEnvio { get; set; }
    public string MessageIdWhatsApp { get; set; } = string.Empty;
}

public class EnviarMensajeFrontDto
{
    public Guid ChatId { get; set; }
    public string Texto { get; set; } = string.Empty;
}