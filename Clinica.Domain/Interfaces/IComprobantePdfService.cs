using Clinica.Domain.DTOs.Comprobantes;

namespace Clinica.Domain.Interfaces;

public interface IComprobantePdfService
{
    byte[] GenerarBoletaPagoPdf(ComprobantePagoPreviewDto dto);

    byte[] GenerarConstanciaCitaPdf(ComprobanteCitaPreviewDto dto);

    byte[] GenerarResumenAtencionPdf(ComprobanteAtencionPreviewDto dto);

    byte[] GenerarEstadoCuentaPacientePdf(ComprobanteEstadoCuentaPreviewDto dto);
}