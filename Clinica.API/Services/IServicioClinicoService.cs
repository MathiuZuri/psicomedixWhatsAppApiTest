using Clinica.Domain.DTOs.Servicios;

namespace Clinica.API.Services;

public interface IServicioClinicoService
{
    Task<IEnumerable<ServicioClinicoResponseDto>> ObtenerTodosAsync();
    Task<IEnumerable<ServicioClinicoResponseDto>> ObtenerActivosAsync();
    Task<ServicioClinicoResponseDto?> ObtenerPorIdAsync(Guid id);
}