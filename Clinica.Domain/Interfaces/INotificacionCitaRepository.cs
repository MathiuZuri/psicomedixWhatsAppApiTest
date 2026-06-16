using Clinica.Domain.Entities;

namespace Clinica.Domain.Interfaces;

// esto es exclusivo de evolution api
public interface INotificacionCitaRepository : IGenericRepository<NotificacionCita>
{
    Task<bool> ExisteRecordatorioParaCitaAsync(Guid citaId);

    Task<IEnumerable<NotificacionCita>> ObtenerPendientesAsync(DateTime fechaActualUtc, int maxIntentos);
    
    Task ActualizarAsync(NotificacionCita notificacion);
}