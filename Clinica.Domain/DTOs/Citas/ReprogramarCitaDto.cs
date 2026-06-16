using System.ComponentModel.DataAnnotations;
using Clinica.Domain.Validations;

namespace Clinica.Domain.DTOs.Citas;

public class ReprogramarCitaDto
{
    [NotEmptyGuid(ErrorMessage = "El doctor es obligatorio.")]
    public Guid DoctorId { get; set; }

    public Guid? HorarioDoctorId { get; set; }

    public DateOnly NuevaFecha { get; set; }

    public TimeOnly NuevaHoraInicio { get; set; }

    public TimeOnly NuevaHoraFin { get; set; }

    [StringLength(500, ErrorMessage = "El motivo de reprogramación no debe superar los 500 caracteres.")]
    public string? MotivoReprogramacion { get; set; }
}