using Clinica.Domain.Enums;

namespace Clinica.Domain.DTOs.Atenciones;

public class AtencionResponseDto
{
    public Guid Id { get; set; }
    public string CodigoAtencion { get; set; } = string.Empty;

    public Guid PacienteId { get; set; }
    public string PacienteNombre { get; set; } = string.Empty;

    public Guid DoctorId { get; set; }
    public string DoctorNombre { get; set; } = string.Empty;

    public Guid ServicioClinicoId { get; set; }
    public string ServicioNombre { get; set; } = string.Empty;

    public Guid? CitaId { get; set; }
    public Guid HistorialClinicoId { get; set; }

    public DateTime FechaInicio { get; set; }
    public DateTime? FechaCierre { get; set; }

    public string MotivoConsulta { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public string? DiagnosticoResumen { get; set; }
    public string? Indicaciones { get; set; }
    public string? Tratamiento { get; set; }

    public EstadoAtencion Estado { get; set; }

    public decimal CostoFinal { get; set; }
    public decimal MontoPagado { get; set; }
    public decimal SaldoPendiente { get; set; }
}