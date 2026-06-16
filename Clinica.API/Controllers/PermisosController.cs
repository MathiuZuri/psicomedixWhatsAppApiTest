using Clinica.API.Authorization;
using Clinica.API.Models;
using Clinica.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.API.Controllers;

/// <summary>
/// Controlador para consultar el diccionario de permisos del sistema.
/// </summary>
/// <remarks>
/// **Módulo de Seguridad:** Este controlador permite consultar todos los permisos definidos en el sistema.
/// Los permisos son la base de la autorización y se asignan a los roles para controlar el acceso a los endpoints.
/// 
/// **Nota de Arquitectura:** Los permisos son inmutables y se definen en <see cref="PermisosPolicies"/>. 
/// No se pueden crear, modificar ni eliminar a través de la API; solo se pueden consultar.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Tags("Seguridad - Permisos")]
public class PermisosController : ControllerBase
{
    private readonly IPermisoService _permisoService;

    public PermisosController(IPermisoService permisoService)
    {
        _permisoService = permisoService;
    }

    /// <summary>
    /// Obtiene la lista completa de todos los permisos del sistema.
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Permite consultar el catálogo completo de permisos definidos en el sistema.
    /// Útil para la administración de roles y la asignación de permisos.
    /// 
    /// **Datos incluidos:**
    /// - ID único del permiso.
    /// - Código del permiso (ej: "PACIENTE_VER").
    /// - Nombre descriptivo.
    /// - Módulo al que pertenece.
    /// - Estado (activo/inactivo).
    /// </remarks>
    /// <returns>Lista de objetos <see cref="PermisoResponseDto"/> con los datos de cada permiso.</returns>
    /// <response code="200">Permisos obtenidos correctamente.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Acceso denegado.</response>
    [Authorize(Policy = PermisosPolicies.PermisoVer)]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll()
    {
        var permisos = await _permisoService.ObtenerTodosAsync();
        return Ok(ApiResponse<object>.Ok(permisos, "Permisos obtenidos correctamente."));
    }
}