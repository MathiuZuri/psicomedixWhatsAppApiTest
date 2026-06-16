using Clinica.Domain.Entities;
using Clinica.Domain.Interfaces;
using Clinica.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infrastructure.Repositories;

public class ComprobanteRepository : GenericRepository<Comprobante>, IComprobanteRepository
{
    public ComprobanteRepository(ApplicationDbContext context) : base(context)
    {
    }

    // ==========================================================
    // LISTADO GENERAL CON DETALLE
    // ==========================================================

    public async Task<IEnumerable<Comprobante>> ObtenerTodosConDetalleAsync()
    {
        return await Context.Comprobantes
            .AsNoTracking()
            .Include(x => x.Paciente)
            .Include(x => x.Pago)
            .Include(x => x.Cita)
            .Include(x => x.Atencion)
            .Include(x => x.HistorialClinico)
            .Include(x => x.UsuarioEmision)
            .Include(x => x.UsuarioAnulacion)
            .Include(x => x.Detalles)
            .OrderByDescending(x => x.FechaEmision)
            .ToListAsync();
    }

    // ==========================================================
    // BUSCAR POR ID CON DETALLE
    // ==========================================================

    public async Task<Comprobante?> ObtenerPorIdConDetalleAsync(Guid id)
    {
        return await Context.Comprobantes
            .Include(x => x.Paciente)
            .Include(x => x.Pago)
            .Include(x => x.Cita)
            .ThenInclude(x => x!.Doctor)
            .Include(x => x.Cita)
            .ThenInclude(x => x!.ServicioClinico)
            .Include(x => x.Atencion)
            .ThenInclude(x => x!.Doctor)
            .Include(x => x.Atencion)
            .ThenInclude(x => x!.ServicioClinico)
            .Include(x => x.HistorialClinico)
            .Include(x => x.UsuarioEmision)
            .Include(x => x.UsuarioAnulacion)
            .Include(x => x.Detalles)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    // ==========================================================
    // CONSULTAS POR RELACIONES
    // ==========================================================

    public async Task<IEnumerable<Comprobante>> ObtenerPorPacienteAsync(Guid pacienteId)
    {
        return await Context.Comprobantes
            .AsNoTracking()
            .Include(x => x.Paciente)
            .Include(x => x.UsuarioEmision)
            .Include(x => x.UsuarioAnulacion)
            .Include(x => x.Detalles)
            .Where(x => x.PacienteId == pacienteId)
            .OrderByDescending(x => x.FechaEmision)
            .ToListAsync();
    }

    public async Task<IEnumerable<Comprobante>> ObtenerPorPagoAsync(Guid pagoId)
    {
        return await Context.Comprobantes
            .AsNoTracking()
            .Include(x => x.Paciente)
            .Include(x => x.Pago)
            .Include(x => x.UsuarioEmision)
            .Include(x => x.UsuarioAnulacion)
            .Include(x => x.Detalles)
            .Where(x => x.PagoId == pagoId)
            .OrderByDescending(x => x.FechaEmision)
            .ToListAsync();
    }

    public async Task<IEnumerable<Comprobante>> ObtenerPorAtencionAsync(Guid atencionId)
    {
        return await Context.Comprobantes
            .AsNoTracking()
            .Include(x => x.Paciente)
            .Include(x => x.Atencion)
            .Include(x => x.UsuarioEmision)
            .Include(x => x.UsuarioAnulacion)
            .Include(x => x.Detalles)
            .Where(x => x.AtencionId == atencionId)
            .OrderByDescending(x => x.FechaEmision)
            .ToListAsync();
    }

    // ==========================================================
    // SERIE Y NUMERACIÓN
    // ==========================================================

    public async Task<Comprobante?> ObtenerPorSerieNumeroAsync(string serie, int numero)
    {
        return await Context.Comprobantes
            .AsNoTracking()
            .Include(x => x.Paciente)
            .Include(x => x.Detalles)
            .FirstOrDefaultAsync(x => x.Serie == serie && x.Numero == numero);
    }

    public async Task<int> ObtenerUltimoNumeroPorSerieAsync(string serie)
    {
        return await Context.Comprobantes
            .Where(x => x.Serie == serie)
            .MaxAsync(x => (int?)x.Numero) ?? 0;
    }
}