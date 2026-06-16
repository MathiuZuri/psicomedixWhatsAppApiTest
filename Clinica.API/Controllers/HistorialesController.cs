using Clinica.API.Authorization;
using Clinica.API.Models;
using Clinica.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.API.Controllers;

/// <summary>
/// Controlador para la consulta de historiales clínicos de pacientes.
/// </summary>
/// <remarks>
/// **Nota de Arquitectura:** El historial clínico es el núcleo de la trazabilidad médica en el sistema.
/// Cada acción relevante (cita, atención, pago) registra un detalle en el historial.
/// Este controlador permite consultar dichos registros para obtener una línea de tiempo clínica del paciente.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Tags("Historial Clínico")]
public class HistorialesController : ControllerBase
{
    private readonly IHistorialClinicoService _historialService;

    public HistorialesController(IHistorialClinicoService historialService)
    {
        _historialService = historialService;
    }

    /// <summary>
    /// Obtiene el historial clínico completo de un paciente.
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Permite recuperar el resumen del historial clínico de un paciente, incluyendo sus datos básicos.
    /// Los detalles de los movimientos se pueden obtener con el endpoint <see cref="GetConDetalles"/>.
    /// 
    /// **Nota:** Si el paciente no tiene historial clínico, se lanza una excepción.
    /// </remarks>
    /// <param name="pacienteId">Identificador único del paciente (GUID).</param>
    /// <returns>Objeto <see cref="HistorialClinicoResponseDto"/> con los datos del historial.</returns>
    /// <response code="200">Historial clínico obtenido correctamente.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Acceso denegado.</response>
    /// <response code="404">Historial clínico no encontrado para el paciente.</response>
    [Authorize(Policy = PermisosPolicies.HistorialVer)]
    [HttpGet("paciente/{pacienteId:guid}")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByPaciente(Guid pacienteId)
    {
        var historial = await _historialService.ObtenerPorPacienteAsync(pacienteId);

        if (historial == null)
            throw new KeyNotFoundException("Historial clínico no encontrado.");

        return Ok(ApiResponse<object>.Ok(historial, "Historial clínico obtenido correctamente."));
    }

    /// <summary>
    /// Obtiene el historial clínico de un paciente con todos sus detalles (movimientos).
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Proporciona la versión completa del historial clínico, incluyendo la lista detallada de todos los movimientos.
    /// 
    /// **Datos incluidos:**
    /// - Datos básicos del paciente y del historial.
    /// - Lista de movimientos (citas, atenciones, pagos) con fechas, tipos, descripciones y usuario responsable.
    /// 
    /// Útil para la visualización completa de la línea de tiempo clínica del paciente.
    /// </remarks>
    /// <param name="historialId">Identificador único del historial clínico (GUID).</param>
    /// <returns>Objeto <see cref="HistorialClinicoResponseDto"/> con todos los detalles.</returns>
    /// <response code="200">Historial clínico con detalles obtenido correctamente.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Acceso denegado.</response>
    /// <response code="404">Historial clínico no encontrado.</response>
    [Authorize(Policy = PermisosPolicies.HistorialVer)]
    [HttpGet("{historialId:guid}/detalles")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetConDetalles(Guid historialId)
    {
        var historial = await _historialService.ObtenerConDetallesAsync(historialId);

        if (historial == null)
            throw new KeyNotFoundException("Historial clínico no encontrado.");

        return Ok(ApiResponse<object>.Ok(historial, "Historial clínico con detalles obtenido correctamente."));
    }
}