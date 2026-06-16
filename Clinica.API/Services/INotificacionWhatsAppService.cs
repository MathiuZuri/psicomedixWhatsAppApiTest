namespace Clinica.API.Services;

// esto es exclusivo de evolution api
public interface INotificacionWhatsAppService
{
    Task EnviarMensajeAsync(string telefonoDestino, string mensaje, CancellationToken cancellationToken = default);
}