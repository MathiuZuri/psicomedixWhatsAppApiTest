using Clinica.Domain.Entities;
using Clinica.Domain.Enums;
using Clinica.Domain.Interfaces;
using Clinica.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infrastructure.Repositories;

public class ServicioClinicoRepository : GenericRepository<ServicioClinico>, IServicioClinicoRepository
{
    public ServicioClinicoRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<ServicioClinico?> ObtenerPorCodigoAsync(string codigoServicio)
    {
        return await Context.ServiciosClinicos
            .FirstOrDefaultAsync(x => x.CodigoServicio == codigoServicio);
    }

    public async Task<IEnumerable<ServicioClinico>> ObtenerActivosAsync()
    {
        return await Context.ServiciosClinicos
            .Where(x => x.Estado == EstadoServicioClinico.Activo)
            .OrderBy(x => x.Nombre)
            .ToListAsync();
    }
}