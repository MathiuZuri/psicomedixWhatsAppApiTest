using Clinica.Domain.Entities;

namespace Clinica.Domain.Interfaces;

public interface IHistorialDetalleRepository : IGenericRepository<HistorialDetalle>
{
    Task<IEnumerable<HistorialDetalle>> ObtenerPorHistorialAsync(Guid historialClinicoId);
}