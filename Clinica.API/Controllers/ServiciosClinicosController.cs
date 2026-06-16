using Clinica.API.Authorization;
using Clinica.API.Models;
using Clinica.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.API.Controllers;

/// <summary>
/// Controlador para la consulta del catálogo de servicios clínicos.
/// </summary>
/// <remarks>
/// **Módulo de Servicios Clínicos:** Permite consultar los servicios disponibles en el sistema (consultas, procedimientos, etc.).
/// 
/// **Nota de Arquitectura:** El catálogo de servicios es de solo lectura a través de la API. 
/// Los servicios se cargan inicialmente mediante el <see cref="DataSeeder"/> y no pueden ser modificados por los usuarios.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Tags("Servicios Clínicos")]
public class ServiciosClinicosController : ControllerBase
{
    private readonly IServicioClinicoService _servicioService;

    public ServiciosClinicosController(IServicioClinicoService servicioService)
    {
        _servicioService = servicioService;
    }

    /// <summary>
    /// Obtiene la lista completa de todos los servicios clínicos registrados.
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Permite consultar el catálogo completo de servicios clínicos, incluyendo su costo base, duración y requisitos.
    /// Útil para la programación de citas y la facturación.
    /// </remarks>
    /// <returns>Lista de objetos <see cref="ServicioClinicoResponseDto"/> con los datos de cada servicio.</returns>
    /// <response code="200">Servicios clínicos obtenidos correctamente.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Acceso denegado.</response>
    [Authorize(Policy = PermisosPolicies.ServicioVer)]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll()
    {
        var servicios = await _servicioService.ObtenerTodosAsync();
        return Ok(ApiResponse<object>.Ok(servicios, "Servicios clínicos obtenidos correctamente."));
    }

    /// <summary>
    /// Obtiene la lista de servicios clínicos activos.
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Permite consultar únicamente los servicios que están activos (estado "Activo").
    /// Útil para la programación de citas, ya que solo los servicios activos pueden ser seleccionados.
    /// </remarks>
    /// <returns>Lista de objetos <see cref="ServicioClinicoResponseDto"/> con los servicios activos.</returns>
    /// <response code="200">Servicios clínicos activos obtenidos correctamente.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Acceso denegado.</response>
    [Authorize(Policy = PermisosPolicies.ServicioVer)]
    [HttpGet("activos")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetActivos()
    {
        var servicios = await _servicioService.ObtenerActivosAsync();
        return Ok(ApiResponse<object>.Ok(servicios, "Servicios clínicos activos obtenidos correctamente."));
    }

    /// <summary>
    /// Obtiene los datos de un servicio clínico específico por su ID.
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Permite recuperar la información completa de un servicio clínico individual.
    /// Útil para la pantalla de detalle del servicio o para validar datos antes de una programación.
    /// </remarks>
    /// <param name="id">Identificador único del servicio clínico (GUID).</param>
    /// <returns>Objeto <see cref="ServicioClinicoResponseDto"/> con los datos del servicio.</returns>
    /// <response code="200">Servicio clínico obtenido correctamente.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Acceso denegado.</response>
    /// <response code="404">Servicio clínico no encontrado.</response>
    [Authorize(Policy = PermisosPolicies.ServicioVer)]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var servicio = await _servicioService.ObtenerPorIdAsync(id);

        if (servicio == null)
            throw new KeyNotFoundException("Servicio clínico no encontrado.");

        return Ok(ApiResponse<object>.Ok(servicio, "Servicio clínico obtenido correctamente."));
    }
}