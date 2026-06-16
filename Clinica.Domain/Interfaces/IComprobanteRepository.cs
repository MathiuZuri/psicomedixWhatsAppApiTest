using Clinica.Domain.Entities;

namespace Clinica.Domain.Interfaces;

public interface IComprobanteRepository : IGenericRepository<Comprobante>
{
    Task<IEnumerable<Comprobante>> ObtenerTodosConDetalleAsync();

    Task<Comprobante?> ObtenerPorIdConDetalleAsync(Guid id);

    Task<IEnumerable<Comprobante>> ObtenerPorPacienteAsync(Guid pacienteId);

    Task<IEnumerable<Comprobante>> ObtenerPorPagoAsync(Guid pagoId);

    Task<IEnumerable<Comprobante>> ObtenerPorAtencionAsync(Guid atencionId);

    Task<Comprobante?> ObtenerPorSerieNumeroAsync(string serie, int numero);

    Task<int> ObtenerUltimoNumeroPorSerieAsync(string serie);
}