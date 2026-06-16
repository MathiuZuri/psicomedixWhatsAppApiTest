using Clinica.Domain.DTOs.Atenciones;

namespace Clinica.API.Services;

public interface IAtencionService
{
    Task<IEnumerable<AtencionResponseDto>> ObtenerPorPacienteAsync(Guid pacienteId);
    Task<AtencionResponseDto?> ObtenerPorCitaAsync(Guid citaId);
    Task<Guid> RegistrarAsync(RegistrarAtencionDto dto);
    Task CerrarAsync(Guid id, CerrarAtencionDto dto);
}