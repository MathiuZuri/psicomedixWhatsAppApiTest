using Clinica.Domain.Entities;

namespace Clinica.Domain.Interfaces;

public interface IAtencionRepository : IGenericRepository<Atencion>
{
    Task<IEnumerable<Atencion>> ObtenerPorPacienteAsync(Guid pacienteId);
    Task<Atencion?> ObtenerPorCitaAsync(Guid citaId);
}