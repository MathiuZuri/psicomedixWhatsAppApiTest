using System.ComponentModel.DataAnnotations;

namespace Clinica.WASM.DTOs.Horarios;

public class CrearHorarioDoctorDto
{
    [Required(ErrorMessage = "El doctor es obligatorio.")]
    public Guid DoctorId { get; set; }

    [Required(ErrorMessage = "El día es obligatorio.")]
    public DayOfWeek DiaSemana { get; set; }

    [Required(ErrorMessage = "La hora de inicio es obligatoria.")]
    public TimeOnly HoraInicio { get; set; } = new TimeOnly(8, 0);

    [Required(ErrorMessage = "La hora de fin es obligatoria.")]
    public TimeOnly HoraFin { get; set; } = new TimeOnly(17, 0);

    [Required(ErrorMessage = "La fecha de inicio de vigencia es obligatoria.")]
    public DateOnly FechaInicioVigencia { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public DateOnly? FechaFinVigencia { get; set; }
}