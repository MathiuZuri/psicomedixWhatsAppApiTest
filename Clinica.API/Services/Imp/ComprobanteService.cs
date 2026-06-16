using System.Text.Json;
using Clinica.Domain.DTOs.Comprobantes;
using Clinica.Domain.Entities;
using Clinica.Domain.Enums;
using Clinica.Domain.Interfaces;

namespace Clinica.API.Services.Imp;

public class ComprobanteService : IComprobanteService
{
    private const decimal TasaIgv = 18m;

    private readonly IComprobanteRepository _comprobanteRepository;
    private readonly IPagoRepository _pagoRepository;
    private readonly IUsuarioActualService _usuarioActualService;
    private readonly IComprobantePdfService _comprobantePdfService;

    public ComprobanteService(
        IComprobanteRepository comprobanteRepository,
        IPagoRepository pagoRepository,
        IUsuarioActualService usuarioActualService,
        IComprobantePdfService comprobantePdfService)
    {
        _comprobanteRepository = comprobanteRepository;
        _pagoRepository = pagoRepository;
        _usuarioActualService = usuarioActualService;
        _comprobantePdfService = comprobantePdfService;
    }

    // ==========================================================
    // PREVIEW BOLETA DE PAGO
    // ==========================================================

    public async Task<ComprobantePagoPreviewDto> PreviewBoletaPagoAsync(Guid pagoId, decimal tasaImpuesto = TasaIgv)
    {
        if (pagoId == Guid.Empty)
            throw new InvalidOperationException("El identificador del pago es obligatorio.");

        var pago = await ObtenerPagoConDetallePorIdAsync(pagoId);

        var subtotal = CalcularSubtotalDesdeTotal(pago.MontoPagado, tasaImpuesto);
        var impuesto = pago.MontoPagado - subtotal;

        return new ComprobantePagoPreviewDto
        {
            CodigoComprobante = "PREVIEW",
            PagoId = pago.Id,
            CodigoPago = pago.CodigoPago,

            PacienteId = pago.PacienteId,
            Paciente = pago.Paciente == null ? "" : $"{pago.Paciente.Nombres} {pago.Paciente.Apellidos}",
            DniPaciente = pago.Paciente?.DNI ?? "",

            AtencionId = pago.AtencionId,
            CodigoAtencion = pago.Atencion?.CodigoAtencion,

            CitaId = pago.CitaId,
            CodigoCita = pago.Cita?.CodigoCita,

            Servicio = pago.ServicioClinico?.Nombre ?? "Servicio clínico",

            MontoPagado = pago.MontoPagado,
            Subtotal = subtotal,
            TasaImpuesto = tasaImpuesto,
            MontoImpuesto = impuesto,
            Total = pago.MontoPagado,

            MetodoPago = pago.MetodoPago.ToString(),
            EstadoPago = pago.Estado.ToString(),

            FechaPago = pago.FechaPago,
            FechaEmision = DateTime.UtcNow,

            Observacion = pago.Observacion,

            Detalles = new List<ComprobanteDetalleDto>
            {
                new()
                {
                    CodigoServicio = pago.ServicioClinico?.CodigoServicio ?? "",
                    Descripcion = pago.ServicioClinico?.Nombre ?? "Servicio clínico",
                    Cantidad = 1,
                    PrecioUnitarioFinal = pago.MontoPagado,
                    Subtotal = subtotal,
                    TasaImpuesto = tasaImpuesto,
                    MontoImpuesto = impuesto,
                    Total = pago.MontoPagado
                }
            }
        };
    }

    // ==========================================================
    // EMITIR BOLETA DE PAGO
    // ==========================================================

