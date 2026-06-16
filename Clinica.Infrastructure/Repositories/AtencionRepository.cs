using Clinica.Domain.Entities;
using Clinica.Domain.Interfaces;
using Clinica.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infrastructure.Repositories;

public class AtencionRepository : GenericRepository<Atencion>, IAtencionRepository
{
    public AtencionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Atencion>> ObtenerPorPacienteAsync(Guid pacienteId)
    {
        return await Context.Atenciones
            .Include(x => x.Paciente)
            .Include(x => x.Doctor)
            .Include(x => x.ServicioClinico)
            .Include(x => x.Cita)
            .Where(x => x.PacienteId == pacienteId)
            .OrderByDescending(x => x.FechaInicio)
            .ToListAsync();
    }

    public async Task<Atencion?> ObtenerPorCitaAsync(Guid citaId)
    {
        return await Context.Atenciones
            .Include(x => x.Paciente)
            .Include(x => x.Doctor)
            .Include(x => x.ServicioClinico)
            .Include(x => x.Cita)
            .FirstOrDefaultAsync(x => x.CitaId == citaId);
    }
}