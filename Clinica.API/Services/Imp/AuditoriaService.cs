using Clinica.Domain.DTOs.Auditoria;
using Clinica.Domain.Interfaces;

namespace Clinica.API.Services.Imp;

public class AuditoriaService : IAuditoriaService
{
    private readonly IAuditoriaRepository _auditoriaRepository;

    public AuditoriaService(IAuditoriaRepository auditoriaRepository)
    {
        _auditoriaRepository = auditoriaRepository;
    }

    public async Task<IEnumerable<AuditoriaResponseDto>> ObtenerTodosAsync()
    {
        var auditorias = await _auditoriaRepository.GetAllAsync();

        return auditorias.Select(x => new AuditoriaResponseDto
        {
            Id = x.Id,
            UsuarioId = x.UsuarioId,
            UsuarioNombre = x.Usuario == null ? null : $"{x.Usuario.Nombres} {x.Usuario.Apellidos}",
            TipoAccion = x.TipoAccion,
            Modulo = x.Modulo,
            EntidadAfectada = x.EntidadAfectada,
            EntidadId = x.EntidadId,
            Descripcion = x.Descripcion,
            ValorAnterior = x.ValorAnterior,
            ValorNuevo = x.ValorNuevo,
            IpAddress = x.IpAddress,
            UserAgent = x.UserAgent,
            FueExitoso = x.FueExitoso,
            DetalleError = x.DetalleError,
            Nivel = x.Nivel,
            FechaHora = x.FechaHora
        });
    }

    public async Task<IEnumerable<AuditoriaResponseDto>> ObtenerPorUsuarioAsync(Guid usuarioId)
    {
        var auditorias = await _auditoriaRepository.ObtenerPorUsuarioAsync(usuarioId);

        return auditorias.Select(x => new AuditoriaResponseDto
        {
            Id = x.Id,
            UsuarioId = x.UsuarioId,
            UsuarioNombre = x.Usuario == null ? null : $"{x.Usuario.Nombres} {x.Usuario.Apellidos}",
            TipoAccion = x.TipoAccion,
            Modulo = x.Modulo,
            EntidadAfectada = x.EntidadAfectada,
            EntidadId = x.EntidadId,
            Descripcion = x.Descripcion,
            ValorAnterior = x.ValorAnterior,
            ValorNuevo = x.ValorNuevo,
            IpAddress = x.IpAddress,
            UserAgent = x.UserAgent,
            FueExitoso = x.FueExitoso,
            DetalleError = x.DetalleError,
            Nivel = x.Nivel,
            FechaHora = x.FechaHora
        });
    }
}