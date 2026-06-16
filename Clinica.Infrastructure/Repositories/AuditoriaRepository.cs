using Clinica.Domain.Entities;
using Clinica.Domain.Interfaces;
using Clinica.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infrastructure.Repositories;

public class AuditoriaRepository : GenericRepository<Auditoria>, IAuditoriaRepository
{
    public AuditoriaRepository(ApplicationDbContext context) : base(context)
    {
    }

    public new async Task<IEnumerable<Auditoria>> GetAllAsync()
    {
        return await Context.Auditorias
            .Include(x => x.Usuario)
            .OrderByDescending(x => x.FechaHora)
            .ToListAsync();
    }

    public async Task<IEnumerable<Auditoria>> ObtenerPorUsuarioAsync(Guid usuarioId)
    {
        return await Context.Auditorias
            .Include(x => x.Usuario)
            .Where(x => x.UsuarioId == usuarioId)
            .OrderByDescending(x => x.FechaHora)
            .ToListAsync();
    }
}