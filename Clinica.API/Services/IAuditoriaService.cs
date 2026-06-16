using Clinica.Domain.DTOs.Auditoria;

namespace Clinica.API.Services;

public interface IAuditoriaService
{
    Task<IEnumerable<AuditoriaResponseDto>> ObtenerTodosAsync();
    Task<IEnumerable<AuditoriaResponseDto>> ObtenerPorUsuarioAsync(Guid usuarioId);
}