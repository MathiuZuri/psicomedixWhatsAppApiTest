using Clinica.Domain.DTOs.Pagos;

namespace Clinica.API.Services;

public interface IPagoService
{
    Task<IEnumerable<PagoResponseDto>> ObtenerPorPacienteAsync(Guid pacienteId);
    Task<IEnumerable<PagoResponseDto>> ObtenerPorCitaAsync(Guid citaId);
    Task<IEnumerable<PagoResponseDto>> ObtenerPorAtencionAsync(Guid atencionId);
    Task<Guid> RegistrarAsync(RegistrarPagoDto dto);
    Task CambiarEstadoAsync(Guid id, CambiarEstadoPagoDto dto);
}