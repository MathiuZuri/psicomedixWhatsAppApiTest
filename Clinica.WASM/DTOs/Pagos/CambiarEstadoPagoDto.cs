using System.ComponentModel.DataAnnotations;
using Clinica.WASM.DTOs.Pagos;

namespace Clinica.WASM.DTOs.Pagos;

public class CambiarEstadoPagoDto
{
    [Required(ErrorMessage = "El nuevo estado es obligatorio.")]
    public EstadoPago Estado { get; set; }
}