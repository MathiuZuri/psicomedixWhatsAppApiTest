using System.ComponentModel.DataAnnotations;

namespace Clinica.WASM.DTOs.Usuarios;

public class AsignarRolUsuarioDto
{
    [Required(ErrorMessage = "El usuario es obligatorio.")]
    public Guid UsuarioId { get; set; }

    [Required(ErrorMessage = "El rol es obligatorio.")]
    public Guid RolId { get; set; }
}