using Clinica.Domain.Entities;

namespace Clinica.Domain.Interfaces;

public interface IDoctorRepository : IGenericRepository<Doctor>
{
    Task<Doctor?> ObtenerPorCmpAsync(string cmp);
    Task<IEnumerable<Doctor>> ObtenerActivosAsync();
}