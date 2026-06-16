using Clinica.Domain.Entities;
using Clinica.Domain.Enums;
using Clinica.Domain.Interfaces;
using Clinica.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infrastructure.Repositories;

// esto es exclusivo de evolution api, no incluir al sistema
public class NotificacionCitaRepository 
    : GenericRepository<NotificacionCita>, INotificacionCitaRepository
{
    public NotificacionCitaRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<bool> ExisteRecordatorioParaCitaAsync(Guid citaId)
    {
        return await Context.Set<NotificacionCita>()
            .AnyAsync(x =>
                x.CitaId == citaId &&
                x.Canal == CanalNotificacion.WhatsApp &&
                x.Estado != EstadoNotificacion.Cancelado);
    }

    public async Task<IEnumerable<NotificacionCita>> ObtenerPendientesAsync(
        DateTime fechaActualUtc,
        int maxIntentos)
    {
        return await Context.Set<NotificacionCita>()
            .Include(x => x.Cita)
            .ThenInclude(x => x.Paciente)
            .Include(x => x.Cita)
            .ThenInclude(x => x.Doctor)
            .Where(x =>
                x.Estado == EstadoNotificacion.Pendiente &&
                x.FechaProgramadaEnvio <= fechaActualUtc &&
                x.Intentos < maxIntentos)
            .OrderBy(x => x.FechaProgramadaEnvio)
            .ToListAsync();
    }

    public async Task ActualizarAsync(NotificacionCita notificacion)
    {
        Context.Set<NotificacionCita>().Update(notificacion);
        await Context.SaveChangesAsync();
    }
}