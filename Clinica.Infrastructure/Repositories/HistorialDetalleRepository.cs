using Clinica.Domain.Entities;
using Clinica.Domain.Interfaces;
using Clinica.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infrastructure.Repositories;

public class HistorialDetalleRepository : GenericRepository<HistorialDetalle>, IHistorialDetalleRepository
{
    public HistorialDetalleRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<HistorialDetalle>> ObtenerPorHistorialAsync(Guid historialClinicoId)
    {
        return await Context.HistorialDetalles
            .Include(x => x.Usuario)
            .Include(x => x.Cita)
            .Include(x => x.Atencion)
            .Include(x => x.Pago)
            .Where(x => x.HistorialClinicoId == historialClinicoId)
            .OrderByDescending(x => x.FechaRegistro)
            .ToListAsync();
    }
}