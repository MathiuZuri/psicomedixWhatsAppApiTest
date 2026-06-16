using Clinica.Domain.Entities;

namespace Clinica.Domain.Interfaces;

public interface IPagoRepository : IGenericRepository<Pago>
{
    Task<IEnumerable<Pago>> ObtenerPorPacienteAsync(Guid pacienteId);
    Task<IEnumerable<Pago>> ObtenerPorCitaAsync(Guid citaId);
    Task<IEnumerable<Pago>> ObtenerPorAtencionAsync(Guid atencionId);

    Task<IEnumerable<Pago>> ObtenerTodosConDetalleAsync();
    Task<Pago?> ObtenerPorCodigoConDetalleAsync(string codigoPago);
}