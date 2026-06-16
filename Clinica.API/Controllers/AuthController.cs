using Clinica.API.Filters;
using Clinica.API.Models;
using Clinica.API.Services;
using Clinica.Domain.DTOs.Auth;
using Clinica.Domain.Enums;
using Microsoft.AspNetCore.Authorization; // <-- Asegúrate de tener este using para el AllowAnonymous
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.API.Controllers;

/// <summary>
/// Controlador de autenticación y gestión de sesiones de usuario.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Autenticación y Seguridad")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Inicia sesión de un usuario en el sistema.
    /// </summary>
    /// <remarks>
    /// **Proceso de autenticación:**
    /// - Valida las credenciales contra la base de datos.
    /// - Si son correctas, genera un token JWT válido por el tiempo configurado (120 minutos por defecto).
    /// - El token incluye los roles y permisos del usuario para la autorización posterior.
    /// 
    /// **Posibles errores:**
    /// - Si el usuario no existe o la contraseña es incorrecta → se lanza una excepción genérica.
    /// - Si la cuenta está inactiva → se indica que el usuario debe contactar al administrador.
    /// </remarks>
    /// <param name="dto">Credenciales de acceso (usuario/correo y contraseña).</param>
    /// <returns>Objeto con el token JWT, datos del usuario y listas de roles y permisos.</returns>
    /// <response code="200">Inicio de sesión exitoso. Retorna el token y datos del usuario.</response>
    /// <response code="400">Las credenciales proporcionadas son inválidas.</response>
    /// <response code="403">La cuenta del usuario está inactiva o bloqueada.</response>
    [AllowAnonymous] // <--- ¡ESTO ES CLAVE! Permite entrar aquí sin tener un JWT previo
    [Auditoria("Seguridad", "Usuario", TipoAccionAuditoria.Login, NivelAuditoria.Critico)]
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<RespuestaInicioSesionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Login([FromBody] IniciarSesionDto dto)
    {
        var respuesta = await _authService.IniciarSesionAsync(dto);

        return Ok(ApiResponse<object>.Ok(
            respuesta,
            "Inicio de sesión correcto."
        ));
    }
}