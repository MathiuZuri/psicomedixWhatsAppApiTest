using Clinica.API.Authorization;
using Clinica.API.Models;
using Clinica.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Clinica.Domain.DTOs.Finanzas;
using Clinica.API.Filters;
using Clinica.Domain.Enums;

namespace Clinica.API.Controllers;

/// <summary>
/// Controlador para la gestión financiera y contable del sistema.
/// </summary>
/// <remarks>
/// **Módulo Financiero:** Este controlador agrupa todos los endpoints relacionados con la gestión de ingresos, pagos, deudas, estados de cuenta y ajustes financieros.
/// 
/// **Nota de Arquitectura:** Todos los endpoints de este controlador requieren el permiso <see cref="PermisosPolicies.FinanzasVer"/>.
/// Las operaciones de ajuste financiero (creación) requieren <see cref="PermisosPolicies.PagoRegistrar"/>.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Tags("Finanzas y Contabilidad")]
public class FinanzasController : ControllerBase
{
    private readonly IFinanzasService _finanzasService;

    public FinanzasController(IFinanzasService finanzasService)
    {
        _finanzasService = finanzasService;
    }

    // ==========================================================
    // RESÚMENES
    // ==========================================================

    /// <summary>
    /// Obtiene el resumen diario de finanzas (ingresos, pagos, deudas).
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Proporciona un panorama general de la actividad financiera de un día específico.
    /// Útil para el cierre de caja diario y la conciliación contable.
    /// 
    /// **Datos incluidos:**
    /// - Total de ingresos del día.
    /// - Total pendiente y deuda acumulada.
    /// - Cantidad de pagos (completados, parciales, pendientes).
    /// - Lista detallada de los pagos del día.
    /// </remarks>
    /// <param name="fecha">Fecha para la cual se desea el resumen (formato DateOnly, ej: 2026-06-02).</param>
    /// <returns>Objeto <see cref="ResumenDiarioFinanzasDto"/> con los datos del resumen diario.</returns>
    /// <response code="200">Resumen diario obtenido correctamente.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Acceso denegado.</response>
    [Authorize(Policy = PermisosPolicies.FinanzasVer)]
    [HttpGet("resumen-diario")]
    [ProducesResponseType(typeof(ApiResponse<ResumenDiarioFinanzasDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ObtenerResumenDiario([FromQuery] DateOnly fecha)
    {
        var resumen = await _finanzasService.ObtenerResumenDiarioAsync(fecha);
        return Ok(ApiResponse<object>.Ok(resumen, "Resumen diario de finanzas obtenido correctamente."));
    }

    /// <summary>
    /// Obtiene el resumen mensual de finanzas (agrupado por día).
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Proporciona un desglose financiero completo de un mes específico.
    /// Útil para reportes de gestión mensual y auditoría.
    /// 
    /// **Datos incluidos:**
    /// - Total de ingresos del mes.
    /// - Total pendiente y deuda acumulada.
    /// - Cantidad de pagos por estado.
    /// - Desglose diario con los mismos indicadores.
    /// </remarks>
    /// <param name="anio">Año del resumen (ej: 2026).</param>
    /// <param name="mes">Mes del resumen (1-12).</param>
    /// <returns>Objeto <see cref="ResumenMensualFinanzasDto"/> con los datos del resumen mensual.</returns>
    /// <response code="200">Resumen mensual obtenido correctamente.</response>
    /// <response code="400">Año o mes inválidos.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Acceso denegado.</response>
    [Authorize(Policy = PermisosPolicies.FinanzasVer)]
    [HttpGet("resumen-mensual")]
    [ProducesResponseType(typeof(ApiResponse<ResumenMensualFinanzasDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ObtenerResumenMensual([FromQuery] int anio, [FromQuery] int mes)
    {
        var resumen = await _finanzasService.ObtenerResumenMensualAsync(anio, mes);
        return Ok(ApiResponse<object>.Ok(resumen, "Resumen mensual de finanzas obtenido correctamente."));
    }

    /// <summary>
    /// Obtiene el resumen anual de finanzas (agrupado por mes).
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Proporciona una visión global de la actividad financiera de todo un año.
    /// Útil para balances anuales y análisis de tendencias.
    /// </remarks>
    /// <param name="anio">Año para el cual se desea el resumen (ej: 2026).</param>
    /// <returns>Objeto <see cref="ResumenAnualFinanzasDto"/> con los datos del resumen anual.</returns>
    /// <response code="200">Resumen anual obtenido correctamente.</response>
    /// <response code="400">Año inválido.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Acceso denegado.</response>
    [Authorize(Policy = PermisosPolicies.FinanzasVer)]
    [HttpGet("resumen-anual")]
    [ProducesResponseType(typeof(ApiResponse<ResumenAnualFinanzasDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ObtenerResumenAnual([FromQuery] int anio)
    {
        var resumen = await _finanzasService.ObtenerResumenAnualAsync(anio);
        return Ok(ApiResponse<object>.Ok(resumen, "Resumen anual de finanzas obtenido correctamente."));
    }

    // ==========================================================
    // PAGOS (filtrados por estado)
    // ==========================================================

    /// <summary>
    /// Obtiene todos los pagos pendientes (con saldo pendiente > 0).
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Permite identificar rápidamente todos los pagos que aún no han sido liquidados completamente.
    /// Útil para el seguimiento de cuentas por cobrar.
    /// </remarks>
    /// <returns>Lista de objetos <see cref="PagoFinanzasDto"/> con los pagos pendientes.</returns>
    /// <response code="200">Pagos pendientes obtenidos correctamente.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Acceso denegado.</response>
    [Authorize(Policy = PermisosPolicies.FinanzasVer)]
    [HttpGet("pagos-pendientes")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PagoFinanzasDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ObtenerPagosPendientes()
    {
        var pagos = await _finanzasService.ObtenerPagosPendientesAsync();
        return Ok(ApiResponse<object>.Ok(pagos, "Pagos pendientes obtenidos correctamente."));
    }

    /// <summary>
    /// Obtiene todos los pagos completamente pagados (saldo pendiente = 0).
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Permite consultar el historial de pagos completados.
    /// Útil para reportes de ingresos y conciliación bancaria.
    /// </remarks>
    /// <returns>Lista de objetos <see cref="PagoFinanzasDto"/> con los pagos pagados.</returns>
    /// <response code="200">Pagos pagados obtenidos correctamente.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Acceso denegado.</response>
    [Authorize(Policy = PermisosPolicies.FinanzasVer)]
    [HttpGet("pagos-pagados")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PagoFinanzasDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ObtenerPagosPagados()
    {
        var pagos = await _finanzasService.ObtenerPagosPagadosAsync();
        return Ok(ApiResponse<object>.Ok(pagos, "Pagos pagados obtenidos correctamente."));
    }

    /// <summary>
    /// Obtiene todos los pagos parciales (estado "Parcial").
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Permite identificar los pagos que han sido abonados parcialmente y que aún tienen un saldo pendiente.
    /// Útil para el seguimiento de deudas fraccionadas.
    /// </remarks>
    /// <returns>Lista de objetos <see cref="PagoFinanzasDto"/> con los pagos parciales.</returns>
    /// <response code="200">Pagos parciales obtenidos correctamente.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Acceso denegado.</response>
    [Authorize(Policy = PermisosPolicies.FinanzasVer)]
    [HttpGet("pagos-parciales")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PagoFinanzasDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ObtenerPagosParciales()
    {
        var pagos = await _finanzasService.ObtenerPagosParcialesAsync();
        return Ok(ApiResponse<object>.Ok(pagos, "Pagos parciales obtenidos correctamente."));
    }

    // ==========================================================
    // CONSULTAS ESPECÍFICAS
    // ==========================================================

    /// <summary>
    /// Obtiene un pago específico por su código de pago.
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Permite buscar un pago de forma rápida utilizando el código único generado por el sistema.
    /// Útil para validaciones en caja o para la emisión de comprobantes.
    /// </remarks>
    /// <param name="codigoPago">Código único del pago (ej: "PAG-2026-abc123").</param>
    /// <returns>Objeto <see cref="PagoFinanzasDto"/> con los datos del pago.</returns>
    /// <response code="200">Pago obtenido correctamente.</response>
    /// <response code="400">El código de pago es inválido o está vacío.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Acceso denegado.</response>
    /// <response code="404">Pago no encontrado.</response>
    [Authorize(Policy = PermisosPolicies.FinanzasVer)]
    [HttpGet("pago/codigo/{codigoPago}")]
    [ProducesResponseType(typeof(ApiResponse<PagoFinanzasDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPagoPorCodigo(string codigoPago)
    {
        var pago = await _finanzasService.ObtenerPagoPorCodigoAsync(codigoPago);

        if (pago == null)
            throw new KeyNotFoundException("Pago no encontrado.");

        return Ok(ApiResponse<object>.Ok(pago, "Pago obtenido correctamente."));
    }

    /// <summary>
    /// Obtiene el estado de cuenta completo de un paciente.
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Proporciona un resumen financiero completo de un paciente, incluyendo el total facturado, pagado y pendiente.
    /// Útil para informar al paciente sobre su situación financiera o para auditoría.
    /// </remarks>
    /// <param name="pacienteId">Identificador único del paciente (GUID).</param>
    /// <returns>Objeto <see cref="EstadoCuentaPacienteDto"/> con los datos del estado de cuenta.</returns>
    /// <response code="200">Estado de cuenta obtenido correctamente.</response>
    /// <response code="400">El identificador del paciente es inválido.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Acceso denegado.</response>
    /// <response code="404">Paciente no encontrado.</response>
    [Authorize(Policy = PermisosPolicies.FinanzasVer)]
    [HttpGet("paciente/{pacienteId:guid}/estado-cuenta")]
    [ProducesResponseType(typeof(ApiResponse<EstadoCuentaPacienteDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerEstadoCuentaPaciente(Guid pacienteId)
    {
        var estadoCuenta = await _finanzasService.ObtenerEstadoCuentaPacienteAsync(pacienteId);
        return Ok(ApiResponse<object>.Ok(estadoCuenta, "Estado de cuenta del paciente obtenido correctamente."));
    }

    /// <summary>
    /// Obtiene la lista de deudas reales (atenciones con saldo pendiente) de todo el sistema.
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Permite identificar globalmente todas las atenciones que aún tienen un saldo pendiente.
    /// Útil para la gestión de cobranza y análisis de cartera.
    /// 
    /// **Nota de arquitectura:** Las deudas se agrupan por atención médica, no por pago individual.
    /// </remarks>
    /// <returns>Lista de objetos <see cref="EstadoPagoAtencionDto"/> con las deudas reales.</returns>
    /// <response code="200">Deudas reales obtenidas correctamente.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Acceso denegado.</response>
    [Authorize(Policy = PermisosPolicies.FinanzasVer)]
    [HttpGet("deudas-reales")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<EstadoPagoAtencionDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ObtenerDeudasReales()
    {
        var deudas = await _finanzasService.ObtenerDeudasRealesAsync();
        return Ok(ApiResponse<object>.Ok(deudas, "Deudas reales obtenidas correctamente."));
    }

    /// <summary>
    /// Obtiene la lista de deudas reales de un paciente específico.
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Permite consultar las deudas pendientes de un paciente en particular.
    /// Útil para el área de cobranza o para informar al paciente.
    /// </remarks>
    /// <param name="pacienteId">Identificador único del paciente (GUID).</param>
    /// <returns>Lista de objetos <see cref="EstadoPagoAtencionDto"/> con las deudas del paciente.</returns>
    /// <response code="200">Deudas reales del paciente obtenidas correctamente.</response>
    /// <response code="400">El identificador del paciente es inválido.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Acceso denegado.</response>
    /// <response code="404">Paciente no encontrado.</response>
    [Authorize(Policy = PermisosPolicies.FinanzasVer)]
    [HttpGet("paciente/{pacienteId:guid}/deudas-reales")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<EstadoPagoAtencionDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerDeudasRealesPaciente(Guid pacienteId)
    {
        var deudas = await _finanzasService.ObtenerDeudasRealesPacienteAsync(pacienteId);
        return Ok(ApiResponse<object>.Ok(deudas, "Deudas reales del paciente obtenidas correctamente."));
    }

    /// <summary>
    /// Obtiene el estado de pago de una atención médica específica.
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Permite conocer el estado financiero de una atención en particular (pagada, parcial, pendiente, sobrepagada).
    /// Útil para el área de caja al momento de registrar nuevos pagos.
    /// </remarks>
    /// <param name="atencionId">Identificador único de la atención (GUID).</param>
    /// <returns>Objeto <see cref="EstadoPagoAtencionDto"/> con el estado de pago de la atención.</returns>
    /// <response code="200">Estado de pago obtenido correctamente.</response>
    /// <response code="400">El identificador de la atención es inválido.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Acceso denegado.</response>
    /// <response code="404">Atención no encontrada o sin pagos asociados.</response>
    [Authorize(Policy = PermisosPolicies.FinanzasVer)]
    [HttpGet("atencion/{atencionId:guid}/estado-pago")]
    [ProducesResponseType(typeof(ApiResponse<EstadoPagoAtencionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerEstadoPagoAtencion(Guid atencionId)
    {
        var estadoPago = await _finanzasService.ObtenerEstadoPagoAtencionAsync(atencionId);
        return Ok(ApiResponse<object>.Ok(estadoPago, "Estado de pago de la atención obtenido correctamente."));
    }

    // ==========================================================
    // REPORTES AVANZADOS
    // ==========================================================

    /// <summary>
    /// Obtiene el libro diario de finanzas para una fecha específica.
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Proporciona un registro cronológico de todos los pagos realizados en una fecha determinada.
    /// Útil para la contabilidad y la auditoría financiera.
    /// </remarks>
    /// <param name="fecha">Fecha para la cual se desea el libro diario (formato DateOnly).</param>
    /// <returns>Lista de objetos <see cref="PagoFinanzasDto"/> con los movimientos del día.</returns>
    /// <response code="200">Libro diario obtenido correctamente.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Acceso denegado.</response>
    [Authorize(Policy = PermisosPolicies.FinanzasVer)]
    [HttpGet("libro-diario")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PagoFinanzasDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ObtenerLibroDiario([FromQuery] DateOnly fecha)
    {
        var resultado = await _finanzasService.ObtenerLibroDiarioAsync(fecha);
        return Ok(ApiResponse<object>.Ok(resultado, "Libro diario obtenido correctamente."));
    }

    /// <summary>
    /// Obtiene un resumen financiero mensual completo que incluye caja, atenciones y ajustes.
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Proporciona la vista más completa de la actividad financiera de un mes, integrando:
    /// - Resumen de caja (ingresos por método de pago, movimientos).
    /// - Resumen real de atenciones (facturado, pagado, deuda real).
    /// - Lista de ajustes financieros registrados en el mes.
    /// 
    /// Útil para el cierre contable mensual y la toma de decisiones gerenciales.
    /// </remarks>
    /// <param name="anio">Año del resumen (ej: 2026).</param>
    /// <param name="mes">Mes del resumen (1-12).</param>
    /// <returns>Objeto <see cref="ResumenFinancieroMensualCompletoDto"/> con todos los datos.</returns>
    /// <response code="200">Resumen financiero mensual completo obtenido correctamente.</response>
    /// <response code="400">Año o mes inválidos.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Acceso denegado.</response>
    [Authorize(Policy = PermisosPolicies.FinanzasVer)]
    [HttpGet("resumen-financiero-mensual-completo")]
    [ProducesResponseType(typeof(ApiResponse<ResumenFinancieroMensualCompletoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ObtenerResumenFinancieroMensualCompleto(
        [FromQuery] int anio,
        [FromQuery] int mes)
    {
        var resultado = await _finanzasService.ObtenerResumenFinancieroMensualCompletoAsync(anio, mes);
        return Ok(ApiResponse<object>.Ok(resultado, "Resumen financiero mensual completo obtenido correctamente."));
    }

    // ==========================================================
    // AJUSTES FINANCIEROS
    // ==========================================================

    /// <summary>
    /// Registra un nuevo ajuste financiero (descuento, recargo, corrección, etc.).
    /// </summary>
    /// <remarks>
    /// **Proceso de negocio:**
    /// 1. Valida que el pago exista.
    /// 2. Valida que el monto del ajuste sea mayor a 0.
    /// 3. Valida que el motivo no esté vacío.
    /// 4. Verifica que no exista un ajuste similar duplicado.
    /// 5. Crea el registro de ajuste financiero.
    /// 
    /// **Nota de auditoría:** Esta acción queda registrada automáticamente como crítica.
    /// </remarks>
    /// <param name="dto">Objeto <see cref="RegistrarAjusteFinancieroDto"/> con los datos del ajuste.</param>
    /// <returns>Objeto con el ID del ajuste creado.</returns>
    /// <response code="200">Ajuste financiero registrado correctamente.</response>
    /// <response code="400">Datos inválidos o ajuste duplicado.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Permisos insuficientes.</response>
    /// <response code="404">Pago no encontrado.</response>
    [Auditoria("Finanzas", "Ajuste financiero", TipoAccionAuditoria.Creacion, NivelAuditoria.Critico)]
    [Authorize(Policy = PermisosPolicies.PagoRegistrar)]
    [HttpPost("ajustes-financieros")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RegistrarAjusteFinanciero([FromBody] RegistrarAjusteFinancieroDto dto)
    {
        var id = await _finanzasService.RegistrarAjusteFinancieroAsync(dto);
        return Ok(ApiResponse<object>.Ok(new { id }, "Ajuste financiero registrado correctamente."));
    }

    /// <summary>
    /// Obtiene todos los ajustes financieros registrados en el sistema.
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Permite consultar el historial completo de ajustes financieros.
    /// Útil para auditoría y seguimiento de correcciones contables.
    /// </remarks>
    /// <returns>Lista de objetos <see cref="AjusteFinancieroDto"/> con los ajustes.</returns>
    /// <response code="200">Ajustes financieros obtenidos correctamente.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Acceso denegado.</response>
    [Authorize(Policy = PermisosPolicies.FinanzasVer)]
    [HttpGet("ajustes-financieros")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<AjusteFinancieroDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ObtenerAjustesFinancieros()
    {
        var resultado = await _finanzasService.ObtenerAjustesFinancierosAsync();
        return Ok(ApiResponse<object>.Ok(resultado, "Ajustes financieros obtenidos correctamente."));
    }

    /// <summary>
    /// Obtiene los ajustes financieros asociados a una atención médica específica.
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Permite consultar los ajustes relacionados con una atención en particular.
    /// Útil para analizar el historial financiero de una atención.
    /// </remarks>
    /// <param name="atencionId">Identificador único de la atención (GUID).</param>
    /// <returns>Lista de objetos <see cref="AjusteFinancieroDto"/> con los ajustes de la atención.</returns>
    /// <response code="200">Ajustes de la atención obtenidos correctamente.</response>
    /// <response code="400">El identificador de la atención es inválido.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Acceso denegado.</response>
    [Authorize(Policy = PermisosPolicies.FinanzasVer)]
    [HttpGet("atencion/{atencionId:guid}/ajustes-financieros")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<AjusteFinancieroDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ObtenerAjustesPorAtencion(Guid atencionId)
    {
        var resultado = await _finanzasService.ObtenerAjustesPorAtencionAsync(atencionId);
        return Ok(ApiResponse<object>.Ok(resultado, "Ajustes financieros de la atención obtenidos correctamente."));
    }

    /// <summary>
    /// Obtiene los ajustes financieros asociados a un pago específico.
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Permite consultar los ajustes relacionados con un pago en particular.
    /// Útil para rastrear correcciones contables aplicadas a un pago.
    /// </remarks>
    /// <param name="pagoId">Identificador único del pago (GUID).</param>
    /// <returns>Lista de objetos <see cref="AjusteFinancieroDto"/> con los ajustes del pago.</returns>
    /// <response code="200">Ajustes del pago obtenidos correctamente.</response>
    /// <response code="400">El identificador del pago es inválido.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Acceso denegado.</response>
    [Authorize(Policy = PermisosPolicies.FinanzasVer)]
    [HttpGet("pago/{pagoId:guid}/ajustes-financieros")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<AjusteFinancieroDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ObtenerAjustesPorPago(Guid pagoId)
    {
        var resultado = await _finanzasService.ObtenerAjustesPorPagoAsync(pagoId);
        return Ok(ApiResponse<object>.Ok(resultado, "Ajustes financieros del pago obtenidos correctamente."));
    }
}