using Clinica.Domain.Entities;

namespace Clinica.Domain.Interfaces;

public interface IAuditoriaRepository : IGenericRepository<Auditoria>
{
    Task<IEnumerable<Auditoria>> ObtenerPorUsuarioAsync(Guid usuarioId);
    new Task<IEnumerable<Auditoria>> GetAllAsync();
}