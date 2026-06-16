using Clinica.API.Services;
using Microsoft.AspNetCore.Mvc;
using Clinica.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Clinica.API.Models;

namespace Clinica.API.Controllers;

/// <summary>
/// Controlador para acceder al registro de auditoría del sistema.
/// </summary>
/// <remarks>
/// **Nota de Arquitectura:** La auditoría es un pilar fundamental en sistemas clínicos para garantizar la trazabilidad y el cumplimiento normativo. 
/// Cada acción relevante en el sistema (creación, modificación, eliminación lógica, login) queda registrada automáticamente mediante el filtro <see cref="AuditoriaAutomaticaFilter"/>.
/// Este controlador permite consultar dichos registros para fines de supervisión, auditoría interna o investigación.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Tags("Auditoría del Sistema")]
public class AuditoriaController : ControllerBase
{
    private readonly IAuditoriaService _auditoriaService;

    public AuditoriaController(IAuditoriaService auditoriaService)
    {
        _auditoriaService = auditoriaService;
    }

    /// <summary>
    /// Obtiene todos los registros de auditoría del sistema.
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Este endpoint permite a los administradores y auditores revisar la totalidad de las acciones registradas en el sistema.
    /// La información obtenida incluye el usuario que realizó la acción, el tipo de acción (consulta, creación, edición, etc.), la entidad afectada, el valor anterior y el nuevo valor, la IP y el User-Agent, entre otros datos.
    /// 
    /// **Nota de rendimiento:** Dado que el volumen de registros puede ser muy grande, se recomienda aplicar filtros por fecha o usuario en futuras versiones para optimizar las consultas.
    /// </remarks>
    /// <returns>Lista de todos los registros de auditoría.</returns>
    /// <response code="200">Lista de auditorías obtenida correctamente.</response>
    /// <response code="401">No autorizado. Token JWT inválido o ausente.</response>
    /// <response code="403">Acceso denegado. El usuario no tiene el permiso requerido.</response>
    [Authorize(Policy = PermisosPolicies.AuditoriaVer)]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _auditoriaService.ObtenerTodosAsync());
    }

    /// <summary>
    /// Obtiene todos los registros de auditoría asociados a un usuario específico.
    /// </summary>
    /// <remarks>
    /// **Casos de uso:**
    /// - Revisar el historial de acciones de un operador o médico en particular.
    /// - Investigar un incidente de seguridad o error operativo relacionado con un usuario específico.
    /// - Extraer reportes de actividad individual para cumplir con requerimientos de auditoría externa.
    /// </remarks>
    /// <param name="usuarioId">Identificador único del usuario (formato GUID).</param>
    /// <returns>Lista de registros de auditoría filtrados por el usuario.</returns>
    /// <response code="200">Registros de auditoría del usuario obtenidos correctamente.</response>
    /// <response code="401">No autorizado. Token JWT inválido o ausente.</response>
    /// <response code="403">Acceso denegado. El usuario no tiene el permiso requerido.</response>
    [Authorize(Policy = PermisosPolicies.AuditoriaVer)]
    [HttpGet("usuario/{usuarioId:guid}")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByUsuario(Guid usuarioId)
    {
        return Ok(await _auditoriaService.ObtenerPorUsuarioAsync(usuarioId));
    }
}