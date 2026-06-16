using Clinica.Domain.Entities;

namespace Clinica.Domain.Interfaces;

public interface IServicioClinicoRepository : IGenericRepository<ServicioClinico>
{
    Task<ServicioClinico?> ObtenerPorCodigoAsync(string codigoServicio);
    Task<IEnumerable<ServicioClinico>> ObtenerActivosAsync();
}