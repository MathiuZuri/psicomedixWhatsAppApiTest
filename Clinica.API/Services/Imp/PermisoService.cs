using Clinica.Domain.DTOs.Permisos;
using Clinica.Domain.Interfaces;

namespace Clinica.API.Services.Imp;

public class PermisoService : IPermisoService
{
    private readonly IPermisoRepository _permisoRepository;

    public PermisoService(IPermisoRepository permisoRepository)
    {
        _permisoRepository = permisoRepository;
    }

    public async Task<IEnumerable<PermisoResponseDto>> ObtenerTodosAsync()
    {
        var permisos = await _permisoRepository.GetAllAsync();

        return permisos.Select(x => new PermisoResponseDto
        {
            Id = x.Id,
            Codigo = x.Codigo,
            Nombre = x.Nombre,
            Modulo = x.Modulo,
            Descripcion = x.Descripcion,
            Activo = x.Activo
        });
    }
}