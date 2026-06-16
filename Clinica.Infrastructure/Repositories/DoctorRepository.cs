using Clinica.Domain.Entities;
using Clinica.Domain.Enums;
using Clinica.Domain.Interfaces;
using Clinica.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infrastructure.Repositories;

public class DoctorRepository : GenericRepository<Doctor>, IDoctorRepository
{
    public DoctorRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Doctor?> ObtenerPorCmpAsync(string cmp)
    {
        return await Context.Doctores
            .Include(x => x.Horarios)
            .FirstOrDefaultAsync(x => x.CMP == cmp);
    }

    public async Task<IEnumerable<Doctor>> ObtenerActivosAsync()
    {
        return await Context.Doctores
            .Where(x => x.Estado == EstadoDoctor.Activo)
            .ToListAsync();
    }
}