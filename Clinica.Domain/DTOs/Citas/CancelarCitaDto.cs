using System.ComponentModel.DataAnnotations;

namespace Clinica.Domain.DTOs.Citas;

public class CancelarCitaDto
{
    [Required(ErrorMessage = "El motivo de cancelación es obligatorio.")]
    [StringLength(500, MinimumLength = 3, ErrorMessage = "El motivo de cancelación debe tener entre 3 y 500 caracteres.")]
    public string MotivoCancelacion { get; set; } = string.Empty;
}