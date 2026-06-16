using System.ComponentModel.DataAnnotations;
using Clinica.Domain.Validations;

namespace Clinica.Domain.DTOs.Atenciones;

public class RegistrarAtencionDto
{
    [NotEmptyGuid(ErrorMessage = "El paciente es obligatorio.")]
    public Guid PacienteId { get; set; }

    [NotEmptyGuid(ErrorMessage = "El doctor es obligatorio.")]
    public Guid DoctorId { get; set; }

    [NotEmptyGuid(ErrorMessage = "El servicio clínico es obligatorio.")]
    public Guid ServicioClinicoId { get; set; }

    public Guid? CitaId { get; set; }

    [NotEmptyGuid(ErrorMessage = "El historial clínico es obligatorio.")]
    public Guid HistorialClinicoId { get; set; }

    [Required(ErrorMessage = "El motivo de consulta es obligatorio.")]
    [StringLength(500, MinimumLength = 3, ErrorMessage = "El motivo de consulta debe tener entre 3 y 500 caracteres.")]
    public string MotivoConsulta { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Las observaciones no deben superar los 1000 caracteres.")]
    public string? Observaciones { get; set; }

    [StringLength(1000, ErrorMessage = "El diagnóstico no debe superar los 1000 caracteres.")]
    public string? DiagnosticoResumen { get; set; }

    [StringLength(1000, ErrorMessage = "Las indicaciones no deben superar los 1000 caracteres.")]
    public string? Indicaciones { get; set; }

    [StringLength(1000, ErrorMessage = "El tratamiento no debe superar los 1000 caracteres.")]
    public string? Tratamiento { get; set; }

    [Range(0, 999999.99, ErrorMessage = "El costo final no puede ser negativo.")]
    public decimal CostoFinal { get; set; }
}