using Clinica.Domain.DTOs.Permisos;

namespace Clinica.API.Services;

public interface IPermisoService
{
    Task<IEnumerable<PermisoResponseDto>> ObtenerTodosAsync();
}