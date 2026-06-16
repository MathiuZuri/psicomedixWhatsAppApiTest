using Clinica.Domain.Entities;
using Clinica.Domain.Enums;

namespace Clinica.Domain.Interfaces;

public interface IAjusteFinancieroRepository : IGenericRepository<AjusteFinanciero>
{
    Task<IEnumerable<AjusteFinanciero>> ObtenerTodosConDetalleAsync();
    Task<IEnumerable<AjusteFinanciero>> ObtenerPorAtencionAsync(Guid atencionId);
    Task<IEnumerable<AjusteFinanciero>> ObtenerPorPagoAsync(Guid pagoId);

    Task<bool> ExisteAjusteSimilarAsync(
        Guid pagoId,
        TipoAjusteFinanciero tipoAjuste,
        decimal montoAjuste,
        string motivo);
}