using Clinica.API.Authorization;
using Clinica.API.Filters;
using Clinica.API.Models;
using Clinica.API.Services;
using Clinica.Domain.DTOs.Atenciones;
using Clinica.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.API.Controllers;

/// <summary>
/// Gestión de atenciones médicas, consultas y procedimientos del sistema clínico.
/// </summary>
/// <remarks>
/// **Nota de Arquitectura:** Por normativas de salud y auditoría clínica, las atenciones 
/// no poseen métodos de eliminación (ni física ni lógica). El ciclo de vida de un registro 
/// médico finaliza mediante el cambio de estado a "Cerrada" o "Anulada".
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Tags("Atenciones Médicas")]
public class AtencionesController : ControllerBase
{
    private readonly IAtencionService _atencionService;

    public AtencionesController(IAtencionService atencionService)
    {
        _atencionService = atencionService;
    }

    /// <summary>
    /// Obtiene el historial de atenciones de un paciente.
    /// </summary>
    /// <remarks>
    /// Recupera el registro histórico de todas las interacciones clínicas de un paciente.
    /// 
    /// **Datos incluidos en la respuesta:**
    /// - Datos del doctor asignado.
    /// - Servicio clínico brindado.
    /// - Costos y saldos pendientes.
    /// - Fechas de registro y cierre.
    /// </remarks>
    /// <param name="pacienteId">Identificador único del paciente (formato GUID).</param>
    /// <returns>Lista de atenciones asociadas al paciente.</returns>
    /// <response code="200">Búsqueda exitosa. Retorna la lista de atenciones (puede estar vacía).</response>
    /// <response code="401">Autenticación requerida. Falta el token JWT o ha expirado.</response>
    /// <response code="403">Acceso denegado. El usuario no tiene el permiso requerido.</response>
    [Authorize(Policy = PermisosPolicies.AtencionVer)]
    [HttpGet("paciente/{pacienteId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<AtencionResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByPaciente(Guid pacienteId)
    {
        var atenciones = await _atencionService.ObtenerPorPacienteAsync(pacienteId);
        return Ok(ApiResponse<object>.Ok(atenciones, "Atenciones del paciente obtenidas correctamente."));
    }

    /// <summary>
    /// Obtiene la atención médica derivada de una cita específica.
    /// </summary>
    /// <remarks>
    /// **Casos de uso:**
    /// Útil cuando el usuario ingresa desde la agenda de citas y necesita ver los detalles clínicos y financieros generados a partir de esa reserva.
    /// </remarks>
    /// <param name="citaId">Identificador único de la cita base (formato GUID).</param>
    /// <returns>Objeto con el detalle completo de la atención.</returns>
    /// <response code="200">Atención encontrada y devuelta con éxito.</response>
    /// <response code="401">Autenticación requerida. Falta el token JWT o ha expirado.</response>
    /// <response code="403">Acceso denegado. El usuario no tiene el permiso requerido.</response>
    /// <response code="404">No existe ninguna atención médica generada para esta cita.</response>
    [Authorize(Policy = PermisosPolicies.AtencionVer)]
    [HttpGet("cita/{citaId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<AtencionResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCita(Guid citaId)
    {
        var atencion = await _atencionService.ObtenerPorCitaAsync(citaId);

        if (atencion == null)
            throw new KeyNotFoundException("Atención no encontrada.");

        return Ok(ApiResponse<object>.Ok(atencion, "Atención obtenida correctamente."));
    }

    /// <summary>
    /// Registra el inicio de una nueva atención médica.
    /// </summary>
    /// <remarks>
    /// **Proceso de negocio:**
    /// Este endpoint se ejecuta en admisión/triaje o cuando el doctor recibe al paciente. 
    /// Transforma una cita programada en una atención activa en la clínica.
    /// 
    /// **Requisitos:**
    /// - El costo final no puede ser negativo.
    /// - Los IDs de paciente, doctor y servicio deben existir en la base de datos.
    /// </remarks>
    /// <param name="dto">Estructura JSON con los parámetros de la nueva atención.</param>
    /// <returns>El ID (GUID) generado para la nueva atención.</returns>
    /// <response code="200">Atención generada e insertada en la base de datos correctamente.</response>
    /// <response code="400">Error de validación (ej. costo negativo o campos obligatorios vacíos).</response>
    /// <response code="401">Autenticación requerida.</response>
    /// <response code="403">Permisos insuficientes para registrar atenciones.</response>
    /// <response code="404">No se encontró el paciente, doctor o servicio indicado.</response>
    [Auditoria("Atenciones", "Atencion", TipoAccionAuditoria.Creacion, NivelAuditoria.Critico)]
    [Authorize(Policy = PermisosPolicies.AtencionRegistrar)]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Registrar([FromBody] RegistrarAtencionDto dto)
    {
        var id = await _atencionService.RegistrarAsync(dto);
        return Ok(ApiResponse<object>.Ok(new { id }, "Atención registrada correctamente."));
    }

    /// <summary>
    /// Finaliza y cierra una atención médica en curso. (Reemplaza la eliminación física/lógica).
    /// </summary>
    /// <remarks>
    /// **Acciones internas del sistema:**
    /// 1. Cambia el estado interno a "Cerrada", blindando el registro contra modificaciones para cumplir normativas de auditoría médica.
    /// 2. Guarda el diagnóstico definitivo.
    /// 3. Adjunta el tratamiento e indicaciones médicas a la historia clínica.
    /// 
    /// *Nota: Una vez cerrada, la atención pasa a estado de solo lectura clínica. Si ocurrió un error grave en la digitación, se debe usar un proceso de anulación, no de eliminación.*
    /// </remarks>
    /// <param name="id">ID de la atención activa que se va a finalizar.</param>
    /// <param name="dto">JSON con el diagnóstico y tratamiento emitido por el doctor.</param>
    /// <response code="200">Atención finalizada con éxito.</response>
    /// <response code="400">Datos médicos de cierre incompletos o inválidos.</response>
    /// <response code="401">Autenticación requerida.</response>
    /// <response code="403">Permisos insuficientes para cerrar atenciones médicas.</response>
    /// <response code="404">La atención solicitada no existe.</response>
    [Auditoria("Atenciones", "Atencion", TipoAccionAuditoria.Edicion, NivelAuditoria.Critico)]
    [Authorize(Policy = PermisosPolicies.AtencionCerrar)]
    [HttpPut("{id:guid}/cerrar")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cerrar(Guid id, [FromBody] CerrarAtencionDto dto)
    {
        await _atencionService.CerrarAsync(id, dto);
        return Ok(ApiResponse<object>.Ok(new { id }, "Atención cerrada correctamente."));
    }
}