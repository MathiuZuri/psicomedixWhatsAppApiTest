using System.ComponentModel.DataAnnotations;

namespace Clinica.WASM.DTOs.Pacientes;

public class ActualizarContactoPacienteDto
{
    [RegularExpression(@"^\d{9}$", ErrorMessage = "El celular debe tener exactamente 9 dígitos.")]
    public string? Celular { get; set; }

    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [StringLength(150, ErrorMessage = "El correo no debe superar los 150 caracteres.")]
    public string? Correo { get; set; }

    [StringLength(200, ErrorMessage = "La dirección no debe superar los 200 caracteres.")]
    public string? Direccion { get; set; }
}