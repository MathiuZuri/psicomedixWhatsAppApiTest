using System.ComponentModel.DataAnnotations;
using Clinica.WASM.DTOs.Usuarios;

namespace Clinica.WASM.DTOs.Usuarios;

public class CambiarEstadoUsuarioDto
{
    [Required(ErrorMessage = "El nuevo estado es obligatorio.")]
    public EstadoUsuario Estado { get; set; }
}