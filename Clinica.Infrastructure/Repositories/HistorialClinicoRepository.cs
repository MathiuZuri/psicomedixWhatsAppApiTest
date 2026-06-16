using Clinica.Domain.Entities;
using Clinica.Domain.Interfaces;
using Clinica.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infrastructure.Repositories;

public class HistorialClinicoRepository : GenericRepository<HistorialClinico>, IHistorialClinicoRepository
{
    public HistorialClinicoRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<HistorialClinico?> ObtenerPorPacienteAsync(Guid pacienteId)
    {
        return await Context.HistorialesClinicos
            .Include(x => x.Paciente)
            .FirstOrDefaultAsync(x => x.PacienteId == pacienteId);
    }

    public async Task<HistorialClinico?> ObtenerConDetallesAsync(Guid historialId)
    {
        return await Context.HistorialesClinicos
            .Include(x => x.Paciente)
            .Include(x => x.Detalles)
            .ThenInclude(x => x.Usuario)
            .FirstOrDefaultAsync(x => x.Id == historialId);
    }
}