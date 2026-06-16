using System.ComponentModel.DataAnnotations;

namespace Clinica.WASM.DTOs.Pacientes;

public class CambiarEstadoPacienteDto
{
    [Required(ErrorMessage = "El nuevo estado es obligatorio.")]
    public EstadoPaciente Estado { get; set; }
}