using Clinica.Domain.Entities;

namespace Clinica.Domain.Interfaces;

public interface IPermisoRepository : IGenericRepository<Permiso>
{
    Task<Permiso?> ObtenerPorCodigoAsync(string codigo);
}