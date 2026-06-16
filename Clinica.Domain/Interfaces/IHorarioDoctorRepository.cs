using Clinica.Domain.Entities;

namespace Clinica.Domain.Interfaces;

public interface IHorarioDoctorRepository : IGenericRepository<HorarioDoctor>
{
    Task<IEnumerable<HorarioDoctor>> ObtenerPorDoctorAsync(Guid doctorId);
    Task<IEnumerable<HorarioDoctor>> ObtenerTodosConDoctorAsync();
}