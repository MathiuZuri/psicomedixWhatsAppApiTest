using Clinica.Domain.Entities;
using Clinica.Domain.Interfaces;
using Clinica.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infrastructure.Repositories;

public class PermisoRepository : GenericRepository<Permiso>, IPermisoRepository
{
    public PermisoRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Permiso?> ObtenerPorCodigoAsync(string codigo)
    {
        return await Context.Permisos
            .FirstOrDefaultAsync(x => x.Codigo == codigo);
    }
}