    public async Task<Guid> EmitirBoletaPagoAsync(EmitirComprobantePagoDto dto)
    {
        if (dto.PagoId == Guid.Empty && string.IsNullOrWhiteSpace(dto.CodigoPago))
            throw new InvalidOperationException("Debe enviar el identificador del pago o el código de pago.");

        Pago pago;

        if (!string.IsNullOrWhiteSpace(dto.CodigoPago))
        {
            pago = await _pagoRepository.ObtenerPorCodigoConDetalleAsync(dto.CodigoPago.Trim())
                   ?? throw new KeyNotFoundException("Pago no encontrado.");
        }
        else
        {
            pago = await ObtenerPagoConDetallePorIdAsync(dto.PagoId);
        }

        var usuarioId = _usuarioActualService.ObtenerUsuarioId();

        var serie = ObtenerSerie(TipoComprobante.BoletaPago);
        var ultimoNumero = await _comprobanteRepository.ObtenerUltimoNumeroPorSerieAsync(serie);
        var numero = ultimoNumero + 1;

        var subtotal = CalcularSubtotalDesdeTotal(pago.MontoPagado, TasaIgv);
        var impuesto = pago.MontoPagado - subtotal;

        var comprobante = new Comprobante
        {
            Id = Guid.NewGuid(),
            CodigoComprobante = $"{serie}-{numero:000000}",
            Serie = serie,
            Numero = numero,

            TipoComprobante = TipoComprobante.BoletaPago,
            Estado = EstadoComprobante.Emitido,
            FormatoImpresion = TipoFormatoImpresion.A4,

            PacienteId = pago.PacienteId,
            PagoId = pago.Id,
            CitaId = pago.CitaId,
            AtencionId = pago.AtencionId,
            HistorialClinicoId = pago.Atencion?.HistorialClinicoId,

            TipoDocumentoPaciente = TipoDocumentoComprobante.DNI,
            NumeroDocumentoPaciente = pago.Paciente?.DNI ?? "",
            NombrePaciente = pago.Paciente == null ? "" : $"{pago.Paciente.Nombres} {pago.Paciente.Apellidos}",
            DireccionPaciente = pago.Paciente?.Direccion,

            Subtotal = subtotal,
            TasaImpuesto = TasaIgv,
            MontoImpuesto = impuesto,
            Total = pago.MontoPagado,

            FechaEmision = DateTime.UtcNow,
            UsuarioEmisionId = usuarioId,

            Observacion = dto.Observacion?.Trim(),

            DatosSnapshotJson = JsonSerializer.Serialize(new
            {
                Tipo = "Boleta de pago",
                PagoId = pago.Id,
                CodigoPago = pago.CodigoPago,
                PacienteId = pago.PacienteId,
                Paciente = pago.Paciente == null ? "" : $"{pago.Paciente.Nombres} {pago.Paciente.Apellidos}",
                DniPaciente = pago.Paciente?.DNI ?? "",
                Servicio = pago.ServicioClinico?.Nombre ?? "Servicio clínico",
                pago.MontoTotal,
                pago.MontoPagado,
                pago.SaldoPendiente,
                MetodoPago = pago.MetodoPago.ToString(),
                EstadoPago = pago.Estado.ToString(),
                FechaPago = pago.FechaPago,
                TasaImpuesto = TasaIgv,
                Subtotal = subtotal,
                MontoImpuesto = impuesto,
                Total = pago.MontoPagado
            })
        };

        comprobante.Detalles.Add(new ComprobanteDetalle
        {
            Id = Guid.NewGuid(),
            ComprobanteId = comprobante.Id,

            CodigoServicio = pago.ServicioClinico?.CodigoServicio ?? "",
            Descripcion = pago.ServicioClinico?.Nombre ?? "Servicio clínico",
            Cantidad = 1,

            PrecioUnitarioFinal = pago.MontoPagado,
            Subtotal = subtotal,
            TasaImpuesto = TasaIgv,
            MontoImpuesto = impuesto,
            Total = pago.MontoPagado
        });

        await _comprobanteRepository.AddAsync(comprobante);
        await _comprobanteRepository.SaveChangesAsync();

        return comprobante.Id;
    }

    // ==========================================================
    // PDF
    // ==========================================================

    public async Task<DocumentoGeneradoDto> GenerarPdfBoletaPagoAsync(Guid comprobanteId)
    {
        if (comprobanteId == Guid.Empty)
            throw new InvalidOperationException("El identificador del comprobante es obligatorio.");

        var comprobante = await _comprobanteRepository.ObtenerPorIdConDetalleAsync(comprobanteId)
            ?? throw new KeyNotFoundException("Comprobante no encontrado.");

        if (comprobante.TipoComprobante != TipoComprobante.BoletaPago)
            throw new InvalidOperationException("El comprobante solicitado no corresponde a una boleta de pago.");

        if (comprobante.Estado == EstadoComprobante.Anulado)
            throw new InvalidOperationException("No se puede generar PDF de un comprobante anulado.");

        var preview = MapearPagoPreview(comprobante);
        var archivo = _comprobantePdfService.GenerarBoletaPagoPdf(preview);

        return new DocumentoGeneradoDto
        {
            NombreArchivo = $"{comprobante.CodigoComprobante}.pdf",
            ContentType = "application/pdf",
            Archivo = archivo
        };
    }
    
