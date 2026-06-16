using Clinica.Domain.DTOs.Usuarios;

namespace Clinica.API.Services;

public interface IUsuarioService
{
    Task<IEnumerable<UsuarioResponseDto>> ObtenerTodosAsync();
    Task<UsuarioResponseDto?> ObtenerPorIdAsync(Guid id);
    Task<Guid> CrearAsync(CrearUsuarioDto dto);
    Task ActualizarAsync(Guid id, EditarUsuarioDto dto);
    Task AsignarRolAsync(AsignarRolUsuarioDto dto);
    Task CambiarEstadoAsync(Guid id, CambiarEstadoUsuarioDto dto);
}