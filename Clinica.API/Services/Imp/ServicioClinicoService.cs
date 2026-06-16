using Clinica.Domain.DTOs.Servicios;
using Clinica.Domain.Interfaces;

namespace Clinica.API.Services.Imp;

public class ServicioClinicoService : IServicioClinicoService
{
    private readonly IServicioClinicoRepository _servicioRepository;

    public ServicioClinicoService(IServicioClinicoRepository servicioRepository)
    {
        _servicioRepository = servicioRepository;
    }

    public async Task<IEnumerable<ServicioClinicoResponseDto>> ObtenerTodosAsync()
    {
        var servicios = await _servicioRepository.GetAllAsync();

        return servicios.Select(MapearServicio);
    }

    public async Task<IEnumerable<ServicioClinicoResponseDto>> ObtenerActivosAsync()
    {
        var servicios = await _servicioRepository.ObtenerActivosAsync();

        return servicios.Select(MapearServicio);
    }

    public async Task<ServicioClinicoResponseDto?> ObtenerPorIdAsync(Guid id)
    {
        var servicio = await _servicioRepository.GetByIdAsync(id);
        if (servicio == null) return null;

        return MapearServicio(servicio);
    }

    private static ServicioClinicoResponseDto MapearServicio(Clinica.Domain.Entities.ServicioClinico servicio)
    {
        return new ServicioClinicoResponseDto
        {
            Id = servicio.Id,
            CodigoServicio = servicio.CodigoServicio,
            Nombre = servicio.Nombre,
            Descripcion = servicio.Descripcion,
            CostoBase = servicio.CostoBase,
            DuracionMinutos = servicio.DuracionMinutos,
            RequiereCita = servicio.RequiereCita,
            GeneraHistorial = servicio.GeneraHistorial,
            Estado = servicio.Estado
        };
    }
}