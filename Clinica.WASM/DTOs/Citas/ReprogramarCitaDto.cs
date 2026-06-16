using System.ComponentModel.DataAnnotations;

namespace Clinica.WASM.DTOs.Citas;

public class ReprogramarCitaDto
{
    [Required(ErrorMessage = "El doctor es obligatorio.")]
    public Guid DoctorId { get; set; }

    public Guid? HorarioDoctorId { get; set; }

    [Required(ErrorMessage = "La nueva fecha es obligatoria.")]
    public DateOnly NuevaFecha { get; set; }

    [Required(ErrorMessage = "La nueva hora de inicio es obligatoria.")]
    public TimeOnly NuevaHoraInicio { get; set; }

    [Required(ErrorMessage = "La nueva hora de fin es obligatoria.")]
    public TimeOnly NuevaHoraFin { get; set; }

    [StringLength(500, ErrorMessage = "El motivo no debe superar los 500 caracteres.")]
    public string? MotivoReprogramacion { get; set; }
}