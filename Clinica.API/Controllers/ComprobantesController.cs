using Clinica.Domain.DTOs.Comprobantes;
using Clinica.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Clinica.API.Models;

namespace Clinica.API.Controllers;

/// <summary>
/// Controlador para la emisión, generación de PDF y gestión de comprobantes del sistema (boletas de pago, constancias de cita, etc.).
/// </summary>
/// <remarks>
/// **Nota de Arquitectura:** Los comprobantes son documentos fiscales y clínicos que reflejan transacciones reales. 
/// No pueden ser eliminados físicamente; solo pueden ser anulados con un motivo justificado, lo que deja una trazabilidad completa.
/// La generación de PDF se realiza mediante la librería QuestPDF, que permite crear documentos profesionales de forma programática.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Authorize] // Todos los endpoints requieren autenticación
[Tags("Comprobantes y Documentos")]
public class ComprobantesController : ControllerBase
{
    private readonly IComprobanteService _comprobanteService;

    public ComprobantesController(IComprobanteService comprobanteService)
    {
        _comprobanteService = comprobanteService;
    }

    // ==========================================================
    // PREVIEWS (Vistas previas)
    // ==========================================================

    /// <summary>
    /// Genera una vista previa de la boleta de pago para un pago específico.
    /// </summary>
    /// <remarks>
    /// **Propósito:** 
    /// Permite al usuario visualizar cómo quedará la boleta de pago antes de emitirla formalmente.
    /// 
    /// **Datos incluidos en la vista previa:**
    /// - Información del paciente (nombre, DNI).
    /// - Detalles del pago (código, monto).
    /// - Servicio clínico asociado.
    /// - Desglose de impuestos (IGV) y subtotal.
    /// - Método de pago y estado del pago.
    /// </remarks>
    /// <param name="pagoId">Identificador único del pago (GUID).</param>
    /// <returns>Objeto <see cref="ComprobantePagoPreviewDto"/> con los datos de la vista previa.</returns>
    /// <response code="200">Vista previa generada correctamente.</response>
    /// <response code="400">El identificador del pago es inválido.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="404">Pago no encontrado.</response>
    [HttpGet("preview/boleta-pago/{pagoId:guid}")]
    [ProducesResponseType(typeof(ComprobantePagoPreviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ComprobantePagoPreviewDto>> PreviewBoletaPago(Guid pagoId)
    {
        var resultado = await _comprobanteService.PreviewBoletaPagoAsync(pagoId);
        return Ok(resultado);
    }

    /// <summary>
    /// Genera una vista previa de la constancia de cita para una cita específica.
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Permite al usuario visualizar cómo quedará la constancia de cita antes de emitirla formalmente.
    /// 
    /// **Datos incluidos:**
    /// - Información del paciente.
    /// - Detalles de la cita (fecha, hora, doctor, servicio).
    /// - Estado de la cita.
    /// </remarks>
    /// <param name="citaId">Identificador único de la cita (GUID).</param>
    /// <returns>Objeto <see cref="ComprobanteCitaPreviewDto"/> con los datos de la vista previa.</returns>
    /// <response code="200">Vista previa generada correctamente.</response>
    /// <response code="400">El identificador de la cita es inválido.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="404">Cita no encontrada.</response>
    [HttpGet("preview/constancia-cita/{citaId:guid}")]
    [ProducesResponseType(typeof(ComprobanteCitaPreviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ComprobanteCitaPreviewDto>> PreviewConstanciaCita(Guid citaId)
    {
        var resultado = await _comprobanteService.PreviewConstanciaCitaAsync(citaId);
        return Ok(resultado);
    }

    /// <summary>
    /// Genera una vista previa del resumen de atención para una atención específica.
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Permite al usuario visualizar cómo quedará el resumen de atención antes de emitirlo formalmente.
    /// 
    /// **Datos incluidos:**
    /// - Información del paciente.
    /// - Detalles de la atención (doctor, servicio, fechas, diagnóstico).
    /// - Costos y pagos asociados.
    /// </remarks>
    /// <param name="atencionId">Identificador único de la atención (GUID).</param>
    /// <returns>Objeto <see cref="ComprobanteAtencionPreviewDto"/> con los datos de la vista previa.</returns>
    /// <response code="200">Vista previa generada correctamente.</response>
    /// <response code="400">El identificador de la atención es inválido.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="404">Atención no encontrada.</response>
    [HttpGet("preview/resumen-atencion/{atencionId:guid}")]
    [ProducesResponseType(typeof(ComprobanteAtencionPreviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ComprobanteAtencionPreviewDto>> PreviewResumenAtencion(Guid atencionId)
    {
        var resultado = await _comprobanteService.PreviewResumenAtencionAsync(atencionId);
        return Ok(resultado);
    }

    /// <summary>
    /// Genera una vista previa del estado de cuenta para un paciente específico.
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Permite al usuario visualizar cómo quedará el estado de cuenta del paciente antes de emitirlo formalmente.
    /// 
    /// **Datos incluidos:**
    /// - Información del paciente.
    /// - Total facturado, total pagado y total pendiente.
    /// - Detalle de los movimientos financieros.
    /// </remarks>
    /// <param name="pacienteId">Identificador único del paciente (GUID).</param>
    /// <returns>Objeto <see cref="ComprobanteEstadoCuentaPreviewDto"/> con los datos de la vista previa.</returns>
    /// <response code="200">Vista previa generada correctamente.</response>
    /// <response code="400">El identificador del paciente es inválido.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="404">Paciente no encontrado.</response>
    [HttpGet("preview/estado-cuenta/paciente/{pacienteId:guid}")]
    [ProducesResponseType(typeof(ComprobanteEstadoCuentaPreviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ComprobanteEstadoCuentaPreviewDto>> PreviewEstadoCuentaPaciente(Guid pacienteId)
    {
        var resultado = await _comprobanteService.PreviewEstadoCuentaPacienteAsync(pacienteId);
        return Ok(resultado);
    }

    // ==========================================================
    // EMISIÓN
    // ==========================================================

    /// <summary>
    /// Emite formalmente una boleta de pago.
    /// </summary>
    /// <remarks>
    /// **Proceso de negocio:**
    /// 1. Valida que el pago exista (por ID o código de pago).
    /// 2. Calcula el subtotal y el impuesto (IGV).
    /// 3. Asigna un número de serie único y correlativo para el comprobante.
    /// 4. Guarda el comprobante en la base de datos junto con sus detalles y un snapshot JSON de los datos.
    /// 
    /// **Resultado:** La boleta queda emitida y lista para ser descargada en PDF.
    /// </remarks>
    /// <param name="dto">Objeto <see cref="EmitirComprobantePagoDto"/> con los datos para la emisión.</param>
    /// <returns>Objeto con el ID del comprobante emitido.</returns>
    /// <response code="200">Boleta de pago emitida correctamente.</response>
    /// <response code="400">Datos inválidos para la emisión.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="404">Pago no encontrado.</response>
    [HttpPost("emitir/boleta-pago")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> EmitirBoletaPago([FromBody] EmitirComprobantePagoDto dto)
    {
        var comprobanteId = await _comprobanteService.EmitirBoletaPagoAsync(dto);

        return Ok(new
        {
            Mensaje = "Boleta de pago emitida correctamente.",
            ComprobanteId = comprobanteId
        });
    }

    /// <summary>
    /// Emite formalmente una constancia de cita.
    /// </summary>
    /// <remarks>
    /// **Proceso de negocio:**
    /// 1. Valida que la cita exista.
    /// 2. Asigna un número de serie único y correlativo.
    /// 3. Guarda el comprobante en la base de datos.
    /// 
    /// **Resultado:** La constancia queda emitida y lista para ser descargada en PDF.
    /// </remarks>
    /// <param name="dto">Objeto <see cref="EmitirComprobanteCitaDto"/> con los datos para la emisión.</param>
    /// <returns>Objeto con el ID del comprobante emitido.</returns>
    /// <response code="200">Constancia de cita emitida correctamente.</response>
    /// <response code="400">Datos inválidos para la emisión.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="404">Cita no encontrada.</response>
    [HttpPost("emitir/constancia-cita")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> EmitirConstanciaCita([FromBody] EmitirComprobanteCitaDto dto)
    {
        var comprobanteId = await _comprobanteService.EmitirConstanciaCitaAsync(dto);

        return Ok(new
        {
            Mensaje = "Constancia de cita emitida correctamente.",
            ComprobanteId = comprobanteId
        });
    }

    /// <summary>
    /// Emite formalmente un resumen de atención.
    /// </summary>
    /// <remarks>
    /// **Proceso de negocio:**
    /// 1. Valida que la atención exista.
    /// 2. Asigna un número de serie único y correlativo.
    /// 3. Guarda el comprobante en la base de datos.
    /// 
    /// **Resultado:** El resumen queda emitido y lista para ser descargado en PDF.
    /// </remarks>
    /// <param name="dto">Objeto <see cref="EmitirComprobanteAtencionDto"/> con los datos para la emisión.</param>
    /// <returns>Objeto con el ID del comprobante emitido.</returns>
    /// <response code="200">Resumen de atención emitido correctamente.</response>
    /// <response code="400">Datos inválidos para la emisión.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="404">Atención no encontrada.</response>
    [HttpPost("emitir/resumen-atencion")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> EmitirResumenAtencion([FromBody] EmitirComprobanteAtencionDto dto)
    {
        var comprobanteId = await _comprobanteService.EmitirResumenAtencionAsync(dto);

        return Ok(new
        {
            Mensaje = "Resumen de atención emitido correctamente.",
            ComprobanteId = comprobanteId
        });
    }

    // ==========================================================
    // PDF
    // ==========================================================

    /// <summary>
    /// Genera y descarga el PDF de una boleta de pago emitida.
    /// </summary>
    /// <remarks>
    /// **Proceso:**
    /// 1. Valida que el comprobante exista y sea del tipo "BoletaPago".
    /// 2. Valida que el comprobante no esté anulado.
    /// 3. Genera el documento PDF mediante QuestPDF.
    /// 4. Retorna el archivo PDF para su descarga.
    /// </remarks>
    /// <param name="comprobanteId">Identificador único del comprobante (GUID).</param>
    /// <returns>Archivo PDF para descargar.</returns>
    /// <response code="200">PDF generado y descargado correctamente.</response>
    /// <response code="400">El comprobante no es de tipo BoletaPago o está anulado.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="404">Comprobante no encontrado.</response>
    [HttpGet("{comprobanteId:guid}/pdf/boleta-pago")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GenerarPdfBoletaPago(Guid comprobanteId)
    {
        var documento = await _comprobanteService.GenerarPdfBoletaPagoAsync(comprobanteId);

        return File(
            documento.Archivo,
            documento.ContentType,
            documento.NombreArchivo
        );
    }

    /// <summary>
    /// Genera y descarga el PDF de una constancia de cita emitida.
    /// </summary>
    /// <remarks>
    /// **Proceso:**
    /// 1. Valida que el comprobante exista y sea del tipo "ConstanciaCita".
    /// 2. Valida que el comprobante no esté anulado.
    /// 3. Genera el documento PDF.
    /// 4. Retorna el archivo PDF para su descarga.
    /// </remarks>
    /// <param name="comprobanteId">Identificador único del comprobante (GUID).</param>
    /// <returns>Archivo PDF para descargar.</returns>
    /// <response code="200">PDF generado y descargado correctamente.</response>
    /// <response code="400">El comprobante no es de tipo ConstanciaCita o está anulado.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="404">Comprobante no encontrado.</response>
    [HttpGet("{comprobanteId:guid}/pdf/constancia-cita")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GenerarPdfConstanciaCita(Guid comprobanteId)
    {
        var documento = await _comprobanteService.GenerarPdfConstanciaCitaAsync(comprobanteId);

        return File(
            documento.Archivo,
            documento.ContentType,
            documento.NombreArchivo
        );
    }

    /// <summary>
    /// Genera y descarga el PDF de un resumen de atención emitido.
    /// </summary>
    /// <remarks>
    /// **Proceso:**
    /// 1. Valida que el comprobante exista y sea del tipo "ResumenAtencion".
    /// 2. Valida que el comprobante no esté anulado.
    /// 3. Genera el documento PDF.
    /// 4. Retorna el archivo PDF para su descarga.
    /// </remarks>
    /// <param name="comprobanteId">Identificador único del comprobante (GUID).</param>
    /// <returns>Archivo PDF para descargar.</returns>
    /// <response code="200">PDF generado y descargado correctamente.</response>
    /// <response code="400">El comprobante no es de tipo ResumenAtencion o está anulado.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="404">Comprobante no encontrado.</response>
    [HttpGet("{comprobanteId:guid}/pdf/resumen-atencion")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GenerarPdfResumenAtencion(Guid comprobanteId)
    {
        var documento = await _comprobanteService.GenerarPdfResumenAtencionAsync(comprobanteId);

        return File(
            documento.Archivo,
            documento.ContentType,
            documento.NombreArchivo
        );
    }

    /// <summary>
    /// Genera y descarga el PDF de un estado de cuenta de paciente emitido.
    /// </summary>
    /// <remarks>
    /// **Proceso:**
    /// 1. Valida que el comprobante exista y sea del tipo "EstadoCuenta".
    /// 2. Valida que el comprobante no esté anulado.
    /// 3. Genera el documento PDF.
    /// 4. Retorna el archivo PDF para su descarga.
    /// </remarks>
    /// <param name="comprobanteId">Identificador único del comprobante (GUID).</param>
    /// <returns>Archivo PDF para descargar.</returns>
    /// <response code="200">PDF generado y descargado correctamente.</response>
    /// <response code="400">El comprobante no es de tipo EstadoCuenta o está anulado.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="404">Comprobante no encontrado.</response>
    [HttpGet("{comprobanteId:guid}/pdf/estado-cuenta")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GenerarPdfEstadoCuenta(Guid comprobanteId)
    {
        var documento = await _comprobanteService.GenerarPdfEstadoCuentaPacienteAsync(comprobanteId);

        return File(
            documento.Archivo,
            documento.ContentType,
            documento.NombreArchivo
        );
    }

    // ==========================================================
    // CONSULTAS
    // ==========================================================

    /// <summary>
    /// Obtiene los datos de un comprobante por su ID.
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Permite recuperar la información completa de un comprobante para visualización o validación.
    /// Incluye los detalles del comprobante (líneas de servicio, montos, etc.).
    /// </remarks>
    /// <param name="id">Identificador único del comprobante (GUID).</param>
    /// <returns>Objeto <see cref="ComprobanteDto"/> con los datos del comprobante.</returns>
    /// <response code="200">Comprobante obtenido correctamente.</response>
    /// <response code="400">El identificador del comprobante es inválido.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="404">Comprobante no encontrado.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ComprobanteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ComprobanteDto>> ObtenerPorId(Guid id)
    {
        var resultado = await _comprobanteService.ObtenerPorIdAsync(id);
        return Ok(resultado);
    }

    /// <summary>
    /// Obtiene todos los comprobantes emitidos para un paciente específico.
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Permite consultar el historial completo de comprobantes de un paciente.
    /// Útil para la ficha del paciente o para auditoría financiera.
    /// </remarks>
    /// <param name="pacienteId">Identificador único del paciente (GUID).</param>
    /// <returns>Lista de objetos <see cref="ComprobanteDto"/>.</returns>
    /// <response code="200">Comprobantes obtenidos correctamente.</response>
    /// <response code="400">El identificador del paciente es inválido.</response>
    /// <response code="401">No autorizado.</response>
    [HttpGet("paciente/{pacienteId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<ComprobanteDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<ComprobanteDto>>> ObtenerPorPaciente(Guid pacienteId)
    {
        var resultado = await _comprobanteService.ObtenerPorPacienteAsync(pacienteId);
        return Ok(resultado);
    }

    /// <summary>
    /// Obtiene todos los comprobantes asociados a un pago específico.
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Permite consultar los comprobantes que se han emitido para un pago determinado.
    /// Útil para validar la emisión de boletas de pago.
    /// </remarks>
    /// <param name="pagoId">Identificador único del pago (GUID).</param>
    /// <returns>Lista de objetos <see cref="ComprobanteDto"/>.</returns>
    /// <response code="200">Comprobantes obtenidos correctamente.</response>
    /// <response code="400">El identificador del pago es inválido.</response>
    /// <response code="401">No autorizado.</response>
    [HttpGet("pago/{pagoId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<ComprobanteDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<ComprobanteDto>>> ObtenerPorPago(Guid pagoId)
    {
        var resultado = await _comprobanteService.ObtenerPorPagoAsync(pagoId);
        return Ok(resultado);
    }

    /// <summary>
    /// Obtiene todos los comprobantes asociados a una atención médica específica.
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Permite consultar los comprobantes que se han emitido en el contexto de una atención médica.
    /// Útil para el seguimiento financiero de una atención.
    /// </remarks>
    /// <param name="atencionId">Identificador único de la atención (GUID).</param>
    /// <returns>Lista de objetos <see cref="ComprobanteDto"/>.</returns>
    /// <response code="200">Comprobantes obtenidos correctamente.</response>
    /// <response code="400">El identificador de la atención es inválido.</response>
    /// <response code="401">No autorizado.</response>
    [HttpGet("atencion/{atencionId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<ComprobanteDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<ComprobanteDto>>> ObtenerPorAtencion(Guid atencionId)
    {
        var resultado = await _comprobanteService.ObtenerPorAtencionAsync(atencionId);
        return Ok(resultado);
    }

    // ==========================================================
    // ANULACIÓN
    // ==========================================================

    /// <summary>
    /// Anula un comprobante emitido.
    /// </summary>
    /// <remarks>
    /// **Proceso de negocio:**
    /// 1. Valida que el comprobante exista y no esté ya anulado.
    /// 2. Cambia el estado del comprobante a "Anulado".
    /// 3. Registra el motivo de la anulación y el usuario que realiza la acción.
    /// 
    /// **Nota de Arquitectura:** Los comprobantes anulados no se eliminan físicamente; se conservan para mantener la trazabilidad fiscal y contable.
    /// </remarks>
    /// <param name="comprobanteId">Identificador único del comprobante a anular (GUID).</param>
    /// <param name="request">Objeto con el motivo de anulación.</param>
    /// <response code="200">Comprobante anulado correctamente.</response>
    /// <response code="400">El comprobante ya está anulado o el motivo es inválido.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="404">Comprobante no encontrado.</response>
    [HttpPut("{comprobanteId:guid}/anular")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> AnularComprobante(
        Guid comprobanteId,
        [FromBody] AnularComprobanteRequest request)
    {
        await _comprobanteService.AnularComprobanteAsync(comprobanteId, request.Motivo);

        return Ok(new
        {
            Mensaje = "Comprobante anulado correctamente."
        });
    }
}

/// <summary>
/// Objeto utilizado para solicitar la anulación de un comprobante.
/// </summary>
public class AnularComprobanteRequest
{
    /// <summary>
    /// Motivo de la anulación del comprobante. (Campo obligatorio y debe tener al menos 3 caracteres).
    /// </summary>
    /// <example>"Error en el monto registrado"</example>
    public string Motivo { get; set; } = string.Empty;
}