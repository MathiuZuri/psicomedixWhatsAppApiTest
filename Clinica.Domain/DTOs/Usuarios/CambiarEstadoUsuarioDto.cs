using System.ComponentModel.DataAnnotations;
using Clinica.Domain.Enums;

namespace Clinica.Domain.DTOs.Usuarios;

public class CambiarEstadoUsuarioDto
{
    [Required(ErrorMessage = "El nuevo estado es obligatorio.")]
    public EstadoUsuario Estado { get; set; }
}