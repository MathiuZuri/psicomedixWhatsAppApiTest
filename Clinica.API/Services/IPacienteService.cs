using Clinica.Domain.DTOs.Pacientes;

namespace Clinica.API.Services;

public interface IPacienteService
{
    Task<IEnumerable<PacienteResponseDto>> ObtenerTodosAsync();
    Task<PacienteResponseDto?> ObtenerPorIdAsync(Guid id);
    Task<PacienteResponseDto?> ObtenerPorDniAsync(string dni);
    Task<Guid> CrearAsync(CrearPacienteDto dto);
    Task ActualizarContactoAsync(Guid id, ActualizarContactoPacienteDto dto);
    Task CambiarEstadoAsync(Guid id, CambiarEstadoPacienteDto dto);
}