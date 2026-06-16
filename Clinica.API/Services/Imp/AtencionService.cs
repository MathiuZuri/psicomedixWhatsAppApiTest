using Clinica.Domain.DTOs.Atenciones;
using Clinica.Domain.Entities;
using Clinica.Domain.Enums;
using Clinica.Domain.Interfaces;

namespace Clinica.API.Services.Imp;

public class AtencionService : IAtencionService
{
    private readonly IAtencionRepository _atencionRepository;
    private readonly ICitaRepository _citaRepository;
    private readonly IPacienteRepository _pacienteRepository;
    private readonly IDoctorRepository _doctorRepository;
    private readonly IServicioClinicoRepository _servicioRepository;
    private readonly IHistorialClinicoRepository _historialRepository;
    private readonly IHistorialDetalleRepository _detalleRepository;
    private readonly IUsuarioActualService _usuarioActualService;

    public AtencionService(
        IAtencionRepository atencionRepository,
        ICitaRepository citaRepository,
        IPacienteRepository pacienteRepository,
        IDoctorRepository doctorRepository,
        IServicioClinicoRepository servicioRepository,
        IHistorialClinicoRepository historialRepository,
        IHistorialDetalleRepository detalleRepository,
        IUsuarioActualService usuarioActualService)
    {
        _atencionRepository = atencionRepository;
        _citaRepository = citaRepository;
        _pacienteRepository = pacienteRepository;
        _doctorRepository = doctorRepository;
        _servicioRepository = servicioRepository;
        _historialRepository = historialRepository;
        _detalleRepository = detalleRepository;
        _usuarioActualService = usuarioActualService;
    }

    public async Task<IEnumerable<AtencionResponseDto>> ObtenerPorPacienteAsync(Guid pacienteId)
    {
        var atenciones = await _atencionRepository.ObtenerPorPacienteAsync(pacienteId);
        return atenciones.Select(MapearAtencion);
    }

    public async Task<AtencionResponseDto?> ObtenerPorCitaAsync(Guid citaId)
    {
        var atencion = await _atencionRepository.ObtenerPorCitaAsync(citaId);
        if (atencion == null) return null;

        return MapearAtencion(atencion);
    }

    public async Task<Guid> RegistrarAsync(RegistrarAtencionDto dto)
    {
        var usuarioId = _usuarioActualService.ObtenerUsuarioId();

        var paciente = await _pacienteRepository.GetByIdAsync(dto.PacienteId)
            ?? throw new KeyNotFoundException("Paciente no encontrado.");

        var doctor = await _doctorRepository.GetByIdAsync(dto.DoctorId)
            ?? throw new KeyNotFoundException("Doctor no encontrado.");

        var servicio = await _servicioRepository.GetByIdAsync(dto.ServicioClinicoId)
            ?? throw new KeyNotFoundException("Servicio no encontrado.");

        var historial = await _historialRepository.GetByIdAsync(dto.HistorialClinicoId)
            ?? throw new KeyNotFoundException("Historial no encontrado.");
        
        if (dto.CostoFinal < 0)
            throw new InvalidOperationException("El costo final no puede ser negativo.");

        var atencion = new Atencion
        {
            Id = Guid.NewGuid(),
            CodigoAtencion = GenerarCodigo("ATE", paciente.DNI),
            PacienteId = dto.PacienteId,
            DoctorId = dto.DoctorId,
            ServicioClinicoId = dto.ServicioClinicoId,
            CitaId = dto.CitaId,
            HistorialClinicoId = dto.HistorialClinicoId,
            FechaInicio = DateTime.UtcNow,
            MotivoConsulta = dto.MotivoConsulta,
            Observaciones = dto.Observaciones,
            DiagnosticoResumen = dto.DiagnosticoResumen,
            Indicaciones = dto.Indicaciones,
            Tratamiento = dto.Tratamiento,
            Estado = EstadoAtencion.Abierta,
            CostoFinal = dto.CostoFinal,
            MontoPagado = 0,
            SaldoPendiente = dto.CostoFinal,
            UsuarioRegistroId = usuarioId
        };

        await _atencionRepository.AddAsync(atencion);

        if (dto.CitaId.HasValue)
        {
            var cita = await _citaRepository.GetByIdAsync(dto.CitaId.Value);
            if (cita != null)
            {
                cita.Estado = EstadoCita.Atendida;
                _citaRepository.Update(cita);
            }
        }

        await _detalleRepository.AddAsync(new HistorialDetalle
        {
            Id = Guid.NewGuid(),
            CodigoDetalle = GenerarCodigo(servicio.CodigoServicio, paciente.DNI),
            HistorialClinicoId = historial.Id,
            AtencionId = atencion.Id,
            TipoMovimiento = TipoMovimientoHistorial.AtencionRegistrada,
            Titulo = "Atención médica registrada",
            Descripcion = $"Se registró atención para el servicio {servicio.Nombre}.",
            FechaRegistro = DateTime.UtcNow,
            UsuarioId = usuarioId
        });

        await _atencionRepository.SaveChangesAsync();

        return atencion.Id;
    }

    public async Task CerrarAsync(Guid id, CerrarAtencionDto dto)
    {
        var atencion = await _atencionRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Atención no encontrada.");

        atencion.DiagnosticoResumen = dto.DiagnosticoResumen ?? atencion.DiagnosticoResumen;
        atencion.Indicaciones = dto.Indicaciones ?? atencion.Indicaciones;
        atencion.Tratamiento = dto.Tratamiento ?? atencion.Tratamiento;
        atencion.Observaciones = dto.ObservacionesFinales ?? atencion.Observaciones;

        atencion.FechaCierre = DateTime.UtcNow;
        atencion.Estado = EstadoAtencion.Cerrada;

        _atencionRepository.Update(atencion);
        await _atencionRepository.SaveChangesAsync();
    }

    private static AtencionResponseDto MapearAtencion(Atencion x)
    {
        return new AtencionResponseDto
        {
            Id = x.Id,
            CodigoAtencion = x.CodigoAtencion,
            PacienteId = x.PacienteId,
            PacienteNombre = x.Paciente == null ? "" : $"{x.Paciente.Nombres} {x.Paciente.Apellidos}",
            DoctorId = x.DoctorId,
            DoctorNombre = x.Doctor == null ? "" : $"{x.Doctor.Nombres} {x.Doctor.Apellidos}",
            ServicioClinicoId = x.ServicioClinicoId,
            ServicioNombre = x.ServicioClinico?.Nombre ?? "",
            CitaId = x.CitaId,
            HistorialClinicoId = x.HistorialClinicoId,
            FechaInicio = x.FechaInicio,
            FechaCierre = x.FechaCierre,
            MotivoConsulta = x.MotivoConsulta,
            Observaciones = x.Observaciones,
            DiagnosticoResumen = x.DiagnosticoResumen,
            Indicaciones = x.Indicaciones,
            Tratamiento = x.Tratamiento,
            Estado = x.Estado,
            CostoFinal = x.CostoFinal,
            MontoPagado = x.MontoPagado,
            SaldoPendiente = x.SaldoPendiente
        };
    }

    private static string GenerarCodigo(string prefijo, string dni)
    {
        return $"{Guid.NewGuid().ToString("N")[..5].ToUpper()}-{prefijo}-{DateTime.UtcNow:yyyy}-{dni}";
    }
}