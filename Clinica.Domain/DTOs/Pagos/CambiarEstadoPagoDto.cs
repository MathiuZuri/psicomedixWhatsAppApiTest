using System.ComponentModel.DataAnnotations;
using Clinica.Domain.Enums;

namespace Clinica.Domain.DTOs.Pagos;

public class CambiarEstadoPagoDto
{
    [Required(ErrorMessage = "El nuevo estado es obligatorio.")]
    public EstadoPago Estado { get; set; }
}