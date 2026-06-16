using Clinica.Domain.DTOs.Roles;

namespace Clinica.API.Services;

public interface IRolService
{
    Task<IEnumerable<RolResponseDto>> ObtenerTodosAsync();
    Task<RolResponseDto?> ObtenerPorIdAsync(Guid id);
    Task<Guid> CrearAsync(CrearRolDto dto);
    Task ActualizarAsync(Guid id, EditarRolDto dto);
    Task AsignarPermisosAsync(AsignarPermisosRolDto dto);
}