using Clinica.API.Helpers;
using Clinica.Domain.DTOs.Auth;
using Clinica.Domain.Enums;         
using Clinica.Domain.Interfaces;

namespace Clinica.API.Services.Imp;

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly JwtHelper _jwtHelper;

    public AuthService(IUsuarioRepository usuarioRepository, JwtHelper jwtHelper)
    {
        _usuarioRepository = usuarioRepository;
        _jwtHelper = jwtHelper;
    }

    public async Task<RespuestaInicioSesionDto> IniciarSesionAsync(IniciarSesionDto dto)
    {
        var usuario = await _usuarioRepository.ObtenerPorCorreoAsync(dto.UsuarioOCorreo)
                      ?? await _usuarioRepository.ObtenerPorUserNameAsync(dto.UsuarioOCorreo);

        if (usuario == null)
            throw new InvalidOperationException("Usuario o contraseña incorrectos.");
        
        if (usuario.Estado != EstadoUsuario.Activo)
            throw new InvalidOperationException("Tu cuenta no está activa. Contacta al administrador.");

        var passwordValido = BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash);

        if (!passwordValido)
            throw new InvalidOperationException("Usuario o contraseña incorrectos.");

        var roles = usuario.UsuarioRoles
            .Where(x => x.Activo)
            .Select(x => x.Rol.Nombre)
            .Distinct()
            .ToList();

        var permisos = usuario.UsuarioRoles
            .Where(x => x.Activo)
            .SelectMany(x => x.Rol.RolPermisos)
            .Select(x => x.Permiso.Codigo)
            .Distinct()
            .ToList();

        var token = _jwtHelper.GenerarToken(usuario, roles, permisos);

        return new RespuestaInicioSesionDto
        {
            UsuarioId = usuario.Id,
            CodigoUsuario = usuario.CodigoUsuario,
            NombreCompleto = $"{usuario.Nombres} {usuario.Apellidos}",
            Correo = usuario.Correo,
            Token = token,
            Roles = roles,
            Permisos = permisos
        };
    }
}