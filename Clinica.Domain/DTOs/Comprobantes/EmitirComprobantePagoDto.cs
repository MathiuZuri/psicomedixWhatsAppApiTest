using Clinica.Domain.Enums;

namespace Clinica.Domain.DTOs.Comprobantes;

public class EmitirComprobantePagoDto
{
    public Guid PagoId { get; set; }

    public string? CodigoPago { get; set; }

    public TipoComprobante TipoComprobante { get; set; } = TipoComprobante.BoletaPago;

    public TipoFormatoImpresion FormatoImpresion { get; set; } = TipoFormatoImpresion.A4;

    public string? Observacion { get; set; }
}