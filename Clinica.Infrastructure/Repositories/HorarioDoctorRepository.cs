using Clinica.Domain.Entities;
using Clinica.Domain.Interfaces;
using Clinica.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infrastructure.Repositories;

public class HorarioDoctorRepository : GenericRepository<HorarioDoctor>, IHorarioDoctorRepository
{
    public HorarioDoctorRepository(ApplicationDbContext context) : base(context)
    {
    }
    public async Task<IEnumerable<HorarioDoctor>> ObtenerTodosConDoctorAsync()
    {
        return await Context.HorariosDoctor
            .Include(x => x.Doctor)
            .OrderBy(x => x.DiaSemana)
            .ThenBy(x => x.HoraInicio)
            .ToListAsync();
    }

    public async Task<IEnumerable<HorarioDoctor>> ObtenerPorDoctorAsync(Guid doctorId)
    {
        return await Context.HorariosDoctor
            .Include(x => x.Doctor)
            .Where(x => x.DoctorId == doctorId)
            .OrderBy(x => x.DiaSemana)
            .ThenBy(x => x.HoraInicio)
            .ToListAsync();
    }
}