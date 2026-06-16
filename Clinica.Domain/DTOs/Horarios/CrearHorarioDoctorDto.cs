using Clinica.Domain.Validations;

namespace Clinica.Domain.DTOs.Horarios;

public class CrearHorarioDoctorDto
{
    [NotEmptyGuid(ErrorMessage = "El doctor es obligatorio.")]
    public Guid DoctorId { get; set; }

    public DayOfWeek DiaSemana { get; set; }

    public TimeOnly HoraInicio { get; set; }

    public TimeOnly HoraFin { get; set; }

    public DateOnly FechaInicioVigencia { get; set; }

    public DateOnly? FechaFinVigencia { get; set; }
}