    public async Task<DocumentoGeneradoDto> GenerarPdfConstanciaCitaAsync(Guid comprobanteId)
    {
        if (comprobanteId == Guid.Empty)
            throw new InvalidOperationException("El identificador del comprobante es obligatorio.");

        var comprobante = await _comprobanteRepository.ObtenerPorIdConDetalleAsync(comprobanteId)
            ?? throw new KeyNotFoundException("Comprobante no encontrado.");

        if (comprobante.TipoComprobante != TipoComprobante.ConstanciaCita)
            throw new InvalidOperationException("El comprobante solicitado no corresponde a una constancia de cita.");

        if (comprobante.Estado == EstadoComprobante.Anulado)
            throw new InvalidOperationException("No se puede generar PDF de un comprobante anulado.");

        var preview = MapearCitaPreview(comprobante);
        var archivo = _comprobantePdfService.GenerarConstanciaCitaPdf(preview);

        return new DocumentoGeneradoDto
        {
            NombreArchivo = $"{comprobante.CodigoComprobante}.pdf",
            ContentType = "application/pdf",
            Archivo = archivo
        };
    }

    public async Task<DocumentoGeneradoDto> GenerarPdfResumenAtencionAsync(Guid comprobanteId)
    {
        if (comprobanteId == Guid.Empty)
            throw new InvalidOperationException("El identificador del comprobante es obligatorio.");

        var comprobante = await _comprobanteRepository.ObtenerPorIdConDetalleAsync(comprobanteId)
            ?? throw new KeyNotFoundException("Comprobante no encontrado.");

        if (comprobante.TipoComprobante != TipoComprobante.ResumenAtencion)
            throw new InvalidOperationException("El comprobante solicitado no corresponde a un resumen de atención.");

        if (comprobante.Estado == EstadoComprobante.Anulado)
            throw new InvalidOperationException("No se puede generar PDF de un comprobante anulado.");

        var preview = MapearAtencionPreview(comprobante);
        var archivo = _comprobantePdfService.GenerarResumenAtencionPdf(preview);

        return new DocumentoGeneradoDto
        {
            NombreArchivo = $"{comprobante.CodigoComprobante}.pdf",
            ContentType = "application/pdf",
            Archivo = archivo
        };
    }

    public async Task<DocumentoGeneradoDto> GenerarPdfEstadoCuentaPacienteAsync(Guid comprobanteId)
    {
        if (comprobanteId == Guid.Empty)
            throw new InvalidOperationException("El identificador del comprobante es obligatorio.");

        var comprobante = await _comprobanteRepository.ObtenerPorIdConDetalleAsync(comprobanteId)
            ?? throw new KeyNotFoundException("Comprobante no encontrado.");

        if (comprobante.TipoComprobante != TipoComprobante.EstadoCuenta)
            throw new InvalidOperationException("El comprobante solicitado no corresponde a un estado de cuenta.");

        if (comprobante.Estado == EstadoComprobante.Anulado)
            throw new InvalidOperationException("No se puede generar PDF de un comprobante anulado.");

        var preview = MapearEstadoCuentaPreview(comprobante);
        var archivo = _comprobantePdfService.GenerarEstadoCuentaPacientePdf(preview);

        return new DocumentoGeneradoDto
        {
            NombreArchivo = $"{comprobante.CodigoComprobante}.pdf",
            ContentType = "application/pdf",
            Archivo = archivo
        };
    }

    // ==========================================================
    // CONSULTAS
    // ==========================================================

    public async Task<ComprobanteDto> ObtenerPorIdAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new InvalidOperationException("El identificador del comprobante es obligatorio.");

        var comprobante = await _comprobanteRepository.ObtenerPorIdConDetalleAsync(id)
            ?? throw new KeyNotFoundException("Comprobante no encontrado.");

