using Clinica.Domain.DTOs.Pagos;
using Clinica.Domain.Entities;
using Clinica.Domain.Enums;
using Clinica.Domain.Interfaces;

namespace Clinica.API.Services.Imp;

public class PagoService : IPagoService
{
    private readonly IPagoRepository _pagoRepository;
    private readonly IPacienteRepository _pacienteRepository;
    private readonly IServicioClinicoRepository _servicioRepository;
    private readonly IAtencionRepository _atencionRepository;
    private readonly IHistorialClinicoRepository _historialRepository;
    private readonly IHistorialDetalleRepository _detalleRepository;
    private readonly IUsuarioActualService _usuarioActualService;

    public PagoService(
        IPagoRepository pagoRepository,
        IPacienteRepository pacienteRepository,
        IServicioClinicoRepository servicioRepository,
        IAtencionRepository atencionRepository,
        IHistorialClinicoRepository historialRepository,
        IHistorialDetalleRepository detalleRepository,
        IUsuarioActualService usuarioActualService)
    {
        _pagoRepository = pagoRepository;
        _pacienteRepository = pacienteRepository;
        _servicioRepository = servicioRepository;
        _atencionRepository = atencionRepository;
        _historialRepository = historialRepository;
        _detalleRepository = detalleRepository;
        _usuarioActualService = usuarioActualService;
    }

    public async Task<IEnumerable<PagoResponseDto>> ObtenerPorPacienteAsync(Guid pacienteId)
    {
        var pagos = await _pagoRepository.ObtenerPorPacienteAsync(pacienteId);
        return pagos.Select(MapearPago);
    }
    
    public async Task CambiarEstadoAsync(Guid id, CambiarEstadoPagoDto dto)
    {
        var pago = await _pagoRepository.GetByIdAsync(id);
        if (pago == null)
            throw new KeyNotFoundException("Pago no encontrado.");
        
        if (pago.Estado == EstadoPago.Eliminado)
            throw new InvalidOperationException("No se puede modificar un pago eliminado.");

        if (dto.Estado == EstadoPago.Eliminado && pago.SaldoPendiente > 0)
            throw new InvalidOperationException("No se puede eliminar un pago con saldo pendiente. Primero regularice la deuda.");

        pago.Estado = dto.Estado;
        _pagoRepository.Update(pago);
        await _pagoRepository.SaveChangesAsync();
    }

    public async Task<IEnumerable<PagoResponseDto>> ObtenerPorCitaAsync(Guid citaId)
    {
        var pagos = await _pagoRepository.ObtenerPorCitaAsync(citaId);
        return pagos.Select(MapearPago);
    }

    public async Task<IEnumerable<PagoResponseDto>> ObtenerPorAtencionAsync(Guid atencionId)
    {
        var pagos = await _pagoRepository.ObtenerPorAtencionAsync(atencionId);
        return pagos.Select(MapearPago);
    }

    public async Task<Guid> RegistrarAsync(RegistrarPagoDto dto)
    {
        var usuarioId = _usuarioActualService.ObtenerUsuarioId();
        
        if (dto.MontoPagado > dto.MontoTotal)
            throw new InvalidOperationException("El monto pagado no puede ser mayor al monto total.");

        if (dto.MontoAdelanto > dto.MontoTotal)
            throw new InvalidOperationException("El monto de adelanto no puede ser mayor al monto total.");

        var paciente = await _pacienteRepository.GetByIdAsync(dto.PacienteId)
            ?? throw new KeyNotFoundException("Paciente no encontrado.");

        var servicio = await _servicioRepository.GetByIdAsync(dto.ServicioClinicoId)
            ?? throw new KeyNotFoundException("Servicio no encontrado.");

        var saldo = dto.MontoTotal - dto.MontoPagado;

        var pago = new Pago
        {
            Id = Guid.NewGuid(),
            CodigoPago = GenerarCodigo("PAG", paciente.DNI),
            PacienteId = dto.PacienteId,
            ServicioClinicoId = dto.ServicioClinicoId,
            CitaId = dto.CitaId,
            AtencionId = dto.AtencionId,
            MontoTotal = dto.MontoTotal,
            MontoPagado = dto.MontoPagado,
            SaldoPendiente = saldo,
            MontoAdelanto = dto.MontoAdelanto,
            MetodoPago = dto.MetodoPago,
            Estado = saldo == 0 ? EstadoPago.Pagado : EstadoPago.Parcial,
            Observacion = dto.Observacion,
            FechaPago = DateTime.UtcNow,
            UsuarioRegistroId = usuarioId
        };

        await _pagoRepository.AddAsync(pago);

        if (dto.AtencionId.HasValue)
        {
            var atencion = await _atencionRepository.GetByIdAsync(dto.AtencionId.Value);
            if (atencion != null)
            {
                atencion.MontoPagado += dto.MontoPagado;
                atencion.SaldoPendiente = atencion.CostoFinal - atencion.MontoPagado;

                _atencionRepository.Update(atencion);
            }
        }

        var historial = await _historialRepository.ObtenerPorPacienteAsync(dto.PacienteId);

        if (historial != null)
        {
            await _detalleRepository.AddAsync(new HistorialDetalle
            {
                Id = Guid.NewGuid(),
                CodigoDetalle = GenerarCodigo(servicio.CodigoServicio, paciente.DNI),
                HistorialClinicoId = historial.Id,
                PagoId = pago.Id,
                TipoMovimiento = TipoMovimientoHistorial.PagoRegistrado,
                Titulo = "Pago registrado",
                Descripcion = $"Se registró pago de S/ {dto.MontoPagado}.",
                FechaRegistro = DateTime.UtcNow,
                UsuarioId = usuarioId
            });
        }

        await _pagoRepository.SaveChangesAsync();

        return pago.Id;
    }

    private static PagoResponseDto MapearPago(Pago x)
    {
        return new PagoResponseDto
        {
            Id = x.Id,
            CodigoPago = x.CodigoPago,
            PacienteId = x.PacienteId,
            PacienteNombre = x.Paciente == null ? "" : $"{x.Paciente.Nombres} {x.Paciente.Apellidos}",
            ServicioClinicoId = x.ServicioClinicoId,
            ServicioNombre = x.ServicioClinico?.Nombre ?? "",
            CitaId = x.CitaId,
            AtencionId = x.AtencionId,
            MontoTotal = x.MontoTotal,
            MontoPagado = x.MontoPagado,
            SaldoPendiente = x.SaldoPendiente,
            MontoAdelanto = x.MontoAdelanto,
            MetodoPago = x.MetodoPago,
            Estado = x.Estado,
            Observacion = x.Observacion,
            FechaPago = x.FechaPago
        };
    }

    private static string GenerarCodigo(string prefijo, string dni)
    {
        return $"{Guid.NewGuid().ToString("N")[..5].ToUpper()}-{prefijo}-{DateTime.UtcNow:yyyy}-{dni}";
    }
}