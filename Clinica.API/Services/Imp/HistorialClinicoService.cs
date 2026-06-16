using Clinica.Domain.DTOs.Historiales;
using Clinica.Domain.Interfaces;

namespace Clinica.API.Services.Imp;

public class HistorialClinicoService : IHistorialClinicoService
{
    private readonly IHistorialClinicoRepository _historialRepository;
    private readonly IHistorialDetalleRepository _detalleRepository;

    public HistorialClinicoService(
        IHistorialClinicoRepository historialRepository,
        IHistorialDetalleRepository detalleRepository)
    {
        _historialRepository = historialRepository;
        _detalleRepository = detalleRepository;
    }

    public async Task<HistorialClinicoResponseDto?> ObtenerPorPacienteAsync(Guid pacienteId)
    {
        var historial = await _historialRepository.ObtenerPorPacienteAsync(pacienteId);
        if (historial == null) return null;

        var detalles = await _detalleRepository.ObtenerPorHistorialAsync(historial.Id);

        return MapearHistorial(historial, detalles);
    }

    public async Task<HistorialClinicoResponseDto?> ObtenerConDetallesAsync(Guid historialId)
    {
        var historial = await _historialRepository.ObtenerConDetallesAsync(historialId);
        if (historial == null) return null;

        return MapearHistorial(historial, historial.Detalles);
    }

    private static HistorialClinicoResponseDto MapearHistorial(
        Clinica.Domain.Entities.HistorialClinico historial,
        IEnumerable<Clinica.Domain.Entities.HistorialDetalle> detalles)
    {
        return new HistorialClinicoResponseDto
        {
            Id = historial.Id,
            CodigoHistorial = historial.CodigoHistorial,
            PacienteId = historial.PacienteId,
            PacienteNombre = historial.Paciente == null
                ? string.Empty
                : $"{historial.Paciente.Nombres} {historial.Paciente.Apellidos}",
            PacienteDni = historial.Paciente?.DNI ?? string.Empty,
            FechaApertura = historial.FechaApertura,
            Estado = historial.Estado,
            Detalles = detalles.Select(d => new HistorialDetalleResponseDto
            {
                Id = d.Id,
                CodigoDetalle = d.CodigoDetalle,
                HistorialClinicoId = d.HistorialClinicoId,
                TipoMovimiento = d.TipoMovimiento,
                CitaId = d.CitaId,
                AtencionId = d.AtencionId,
                PagoId = d.PagoId,
                Titulo = d.Titulo,
                Descripcion = d.Descripcion,
                FechaRegistro = d.FechaRegistro,
                UsuarioId = d.UsuarioId,
                UsuarioNombre = d.Usuario == null ? null : $"{d.Usuario.Nombres} {d.Usuario.Apellidos}"
            }).ToList()
        };
    }
}