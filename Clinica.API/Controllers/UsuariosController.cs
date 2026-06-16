using Clinica.API.Authorization;
using Clinica.API.Filters;
using Clinica.API.Models;
using Clinica.API.Services;
using Clinica.Domain.DTOs.Usuarios;
using Clinica.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.API.Controllers;

/// <summary>
/// Controlador para la gestión de usuarios del sistema.
/// </summary>
/// <remarks>
/// **Módulo de Seguridad:** Este controlador permite gestionar el ciclo de vida completo de los usuarios: creación, consulta, actualización, cambio de estado y asignación de roles.
/// 
/// **Nota de Arquitectura:** Los usuarios son el pilar de la autenticación y autorización en el sistema.
/// El usuario administrador principal (con código "USR-ADMIN") no puede ser desactivado ni eliminado.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Tags("Seguridad - Usuarios")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public UsuariosController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    /// <summary>
    /// Obtiene la lista de todos los usuarios del sistema.
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Permite consultar el catálogo completo de usuarios, incluyendo sus datos básicos.
    /// Útil para la administración de personal y la asignación de roles.
    /// </remarks>
    /// <returns>Lista de objetos <see cref="UsuarioResponseDto"/> con los datos de cada usuario.</returns>
    /// <response code="200">Usuarios obtenidos correctamente.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Acceso denegado.</response>
    [Authorize(Policy = PermisosPolicies.UsuarioVer)]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<UsuarioResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll()
    {
        var usuarios = await _usuarioService.ObtenerTodosAsync();
        return Ok(ApiResponse<object>.Ok(usuarios, "Usuarios obtenidos correctamente."));
    }

    /// <summary>
    /// Obtiene los datos de un usuario específico por su ID.
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Permite recuperar la información completa de un usuario individual.
    /// Útil para la pantalla de detalle del usuario o para validar datos antes de una actualización.
    /// </remarks>
    /// <param name="id">Identificador único del usuario (GUID).</param>
    /// <returns>Objeto <see cref="UsuarioResponseDto"/> con los datos del usuario.</returns>
    /// <response code="200">Usuario obtenido correctamente.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Acceso denegado.</response>
    /// <response code="404">Usuario no encontrado.</response>
    [Authorize(Policy = PermisosPolicies.UsuarioVer)]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UsuarioResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var usuario = await _usuarioService.ObtenerPorIdAsync(id);

        if (usuario == null)
            throw new KeyNotFoundException("Usuario no encontrado.");

        return Ok(ApiResponse<object>.Ok(usuario, "Usuario obtenido correctamente."));
    }

    /// <summary>
    /// Crea un nuevo usuario en el sistema.
    /// </summary>
    /// <remarks>
    /// **Proceso de negocio:**
    /// 1. Valida que el correo no esté ya registrado.
    /// 2. Valida que el nombre de usuario no esté ya registrado.
    /// 3. Hash de la contraseña utilizando BCrypt.
    /// 4. Crea el usuario con estado "Activo".
    /// 
    /// **Nota:** El usuario creado no tiene ningún rol asignado por defecto.
    /// </remarks>
    /// <param name="dto">Objeto <see cref="CrearUsuarioDto"/> con los datos del nuevo usuario.</param>
    /// <returns>Objeto con el ID del usuario creado.</returns>
    /// <response code="201">Usuario creado correctamente.</response>
    /// <response code="400">Correo o nombre de usuario ya existen.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Permisos insuficientes.</response>
    [Auditoria("Usuarios", "Usuario", TipoAccionAuditoria.Creacion, NivelAuditoria.Critico)]
    [Authorize(Policy = PermisosPolicies.UsuarioCrear)]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CrearUsuarioDto dto)
    {
        var id = await _usuarioService.CrearAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id },
            ApiResponse<object>.Ok(new { id }, "Usuario creado correctamente.", 201)
        );
    }

    /// <summary>
    /// Actualiza los datos de un usuario existente.
    /// </summary>
    /// <remarks>
    /// **Proceso de negocio:**
    /// 1. Valida que el usuario exista.
    /// 2. Actualiza los campos permitidos (nombres, apellidos, nombre de usuario, correo).
    /// 
    /// **Nota de auditoría:** Esta acción queda registrada automáticamente como crítica.
    /// </remarks>
    /// <param name="id">Identificador único del usuario a actualizar (GUID).</param>
    /// <param name="dto">Objeto <see cref="EditarUsuarioDto"/> con los datos actualizados.</param>
    /// <response code="200">Usuario actualizado correctamente.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Permisos insuficientes.</response>
    /// <response code="404">Usuario no encontrado.</response>
    [Auditoria("Usuarios", "Usuario", TipoAccionAuditoria.Edicion, NivelAuditoria.Critico)]
    [Authorize(Policy = PermisosPolicies.UsuarioEditar)]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] EditarUsuarioDto dto)
    {
        await _usuarioService.ActualizarAsync(id, dto);
        return Ok(ApiResponse<object>.Ok(new { id }, "Usuario actualizado correctamente."));
    }

    /// <summary>
    /// Asigna un rol a un usuario específico.
    /// </summary>
    /// <remarks>
    /// **Proceso de negocio:**
    /// 1. Valida que el usuario exista.
    /// 2. Valida que el rol exista.
    /// 3. Verifica que el usuario no tenga ya asignado ese rol.
    /// 4. Asigna el rol al usuario.
    /// 
    /// **Nota:** Un usuario puede tener múltiples roles, y todos sus permisos se combinan.
    /// </remarks>
    /// <param name="dto">Objeto <see cref="AsignarRolUsuarioDto"/> con el usuario y el rol.</param>
    /// <response code="200">Rol asignado correctamente.</response>
    /// <response code="400">El usuario ya tiene asignado ese rol.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Permisos insuficientes.</response>
    /// <response code="404">Usuario o rol no encontrado.</response>
    [Auditoria("Usuarios", "UsuarioRol", TipoAccionAuditoria.Asignacion, NivelAuditoria.Critico)]
    [Authorize(Policy = PermisosPolicies.UsuarioAsignarRol)]
    [HttpPost("asignar-rol")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignRole([FromBody] AsignarRolUsuarioDto dto)
    {
        await _usuarioService.AsignarRolAsync(dto);
        return Ok(ApiResponse<object>.Ok(dto, "Rol asignado correctamente."));
    }

    /// <summary>
    /// Cambia el estado de un usuario.
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Permite activar, desactivar o bloquear un usuario.
    /// 
    /// **Nota de Arquitectura:** La "eliminación" de un usuario es lógica y se realiza asignando el estado "Eliminado".
    /// El administrador principal (código "USR-ADMIN") no puede ser desactivado.
    /// </remarks>
    /// <param name="id">Identificador único del usuario (GUID).</param>
    /// <param name="dto">Objeto <see cref="CambiarEstadoUsuarioDto"/> con el nuevo estado.</param>
    /// <response code="200">Estado del usuario actualizado correctamente.</response>
    /// <response code="400">No se puede desactivar al administrador principal.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Permisos insuficientes.</response>
    /// <response code="404">Usuario no encontrado.</response>
    [Auditoria("Usuarios", "Usuario", TipoAccionAuditoria.Edicion, NivelAuditoria.Critico)]
    [Authorize(Policy = PermisosPolicies.UsuarioEditar)]
    [HttpPut("{id:guid}/estado")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CambiarEstado(Guid id, [FromBody] CambiarEstadoUsuarioDto dto)
    {
        await _usuarioService.CambiarEstadoAsync(id, dto);
        return Ok(ApiResponse<object>.Ok(new { id }, "Estado del usuario actualizado correctamente."));
    }
}