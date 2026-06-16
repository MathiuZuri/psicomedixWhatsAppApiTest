using Clinica.Domain.Enums;

namespace Clinica.Domain.DTOs.Comprobantes;

public class EmitirComprobanteCitaDto
{
    public Guid CitaId { get; set; }

    public TipoComprobante TipoComprobante { get; set; } = TipoComprobante.ConstanciaCita;

    public TipoFormatoImpresion FormatoImpresion { get; set; } = TipoFormatoImpresion.A4;

    public string? Observacion { get; set; }
}