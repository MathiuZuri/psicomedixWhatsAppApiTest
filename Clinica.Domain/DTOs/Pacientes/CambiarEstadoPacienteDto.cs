using System.ComponentModel.DataAnnotations;
using Clinica.Domain.Enums;

namespace Clinica.Domain.DTOs.Pacientes;

public class CambiarEstadoPacienteDto
{
    [Required(ErrorMessage = "El nuevo estado es obligatorio.")]
    public EstadoPaciente Estado { get; set; }
}