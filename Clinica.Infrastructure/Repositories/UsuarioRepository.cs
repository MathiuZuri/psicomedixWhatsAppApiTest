using Clinica.Domain.Entities;
using Clinica.Domain.Interfaces;
using Clinica.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infrastructure.Repositories;

public class UsuarioRepository : GenericRepository<Usuario>, IUsuarioRepository
{
    public UsuarioRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Usuario?> ObtenerPorCorreoAsync(string correo)
    {
        return await Context.Usuarios
            .Include(x => x.UsuarioRoles)
            .ThenInclude(x => x.Rol)
            .ThenInclude(x => x.RolPermisos)
            .ThenInclude(x => x.Permiso)
            .FirstOrDefaultAsync(x => x.Correo == correo);
    }

    public async Task<Usuario?> ObtenerPorUserNameAsync(string userName)
    {
        return await Context.Usuarios
            .Include(x => x.UsuarioRoles)
            .ThenInclude(x => x.Rol)
            .ThenInclude(x => x.RolPermisos)
            .ThenInclude(x => x.Permiso)
            .FirstOrDefaultAsync(x => x.UserName == userName);
    }
    public async Task<Usuario?> ObtenerConRolesAsync(Guid id)
    {
        return await Context.Usuarios
            .Include(x => x.UsuarioRoles)
            .ThenInclude(x => x.Rol)
            .FirstOrDefaultAsync(x => x.Id == id);
    }
    public async Task<bool> TieneRolAsignadoAsync(Guid usuarioId, Guid rolId)
    {
        return await Context.UsuarioRoles.AnyAsync(x =>
            x.UsuarioId == usuarioId &&
            x.RolId == rolId &&
            x.Activo);
    }

    public async Task AgregarRolAsync(UsuarioRol usuarioRol)
    {
        await Context.UsuarioRoles.AddAsync(usuarioRol);
    }
}