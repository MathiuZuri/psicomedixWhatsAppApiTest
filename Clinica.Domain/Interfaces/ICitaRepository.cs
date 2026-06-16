using Clinica.Domain.Entities;

namespace Clinica.Domain.Interfaces;

public interface ICitaRepository : IGenericRepository<Cita>
{
    Task<IEnumerable<Cita>> ObtenerTodasConRelacionesAsync();
    Task<Cita?> ObtenerPorIdConRelacionesAsync(Guid id);
    
    Task<bool> ExisteInterferenciaHorarioAsync(
        Guid doctorId,
        DateOnly fecha,
        TimeOnly horaInicio,
        TimeOnly horaFin,
        Guid? citaIdExcluir = null);

    Task<IEnumerable<Cita>> ObtenerPorPacienteAsync(Guid pacienteId);
    Task<IEnumerable<Cita>> ObtenerPorDoctorAsync(Guid doctorId);
    
    // esto es exclusivo de evolution api, no incluir al sistema
    Task<IEnumerable<Cita>> ObtenerCitasParaRecordatorioAsync(
        DateTime desdeUtc,
        DateTime hastaUtc);
}