using Clinica.Domain.DTOs.Doctores;

namespace Clinica.API.Services;

public interface IDoctorService
{
    Task<IEnumerable<DoctorResponseDto>> ObtenerTodosAsync();
    Task<IEnumerable<DoctorResponseDto>> ObtenerActivosAsync();
    Task<DoctorResponseDto?> ObtenerPorIdAsync(Guid id);
    Task<Guid> CrearAsync(CrearDoctorDto dto);
    Task ActualizarAsync(Guid id, EditarDoctorDto dto);
}