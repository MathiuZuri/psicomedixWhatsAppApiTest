using Clinica.Domain.DTOs.Roles;
using Clinica.Domain.Entities;
using Clinica.Domain.Interfaces;

namespace Clinica.API.Services.Imp;

public class RolService : IRolService
{
    private readonly IRolRepository _rolRepository;
    private readonly IPermisoRepository _permisoRepository;

    public RolService(IRolRepository rolRepository, IPermisoRepository permisoRepository)
    {
        _rolRepository = rolRepository;
        _permisoRepository = permisoRepository;
    }

    public async Task<IEnumerable<RolResponseDto>> ObtenerTodosAsync()
    {
        var roles = await _rolRepository.GetAllAsync();

        return roles.Select(x => new RolResponseDto
        {
            Id = x.Id,
            Nombre = x.Nombre,
            Descripcion = x.Descripcion,
            EsSistema = x.EsSistema,
            Activo = x.Activo,
            FechaCreacion = x.FechaCreacion
        });
    }

    public async Task<RolResponseDto?> ObtenerPorIdAsync(Guid id)
    {
        var rol = await _rolRepository.GetByIdAsync(id);
        if (rol == null) return null;

        return new RolResponseDto
        {
            Id = rol.Id,
            Nombre = rol.Nombre,
            Descripcion = rol.Descripcion,
            EsSistema = rol.EsSistema,
            Activo = rol.Activo,
            FechaCreacion = rol.FechaCreacion
        };
    }

    public async Task<Guid> CrearAsync(CrearRolDto dto)
    {
        var existe = await _rolRepository.ObtenerPorNombreAsync(dto.Nombre);
        if (existe != null)
            throw new InvalidOperationException("Ya existe un rol con ese nombre.");

        var rol = new Rol
        {
            Id = Guid.NewGuid(),
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            EsSistema = false,
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };

        await _rolRepository.AddAsync(rol);
        await _rolRepository.SaveChangesAsync();

        return rol.Id;
    }

    public async Task ActualizarAsync(Guid id, EditarRolDto dto)
    {
        var rol = await _rolRepository.GetByIdAsync(id);
        if (rol == null)
            throw new KeyNotFoundException("Rol no encontrado.");

        if (rol.EsSistema)
            throw new InvalidOperationException("No se puede editar un rol del sistema.");

        rol.Nombre = dto.Nombre;
        rol.Descripcion = dto.Descripcion;
        rol.Activo = dto.Activo;

        _rolRepository.Update(rol);
        await _rolRepository.SaveChangesAsync();
    }

    public async Task AsignarPermisosAsync(AsignarPermisosRolDto dto)
    {
        var rol = await _rolRepository.GetByIdAsync(dto.RolId);
        if (rol == null)
            throw new KeyNotFoundException("Rol no encontrado.");

        foreach (var permisoId in dto.PermisosIds.Distinct())
        {
            var permiso = await _permisoRepository.GetByIdAsync(permisoId);
            if (permiso == null)
                throw new KeyNotFoundException("Uno o más permisos no fueron encontrados.");

            var yaTienePermiso = await _rolRepository.TienePermisoAsignadoAsync(
                dto.RolId,
                permisoId
            );

            if (yaTienePermiso)
                continue;

            await _rolRepository.AgregarPermisoAsync(new RolPermiso
            {
                RolId = dto.RolId,
                PermisoId = permisoId,
                FechaAsignacion = DateTime.UtcNow
            });
        }

        await _rolRepository.SaveChangesAsync();
    }
}