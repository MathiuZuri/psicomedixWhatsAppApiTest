using Clinica.Domain.Entities;

namespace Clinica.Domain.Interfaces;

public interface IUsuarioRepository : IGenericRepository<Usuario>
{
    Task<Usuario?> ObtenerPorCorreoAsync(string correo);
    Task<Usuario?> ObtenerPorUserNameAsync(string userName);
    Task<Usuario?> ObtenerConRolesAsync(Guid id);
    
    Task<bool> TieneRolAsignadoAsync(Guid usuarioId, Guid rolId);
    Task AgregarRolAsync(UsuarioRol usuarioRol);
}