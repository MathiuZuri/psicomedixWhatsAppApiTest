using Clinica.API.Authorization;
using Clinica.API.Filters;
using Clinica.API.Models;
using Clinica.API.Services;
using Clinica.Domain.DTOs.Roles;
using Clinica.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.API.Controllers;

/// <summary>
/// Controlador para la gestión de roles y la asignación de permisos en el sistema.
/// </summary>
/// <remarks>
/// **Módulo de Seguridad:** Los roles agrupan permisos y definen los niveles de acceso de los usuarios.
/// Este controlador permite gestionar el ciclo de vida completo de los roles: creación, consulta, actualización y asignación de permisos.
/// 
/// **Nota de Arquitectura:** Los roles del sistema (Administrador, Recepcionista, etc.) no pueden ser eliminados ni modificados en sus permisos base.
/// Los roles creados por el usuario pueden ser gestionados libremente.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Tags("Seguridad - Roles")]
public class RolesController : ControllerBase
{
    private readonly IRolService _rolService;

    public RolesController(IRolService rolService)
    {
        _rolService = rolService;
    }

    /// <summary>
    /// Obtiene la lista de todos los roles del sistema.
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Permite consultar el catálogo completo de roles, incluyendo los roles del sistema y los creados por el usuario.
    /// Útil para la administración de seguridad y la asignación de roles a usuarios.
    /// </remarks>
    /// <returns>Lista de objetos <see cref="RolResponseDto"/> con los datos de cada rol.</returns>
    /// <response code="200">Roles obtenidos correctamente.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Acceso denegado.</response>
    [Authorize(Policy = PermisosPolicies.RolVer)]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<RolResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll()
    {
        var roles = await _rolService.ObtenerTodosAsync();
        return Ok(ApiResponse<object>.Ok(roles, "Roles obtenidos correctamente."));
    }

    /// <summary>
    /// Obtiene los detalles de un rol específico por su ID.
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Permite recuperar la información completa de un rol individual, incluyendo sus datos básicos.
    /// Útil para la pantalla de detalle del rol o para validar datos antes de una actualización.
    /// </remarks>
    /// <param name="id">Identificador único del rol (GUID).</param>
    /// <returns>Objeto <see cref="RolResponseDto"/> con los datos del rol.</returns>
    /// <response code="200">Rol obtenido correctamente.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Acceso denegado.</response>
    /// <response code="404">Rol no encontrado.</response>
    [Authorize(Policy = PermisosPolicies.RolVer)]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<RolResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var rol = await _rolService.ObtenerPorIdAsync(id);

        if (rol == null)
            throw new KeyNotFoundException("Rol no encontrado.");

        return Ok(ApiResponse<object>.Ok(rol, "Rol obtenido correctamente."));
    }

    /// <summary>
    /// Crea un nuevo rol en el sistema.
    /// </summary>
    /// <remarks>
    /// **Proceso de negocio:**
    /// 1. Valida que el nombre del rol no esté ya registrado.
    /// 2. Crea el rol con estado "Activo".
    /// 3. El rol no tiene permisos asignados por defecto.
    /// 
    /// **Nota:** Los roles del sistema (Administrador, etc.) no pueden ser creados por este endpoint.
    /// </remarks>
    /// <param name="dto">Objeto <see cref="CrearRolDto"/> con el nombre y descripción del nuevo rol.</param>
    /// <returns>Objeto con el ID del rol creado.</returns>
    /// <response code="201">Rol creado correctamente.</response>
    /// <response code="400">El nombre del rol ya existe.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Permisos insuficientes.</response>
    [Auditoria("Roles", "Rol", TipoAccionAuditoria.Creacion, NivelAuditoria.Critico)]
    [Authorize(Policy = PermisosPolicies.RolCrear)]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CrearRolDto dto)
    {
        var id = await _rolService.CrearAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id },
            ApiResponse<object>.Ok(new { id }, "Rol creado correctamente.", 201)
        );
    }

    /// <summary>
    /// Actualiza los datos de un rol existente.
    /// </summary>
    /// <remarks>
    /// **Proceso de negocio:**
    /// 1. Valida que el rol exista.
    /// 2. Valida que el rol no sea del sistema (no se pueden modificar).
    /// 3. Actualiza el nombre, la descripción y el estado del rol.
    /// 
    /// **Nota de auditoría:** Esta acción queda registrada automáticamente como crítica.
    /// </remarks>
    /// <param name="id">Identificador único del rol a actualizar (GUID).</param>
    /// <param name="dto">Objeto <see cref="EditarRolDto"/> con los datos actualizados.</param>
    /// <response code="200">Rol actualizado correctamente.</response>
    /// <response code="400">El rol es del sistema y no se puede modificar.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Permisos insuficientes.</response>
    /// <response code="404">Rol no encontrado.</response>
    [Auditoria("Roles", "Rol", TipoAccionAuditoria.Edicion, NivelAuditoria.Critico)]
    [Authorize(Policy = PermisosPolicies.RolEditar)]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] EditarRolDto dto)
    {
        await _rolService.ActualizarAsync(id, dto);
        return Ok(ApiResponse<object>.Ok(new { id }, "Rol actualizado correctamente."));
    }

    /// <summary>
    /// Asigna permisos a un rol específico.
    /// </summary>
    /// <remarks>
    /// **Proceso de negocio:**
    /// 1. Valida que el rol exista.
    /// 2. Por cada permiso ID en la lista, valida que el permiso exista.
    /// 3. Asigna los permisos al rol (si no están ya asignados).
    /// 
    /// **Nota:** Los permisos asignados a roles del sistema no pueden ser modificados.
    /// </remarks>
    /// <param name="dto">Objeto <see cref="AsignarPermisosRolDto"/> con el rol y la lista de permisos.</param>
    /// <response code="200">Permisos asignados correctamente.</response>
    /// <response code="400">Uno o más permisos no existen.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Permisos insuficientes.</response>
    /// <response code="404">Rol no encontrado.</response>
    [Auditoria("Roles", "RolPermiso", TipoAccionAuditoria.Asignacion, NivelAuditoria.Critico)]
    [Authorize(Policy = PermisosPolicies.RolAsignarPermisos)]
    [HttpPost("asignar-permisos")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignPermissions([FromBody] AsignarPermisosRolDto dto)
    {
        await _rolService.AsignarPermisosAsync(dto);
        return Ok(ApiResponse<object>.Ok(dto, "Permisos asignados correctamente."));
    }
}