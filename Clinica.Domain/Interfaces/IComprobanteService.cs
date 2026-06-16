using Clinica.Domain.DTOs.Comprobantes;

namespace Clinica.Domain.Interfaces;

public interface IComprobanteService
{
    // ==========================================================
    // PREVIEWS
    // ==========================================================

    Task<ComprobantePagoPreviewDto> PreviewBoletaPagoAsync(Guid pagoId, decimal tasaImpuesto = 18m);

    Task<ComprobanteCitaPreviewDto> PreviewConstanciaCitaAsync(Guid citaId);

    Task<ComprobanteAtencionPreviewDto> PreviewResumenAtencionAsync(Guid atencionId);

    Task<ComprobanteEstadoCuentaPreviewDto> PreviewEstadoCuentaPacienteAsync(Guid pacienteId);

    // ==========================================================
    // EMISIÓN
    // ==========================================================

    Task<Guid> EmitirBoletaPagoAsync(EmitirComprobantePagoDto dto);

    Task<Guid> EmitirConstanciaCitaAsync(EmitirComprobanteCitaDto dto);

    Task<Guid> EmitirResumenAtencionAsync(EmitirComprobanteAtencionDto dto);

    // ==========================================================
    // PDF
    // ==========================================================

    Task<DocumentoGeneradoDto> GenerarPdfBoletaPagoAsync(Guid comprobanteId);

    Task<DocumentoGeneradoDto> GenerarPdfConstanciaCitaAsync(Guid comprobanteId);

    Task<DocumentoGeneradoDto> GenerarPdfResumenAtencionAsync(Guid comprobanteId);

    Task<DocumentoGeneradoDto> GenerarPdfEstadoCuentaPacienteAsync(Guid comprobanteId);

    // ==========================================================
    // CONSULTAS
    // ==========================================================

    Task<ComprobanteDto> ObtenerPorIdAsync(Guid id);

    Task<IEnumerable<ComprobanteDto>> ObtenerPorPacienteAsync(Guid pacienteId);

    Task<IEnumerable<ComprobanteDto>> ObtenerPorPagoAsync(Guid pagoId);

    Task<IEnumerable<ComprobanteDto>> ObtenerPorAtencionAsync(Guid atencionId);

    // ==========================================================
    // ANULACIÓN
    // ==========================================================

    Task AnularComprobanteAsync(Guid comprobanteId, string motivo);
}