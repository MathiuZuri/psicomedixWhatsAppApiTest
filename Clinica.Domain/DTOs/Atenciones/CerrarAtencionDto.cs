using System.ComponentModel.DataAnnotations;

namespace Clinica.Domain.DTOs.Atenciones;

public class CerrarAtencionDto
{
    [StringLength(1000, ErrorMessage = "El diagnóstico no debe superar los 1000 caracteres.")]
    public string? DiagnosticoResumen { get; set; }

    [StringLength(1000, ErrorMessage = "Las indicaciones no deben superar los 1000 caracteres.")]
    public string? Indicaciones { get; set; }

    [StringLength(1000, ErrorMessage = "El tratamiento no debe superar los 1000 caracteres.")]
    public string? Tratamiento { get; set; }

    [StringLength(1000, ErrorMessage = "Las observaciones finales no deben superar los 1000 caracteres.")]
    public string? ObservacionesFinales { get; set; }
}