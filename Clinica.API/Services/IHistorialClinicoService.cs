using Clinica.Domain.DTOs.Historiales;

namespace Clinica.API.Services;

public interface IHistorialClinicoService
{
    Task<HistorialClinicoResponseDto?> ObtenerPorPacienteAsync(Guid pacienteId);
    Task<HistorialClinicoResponseDto?> ObtenerConDetallesAsync(Guid historialId);
}