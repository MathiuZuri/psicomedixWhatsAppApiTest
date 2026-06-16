using System.ComponentModel.DataAnnotations;

namespace Clinica.Domain.DTOs.Pacientes;

public class CrearPacienteDto
{
    [Required(ErrorMessage = "El DNI es obligatorio.")]
    [RegularExpression(@"^\d{8}$", ErrorMessage = "El DNI debe tener exactamente 8 dígitos.")]
    public string DNI { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los nombres son obligatorios.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Los nombres deben tener entre 2 y 100 caracteres.")]
    public string Nombres { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los apellidos son obligatorios.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Los apellidos deben tener entre 2 y 100 caracteres.")]
    public string Apellidos { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
    public DateTime FechaNacimiento { get; set; }

    [Required(ErrorMessage = "El sexo es obligatorio.")]
    [RegularExpression(@"^[MF]$", ErrorMessage = "El sexo debe ser 'M' o 'F'.")]
    public string Sexo { get; set; } = string.Empty;

    [RegularExpression(@"^\d{9}$", ErrorMessage = "El celular debe tener exactamente 9 dígitos.")]
    public string? Celular { get; set; }

    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [StringLength(150, ErrorMessage = "El correo no debe superar los 150 caracteres.")]
    public string? Correo { get; set; }

    [StringLength(200, ErrorMessage = "La dirección no debe superar los 200 caracteres.")]
    public string? Direccion { get; set; }
}