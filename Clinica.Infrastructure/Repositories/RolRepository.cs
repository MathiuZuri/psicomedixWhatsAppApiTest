using Clinica.Domain.Entities;
using Clinica.Domain.Interfaces;
using Clinica.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infrastructure.Repositories;

public class RolRepository : GenericRepository<Rol>, IRolRepository
{
    public RolRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Rol?> ObtenerPorNombreAsync(string nombre)
    {
        return await Context.Roles
            .Include(x => x.RolPermisos)
            .ThenInclude(x => x.Permiso)
            .FirstOrDefaultAsync(x => x.Nombre == nombre);
    }
    public async Task<Rol?> ObtenerConPermisosAsync(Guid id)
    {
        return await Context.Roles
            .Include(x => x.RolPermisos)
            .ThenInclude(x => x.Permiso)
            .FirstOrDefaultAsync(x => x.Id == id);
    }
    
    public async Task<bool> TienePermisoAsignadoAsync(Guid rolId, Guid permisoId)
    {
        return await Context.RolPermisos.AnyAsync(x =>
            x.RolId == rolId &&
            x.PermisoId == permisoId);
    }

    public async Task AgregarPermisoAsync(RolPermiso rolPermiso)
    {
        await Context.RolPermisos.AddAsync(rolPermiso);
    }
}