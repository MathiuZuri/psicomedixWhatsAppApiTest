using Clinica.API.Authorization;
using Clinica.API.Filters;
using Clinica.API.Models;
using Clinica.API.Services;
using Clinica.Domain.DTOs.Pagos;
using Clinica.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.API.Controllers;

/// <summary>
/// Controlador para la gestión de pagos y transacciones financieras del sistema.
/// </summary>
/// <remarks>
/// **Módulo de Pagos:** Permite registrar, consultar y modificar el estado de los pagos asociados a pacientes, citas y atenciones médicas.
/// 
/// **Nota de Arquitectura:** Los pagos son el núcleo del sistema financiero. Cada pago queda vinculado a una atención o cita, y su estado puede ser gestionado.
/// La eliminación física de pagos no está permitida; solo se puede cambiar su estado a "Anulado" o "Eliminado".
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Tags("Gestión de Pagos")]
public class PagosController : ControllerBase
{
    private readonly IPagoService _pagoService;

    public PagosController(IPagoService pagoService)
    {
        _pagoService = pagoService;
    }

    /// <summary>
    /// Obtiene todos los pagos asociados a un paciente específico.
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Permite consultar el historial completo de pagos de un paciente.
    /// Útil para la ficha del paciente, el área de caja o para auditoría financiera.
    /// 
    /// **Datos incluidos:**
    /// - Código de pago único.
    /// - Servicio clínico asociado.
    /// - Montos (total, pagado, saldo pendiente, adelanto).
    /// - Método de pago y estado actual.
    /// </remarks>
    /// <param name="pacienteId">Identificador único del paciente (GUID).</param>
    /// <returns>Lista de objetos <see cref="PagoResponseDto"/> con los datos de cada pago.</returns>
    /// <response code="200">Pagos del paciente obtenidos correctamente.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Acceso denegado.</response>
    [Authorize(Policy = PermisosPolicies.PagoVer)]
    [HttpGet("paciente/{pacienteId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PagoResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByPaciente(Guid pacienteId)
    {
        var pagos = await _pagoService.ObtenerPorPacienteAsync(pacienteId);
        return Ok(ApiResponse<object>.Ok(pagos, "Pagos del paciente obtenidos correctamente."));
    }

    /// <summary>
    /// Obtiene todos los pagos asociados a una cita específica.
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Permite consultar los pagos realizados en el contexto de una cita (ej: reserva, adelanto, pago completo).
    /// Útil para verificar el estado financiero de una cita.
    /// </remarks>
    /// <param name="citaId">Identificador único de la cita (GUID).</param>
    /// <returns>Lista de objetos <see cref="PagoResponseDto"/> con los datos de cada pago.</returns>
    /// <response code="200">Pagos de la cita obtenidos correctamente.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Acceso denegado.</response>
    [Authorize(Policy = PermisosPolicies.PagoVer)]
    [HttpGet("cita/{citaId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PagoResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByCita(Guid citaId)
    {
        var pagos = await _pagoService.ObtenerPorCitaAsync(citaId);
        return Ok(ApiResponse<object>.Ok(pagos, "Pagos de la cita obtenidos correctamente."));
    }

    /// <summary>
    /// Obtiene todos los pagos asociados a una atención médica específica.
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Permite consultar los pagos realizados en el contexto de una atención médica.
    /// Útil para el seguimiento financiero de una consulta o procedimiento.
    /// </remarks>
    /// <param name="atencionId">Identificador único de la atención (GUID).</param>
    /// <returns>Lista de objetos <see cref="PagoResponseDto"/> con los datos de cada pago.</returns>
    /// <response code="200">Pagos de la atención obtenidos correctamente.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Acceso denegado.</response>
    [Authorize(Policy = PermisosPolicies.PagoVer)]
    [HttpGet("atencion/{atencionId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PagoResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByAtencion(Guid atencionId)
    {
        var pagos = await _pagoService.ObtenerPorAtencionAsync(atencionId);
        return Ok(ApiResponse<object>.Ok(pagos, "Pagos de la atención obtenidos correctamente."));
    }

    /// <summary>
    /// Registra un nuevo pago en el sistema.
    /// </summary>
    /// <remarks>
    /// **Proceso de negocio:**
    /// 1. Valida que el paciente y el servicio existan.
    /// 2. Valida que el monto pagado no sea mayor al monto total.
    /// 3. Valida que el monto de adelanto no sea mayor al monto total.
    /// 4. Calcula el saldo pendiente.
    /// 5. Crea el pago con estado "Pagado" (si saldo = 0) o "Parcial" (si saldo > 0).
    /// 6. Actualiza el monto pagado y saldo pendiente de la atención asociada (si existe).
    /// 7. Registra un detalle en el historial clínico del paciente.
    /// 
    /// **Nota de auditoría:** Esta acción queda registrada automáticamente como crítica.
    /// </remarks>
    /// <param name="dto">Objeto <see cref="RegistrarPagoDto"/> con los datos del pago.</param>
    /// <returns>Objeto con el ID del pago creado.</returns>
    /// <response code="200">Pago registrado correctamente.</response>
    /// <response code="400">Datos inválidos (monto pagado > total, etc.).</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Permisos insuficientes.</response>
    /// <response code="404">Paciente o servicio no encontrado.</response>
    [Auditoria("Pagos", "Pago", TipoAccionAuditoria.Creacion, NivelAuditoria.Critico)]
    [Authorize(Policy = PermisosPolicies.PagoRegistrar)]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Registrar([FromBody] RegistrarPagoDto dto)
    {
        var id = await _pagoService.RegistrarAsync(dto);
        return Ok(ApiResponse<object>.Ok(new { id }, "Pago registrado correctamente."));
    }

    /// <summary>
    /// Cambia el estado de un pago existente.
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Permite modificar el estado de un pago (ej: de "Pendiente" a "Pagado", o de "Pagado" a "Anulado").
    /// 
    /// **Nota de Arquitectura:** Esta acción es una "eliminación lógica" cuando se usa el estado "Eliminado".
    /// No se puede eliminar un pago con saldo pendiente.
    /// </remarks>
    /// <param name="id">Identificador único del pago (GUID).</param>
    /// <param name="dto">Objeto <see cref="CambiarEstadoPagoDto"/> con el nuevo estado.</param>
    /// <response code="200">Estado del pago actualizado correctamente.</response>
    /// <response code="400">El pago ya está eliminado o tiene saldo pendiente.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Permisos insuficientes.</response>
    /// <response code="404">Pago no encontrado.</response>
    [Auditoria("Pagos", "Pago", TipoAccionAuditoria.Edicion, NivelAuditoria.Critico)]
    [Authorize(Policy = PermisosPolicies.PagoRegistrar)]
    [HttpPut("{id:guid}/estado")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CambiarEstado(Guid id, [FromBody] CambiarEstadoPagoDto dto)
    {
        await _pagoService.CambiarEstadoAsync(id, dto);
        return Ok(ApiResponse<object>.Ok(new { id }, "Estado del pago actualizado correctamente."));
    }
}