        return MapearComprobante(comprobante);
    }

    public async Task<IEnumerable<ComprobanteDto>> ObtenerPorPacienteAsync(Guid pacienteId)
    {
        if (pacienteId == Guid.Empty)
            throw new InvalidOperationException("El identificador del paciente es obligatorio.");

        var comprobantes = await _comprobanteRepository.ObtenerPorPacienteAsync(pacienteId);
        return comprobantes.Select(MapearComprobante).ToList();
    }

    public async Task<IEnumerable<ComprobanteDto>> ObtenerPorPagoAsync(Guid pagoId)
    {
        if (pagoId == Guid.Empty)
            throw new InvalidOperationException("El identificador del pago es obligatorio.");

        var comprobantes = await _comprobanteRepository.ObtenerPorPagoAsync(pagoId);
        return comprobantes.Select(MapearComprobante).ToList();
    }

    public async Task<IEnumerable<ComprobanteDto>> ObtenerPorAtencionAsync(Guid atencionId)
    {
        if (atencionId == Guid.Empty)
            throw new InvalidOperationException("El identificador de la atención es obligatorio.");

        var comprobantes = await _comprobanteRepository.ObtenerPorAtencionAsync(atencionId);
        return comprobantes.Select(MapearComprobante).ToList();
    }

    // ==========================================================
    // ANULACIÓN
    // ==========================================================

    public async Task AnularComprobanteAsync(Guid comprobanteId, string motivo)
    {
        if (comprobanteId == Guid.Empty)
            throw new InvalidOperationException("El identificador del comprobante es obligatorio.");

        if (string.IsNullOrWhiteSpace(motivo))
            throw new InvalidOperationException("El motivo de anulación es obligatorio.");

        var comprobante = await _comprobanteRepository.GetByIdAsync(comprobanteId)
            ?? throw new KeyNotFoundException("Comprobante no encontrado.");

        if (comprobante.Estado == EstadoComprobante.Anulado)
            throw new InvalidOperationException("El comprobante ya se encuentra anulado.");

        comprobante.Estado = EstadoComprobante.Anulado;
        comprobante.FechaAnulacion = DateTime.UtcNow;
        comprobante.UsuarioAnulacionId = _usuarioActualService.ObtenerUsuarioId();
        comprobante.MotivoAnulacion = motivo.Trim();

        _comprobanteRepository.Update(comprobante);
        await _comprobanteRepository.SaveChangesAsync();
    }

    // ==========================================================
    // MÉTODOS TEMPORALES PARA SIGUIENTES DOCUMENTOS
    // ==========================================================

    public Task<ComprobanteCitaPreviewDto> PreviewConstanciaCitaAsync(Guid citaId)
    {
        throw new NotImplementedException("La vista previa de constancia de cita se implementará en la siguiente fase.");
    }

    public Task<Guid> EmitirConstanciaCitaAsync(EmitirComprobanteCitaDto dto)
    {
        throw new NotImplementedException("La emisión de constancia de cita se implementará en la siguiente fase.");
    }

    public Task<ComprobanteAtencionPreviewDto> PreviewResumenAtencionAsync(Guid atencionId)
    {
        throw new NotImplementedException("La vista previa de resumen de atención se implementará en la siguiente fase.");
    }

    public Task<Guid> EmitirResumenAtencionAsync(EmitirComprobanteAtencionDto dto)
    {
        throw new NotImplementedException("La emisión de resumen de atención se implementará en la siguiente fase.");
    }

    public Task<ComprobanteEstadoCuentaPreviewDto> PreviewEstadoCuentaPacienteAsync(Guid pacienteId)
    {
        throw new NotImplementedException("El estado de cuenta del paciente se implementará en la siguiente fase.");
    }

    // ==========================================================
    // MÉTODOS PRIVADOS
    // ==========================================================

    private async Task<Pago> ObtenerPagoConDetallePorIdAsync(Guid pagoId)
    {
        var pagos = await _pagoRepository.ObtenerTodosConDetalleAsync();

        return pagos.FirstOrDefault(x => x.Id == pagoId)
               ?? throw new KeyNotFoundException("Pago no encontrado.");
    }

    private static string ObtenerSerie(TipoComprobante tipo)
    {
        return tipo switch
        {
            TipoComprobante.BoletaPago => "B001",
            TipoComprobante.ConstanciaCita => "C001",
            TipoComprobante.ResumenAtencion => "A001",
            TipoComprobante.EstadoCuenta => "E001",
            TipoComprobante.HistoriaClinica => "H001",
            _ => "D001"
        };
    }

    private static decimal CalcularSubtotalDesdeTotal(decimal total, decimal tasaImpuesto)
    {
        return Math.Round(total / (1 + tasaImpuesto / 100), 2);
    }

    private static ComprobantePagoPreviewDto MapearPagoPreview(Comprobante comprobante)
    {
        return new ComprobantePagoPreviewDto
        {
            CodigoComprobante = comprobante.CodigoComprobante,

            PagoId = comprobante.PagoId ?? Guid.Empty,
            CodigoPago = comprobante.Pago?.CodigoPago ?? "",

            PacienteId = comprobante.PacienteId,
            Paciente = comprobante.NombrePaciente,
            DniPaciente = comprobante.NumeroDocumentoPaciente,

            AtencionId = comprobante.AtencionId,
            CodigoAtencion = comprobante.Atencion?.CodigoAtencion,

            CitaId = comprobante.CitaId,
            CodigoCita = comprobante.Cita?.CodigoCita,

            Servicio = comprobante.Detalles.FirstOrDefault()?.Descripcion ?? "Servicio clínico",

            MontoPagado = comprobante.Total,
            Subtotal = comprobante.Subtotal,
            TasaImpuesto = comprobante.TasaImpuesto,
            MontoImpuesto = comprobante.MontoImpuesto,
            Total = comprobante.Total,

            MetodoPago = comprobante.Pago?.MetodoPago.ToString() ?? "",
            EstadoPago = comprobante.Pago?.Estado.ToString() ?? "",

            FechaPago = comprobante.Pago?.FechaPago ?? comprobante.FechaEmision,
            FechaEmision = comprobante.FechaEmision,

            Observacion = comprobante.Observacion,

            Detalles = comprobante.Detalles.Select(d => new ComprobanteDetalleDto
            {
                Id = d.Id,
                CodigoServicio = d.CodigoServicio,
                Descripcion = d.Descripcion,
                Cantidad = d.Cantidad,
                PrecioUnitarioFinal = d.PrecioUnitarioFinal,
                Subtotal = d.Subtotal,
                TasaImpuesto = d.TasaImpuesto,
                MontoImpuesto = d.MontoImpuesto,
                Total = d.Total
            }).ToList()
        };
    }

    private static ComprobanteDto MapearComprobante(Comprobante x)
    {
        return new ComprobanteDto
        {
            Id = x.Id,
            CodigoComprobante = x.CodigoComprobante,
            Serie = x.Serie,
            Numero = x.Numero,

            TipoComprobante = x.TipoComprobante.ToString(),
            Estado = x.Estado.ToString(),
            FormatoImpresion = x.FormatoImpresion.ToString(),

            PacienteId = x.PacienteId,
            Paciente = x.NombrePaciente,

            TipoDocumentoPaciente = x.TipoDocumentoPaciente.ToString(),
            NumeroDocumentoPaciente = x.NumeroDocumentoPaciente,
            DireccionPaciente = x.DireccionPaciente,

            PagoId = x.PagoId,
            CitaId = x.CitaId,
            AtencionId = x.AtencionId,
            HistorialClinicoId = x.HistorialClinicoId,

            Subtotal = x.Subtotal,
            TasaImpuesto = x.TasaImpuesto,
            MontoImpuesto = x.MontoImpuesto,
            Total = x.Total,

            FechaEmision = x.FechaEmision,

            UsuarioEmisionId = x.UsuarioEmisionId,
            UsuarioEmision = x.UsuarioEmision == null
                ? null
                : $"{x.UsuarioEmision.Nombres} {x.UsuarioEmision.Apellidos}",

            FechaAnulacion = x.FechaAnulacion,
            UsuarioAnulacionId = x.UsuarioAnulacionId,
            UsuarioAnulacion = x.UsuarioAnulacion == null
                ? null
                : $"{x.UsuarioAnulacion.Nombres} {x.UsuarioAnulacion.Apellidos}",

            Observacion = x.Observacion,
            MotivoAnulacion = x.MotivoAnulacion,

            Detalles = x.Detalles.Select(d => new ComprobanteDetalleDto
            {
                Id = d.Id,
                CodigoServicio = d.CodigoServicio,
                Descripcion = d.Descripcion,
                Cantidad = d.Cantidad,
                PrecioUnitarioFinal = d.PrecioUnitarioFinal,
                Subtotal = d.Subtotal,
                TasaImpuesto = d.TasaImpuesto,
                MontoImpuesto = d.MontoImpuesto,
                Total = d.Total
            }).ToList()
        };
    }
    
    private static ComprobanteCitaPreviewDto MapearCitaPreview(Comprobante comprobante)
    {
        var cita = comprobante.Cita;

        return new ComprobanteCitaPreviewDto
        {
            ComprobanteId = comprobante.Id,
            CodigoComprobante = comprobante.CodigoComprobante,

            CitaId = comprobante.CitaId ?? Guid.Empty,
            CodigoCita = cita?.CodigoCita ?? "",

            PacienteId = comprobante.PacienteId,
            Paciente = comprobante.NombrePaciente,
            DniPaciente = comprobante.NumeroDocumentoPaciente,
            DireccionPaciente = comprobante.DireccionPaciente,

            DoctorId = cita?.DoctorId ?? Guid.Empty,
            Doctor = cita?.Doctor == null ? "" : $"{cita.Doctor.Nombres} {cita.Doctor.Apellidos}",
            Especialidad = cita?.Doctor?.Especialidad ?? "",

            ServicioClinicoId = cita?.ServicioClinicoId ?? Guid.Empty,
            Servicio = cita?.ServicioClinico?.Nombre ?? "Servicio clínico",

            FechaCita = cita?.Fecha ?? default,
            HoraInicio = cita?.HoraInicio ?? default,
            HoraFin = cita?.HoraFin ?? default,

            EstadoCita = cita?.Estado.ToString() ?? "",
            Motivo = cita?.Motivo ?? "",

            FechaEmision = comprobante.FechaEmision,
            Observacion = comprobante.Observacion
        };
    }

    private static ComprobanteAtencionPreviewDto MapearAtencionPreview(Comprobante comprobante)
    {
        var atencion = comprobante.Atencion;

        return new ComprobanteAtencionPreviewDto
        {
            ComprobanteId = comprobante.Id,
            CodigoComprobante = comprobante.CodigoComprobante,

            AtencionId = comprobante.AtencionId ?? Guid.Empty,
            CodigoAtencion = atencion?.CodigoAtencion ?? "",

            PacienteId = comprobante.PacienteId,
            Paciente = comprobante.NombrePaciente,
            DniPaciente = comprobante.NumeroDocumentoPaciente,
            DireccionPaciente = comprobante.DireccionPaciente,

            DoctorId = atencion?.DoctorId ?? Guid.Empty,
            Doctor = atencion?.Doctor == null ? "" : $"{atencion.Doctor.Nombres} {atencion.Doctor.Apellidos}",
            Especialidad = atencion?.Doctor?.Especialidad ?? "",

            ServicioClinicoId = atencion?.ServicioClinicoId ?? Guid.Empty,
            Servicio = atencion?.ServicioClinico?.Nombre ?? "Servicio clínico",

            FechaInicio = atencion?.FechaInicio ?? comprobante.FechaEmision,
            FechaCierre = atencion?.FechaCierre,

            MotivoConsulta = atencion?.MotivoConsulta ?? "",
            DiagnosticoResumen = atencion?.DiagnosticoResumen,
            Indicaciones = atencion?.Indicaciones,
            Tratamiento = atencion?.Tratamiento,
            Observaciones = atencion?.Observaciones,

            EstadoAtencion = atencion?.Estado.ToString() ?? "",

            CostoFinal = atencion?.CostoFinal ?? 0,
            MontoPagado = atencion?.MontoPagado ?? 0,
            SaldoPendiente = atencion?.SaldoPendiente ?? 0,

            FechaEmision = comprobante.FechaEmision
        };
    }

    private static ComprobanteEstadoCuentaPreviewDto MapearEstadoCuentaPreview(Comprobante comprobante)
    {
        return new ComprobanteEstadoCuentaPreviewDto
        {
            ComprobanteId = comprobante.Id,
            CodigoComprobante = comprobante.CodigoComprobante,

            PacienteId = comprobante.PacienteId,
            Paciente = comprobante.NombrePaciente,
            DniPaciente = comprobante.NumeroDocumentoPaciente,
            DireccionPaciente = comprobante.DireccionPaciente,

            TotalFacturado = comprobante.Total,
            TotalPagado = comprobante.Total,
            TotalPendiente = 0,

            FechaEmision = comprobante.FechaEmision,

            Detalles = new List<DetalleEstadoCuentaComprobanteDto>()
        };
    }
}