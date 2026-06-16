using Clinica.Domain.Entities;

namespace Clinica.Domain.Interfaces;

public interface IHistorialClinicoRepository : IGenericRepository<HistorialClinico>
{
    Task<HistorialClinico?> ObtenerPorPacienteAsync(Guid pacienteId);
    Task<HistorialClinico?> ObtenerConDetallesAsync(Guid historialId);
}