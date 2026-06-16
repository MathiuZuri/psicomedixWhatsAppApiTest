using System.ComponentModel.DataAnnotations;

namespace Clinica.WASM.DTOs.Roles;

public class AsignarPermisosRolDto
{
    [Required(ErrorMessage = "El rol es obligatorio.")]
    public Guid RolId { get; set; }

    [Required(ErrorMessage = "Debe seleccionar al menos un permiso.")]
    [MinLength(1, ErrorMessage = "Debe seleccionar al menos un permiso.")]
    public List<Guid> PermisosIds { get; set; } = new();
}