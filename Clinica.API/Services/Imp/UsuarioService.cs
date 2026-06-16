using Clinica.Domain.DTOs.Usuarios;
using Clinica.Domain.Entities;
using Clinica.Domain.Enums;
using Clinica.Domain.Interfaces;

namespace Clinica.API.Services.Imp;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IRolRepository _rolRepository;

    public UsuarioService(IUsuarioRepository usuarioRepository, IRolRepository rolRepository)
    {
        _usuarioRepository = usuarioRepository;
        _rolRepository = rolRepository;
    }

    public async Task<IEnumerable<UsuarioResponseDto>> ObtenerTodosAsync()
    {
        var usuarios = await _usuarioRepository.GetAllAsync();

        return usuarios.Select(x => new UsuarioResponseDto
        {
            Id = x.Id,
            CodigoUsuario = x.CodigoUsuario,
            Nombres = x.Nombres,
            Apellidos = x.Apellidos,
            UserName = x.UserName,
            Correo = x.Correo,
            Estado = x.Estado,
            FechaRegistro = x.FechaRegistro,
            UltimoAcceso = x.UltimoAcceso
        });
    }
    
    public async Task CambiarEstadoAsync(Guid id, CambiarEstadoUsuarioDto dto)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(id);
        if (usuario == null)
            throw new KeyNotFoundException("Usuario no encontrado.");

        if (usuario.CodigoUsuario.StartsWith("USR-ADMIN") && dto.Estado != EstadoUsuario.Activo)
            throw new InvalidOperationException("No se puede desactivar al administrador principal del sistema.");

        usuario.Estado = dto.Estado;
        _usuarioRepository.Update(usuario);
        await _usuarioRepository.SaveChangesAsync();
    }

    public async Task<UsuarioResponseDto?> ObtenerPorIdAsync(Guid id)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(id);

        if (usuario == null) return null;

        return new UsuarioResponseDto
        {
            Id = usuario.Id,
            CodigoUsuario = usuario.CodigoUsuario,
            Nombres = usuario.Nombres,
            Apellidos = usuario.Apellidos,
            UserName = usuario.UserName,
            Correo = usuario.Correo,
            Estado = usuario.Estado,
            FechaRegistro = usuario.FechaRegistro,
            UltimoAcceso = usuario.UltimoAcceso
        };
    }

    public async Task<Guid> CrearAsync(CrearUsuarioDto dto)
    {
        var existeCorreo = await _usuarioRepository.ObtenerPorCorreoAsync(dto.Correo);
        if (existeCorreo != null)
            throw new InvalidOperationException("Ya existe un usuario con ese correo.");

        var existeUserName = await _usuarioRepository.ObtenerPorUserNameAsync(dto.UserName);
        if (existeUserName != null)
            throw new InvalidOperationException("Ya existe un usuario con ese nombre de usuario.");

        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            CodigoUsuario = GenerarCodigoUsuario(),
            Nombres = dto.Nombres,
            Apellidos = dto.Apellidos,
            UserName = dto.UserName,
            Correo = dto.Correo,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            FechaRegistro = DateTime.UtcNow
        };

        await _usuarioRepository.AddAsync(usuario);
        await _usuarioRepository.SaveChangesAsync();

        return usuario.Id;
    }

    public async Task ActualizarAsync(Guid id, EditarUsuarioDto dto)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(id);
        if (usuario == null)
            throw new KeyNotFoundException("Usuario no encontrado.");

        usuario.Nombres = dto.Nombres;
        usuario.Apellidos = dto.Apellidos;
        usuario.UserName = dto.UserName;
        usuario.Correo = dto.Correo;

        _usuarioRepository.Update(usuario);
        await _usuarioRepository.SaveChangesAsync();
    }

    public async Task AsignarRolAsync(AsignarRolUsuarioDto dto)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(dto.UsuarioId);
        if (usuario == null)
            throw new KeyNotFoundException("Usuario no encontrado.");

        var rol = await _rolRepository.GetByIdAsync(dto.RolId);
        if (rol == null)
            throw new KeyNotFoundException("Rol no encontrado.");

        var yaTieneRol = await _usuarioRepository.TieneRolAsignadoAsync(
            dto.UsuarioId,
            dto.RolId
        );

        if (yaTieneRol)
            throw new InvalidOperationException("El usuario ya tiene asignado ese rol.");

        await _usuarioRepository.AgregarRolAsync(new UsuarioRol
        {
            UsuarioId = dto.UsuarioId,
            RolId = dto.RolId,
            FechaAsignacion = DateTime.UtcNow,
            Activo = true
        });

        await _usuarioRepository.SaveChangesAsync();
    }

    private static string GenerarCodigoUsuario()
    {
        return $"USR-{DateTime.UtcNow:yyyy}-{Guid.NewGuid().ToString("N")[..5].ToUpper()}";
    }
}