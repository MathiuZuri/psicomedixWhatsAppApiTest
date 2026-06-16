using Clinica.Domain.Entities;
using Clinica.Domain.Interfaces;
using Clinica.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infrastructure.Repositories;

public class PacienteRepository : GenericRepository<Paciente>, IPacienteRepository
{
    public PacienteRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Paciente?> ObtenerPorDniAsync(string dni)
    {
        return await Context.Pacientes
            .Include(x => x.HistorialClinico)
            .FirstOrDefaultAsync(x => x.DNI == dni);
    }

    public async Task<Paciente?> ObtenerConHistorialAsync(Guid pacienteId)
    {
        return await Context.Pacientes
            .Include(x => x.HistorialClinico)
            .ThenInclude(x => x!.Detalles)
            .FirstOrDefaultAsync(x => x.Id == pacienteId);
    }
}