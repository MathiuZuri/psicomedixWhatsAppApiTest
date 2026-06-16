using Clinica.Domain.Entities;

namespace Clinica.Domain.Interfaces;

public interface IRolRepository : IGenericRepository<Rol>
{
    Task<Rol?> ObtenerPorNombreAsync(string nombre);
    Task<Rol?> ObtenerConPermisosAsync(Guid id);
    
    Task<bool> TienePermisoAsignadoAsync(Guid rolId, Guid permisoId);
    Task AgregarPermisoAsync(RolPermiso rolPermiso);
}