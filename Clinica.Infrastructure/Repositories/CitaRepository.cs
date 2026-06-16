using Clinica.Domain.Entities;
using Clinica.Domain.Interfaces;
using Clinica.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infrastructure.Repositories;

public class CitaRepository : GenericRepository<Cita>, ICitaRepository
{
    public CitaRepository(ApplicationDbContext context) : base(context)
    {
    }
    
    public async Task<IEnumerable<Cita>> ObtenerTodasConRelacionesAsync()
    {
        return await Context.Citas
            .Include(x => x.Paciente)
            .Include(x => x.Doctor)
            .Include(x => x.ServicioClinico)
            .OrderByDescending(x => x.Fecha)
            .ThenByDescending(x => x.HoraInicio)
            .ToListAsync();
    }
    
    public async Task<Cita?> ObtenerPorIdConRelacionesAsync(Guid id)
    {
        return await Context.Citas
            .Include(x => x.Paciente)
            .Include(x => x.Doctor)
            .Include(x => x.ServicioClinico)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<bool> ExisteInterferenciaHorarioAsync(
        Guid doctorId,
        DateOnly fecha,
        TimeOnly horaInicio,
        TimeOnly horaFin,
        Guid? citaIdExcluir = null)
    {
        var query = Context.Citas
            .Where(x =>
                x.DoctorId == doctorId &&
                x.Fecha == fecha &&
                x.Estado != Clinica.Domain.Enums.EstadoCita.Cancelada &&
                x.Estado != Clinica.Domain.Enums.EstadoCita.Eliminada);

        if (citaIdExcluir.HasValue)
        {
            query = query.Where(x => x.Id != citaIdExcluir.Value);
        }

        return await query.AnyAsync(x =>
            horaInicio < x.HoraFin &&
            horaFin > x.HoraInicio);
    }

    public async Task<IEnumerable<Cita>> ObtenerPorPacienteAsync(Guid pacienteId)
    {
        return await Context.Citas
            .Include(x => x.Paciente)
            .Include(x => x.Doctor)
            .Include(x => x.ServicioClinico)
            .Where(x => x.PacienteId == pacienteId)
            .OrderByDescending(x => x.Fecha)
            .ThenByDescending(x => x.HoraInicio)
            .ToListAsync();
    }

    public async Task<IEnumerable<Cita>> ObtenerPorDoctorAsync(Guid doctorId)
    {
        return await Context.Citas
            .Include(x => x.Paciente)
            .Include(x => x.Doctor)
            .Include(x => x.ServicioClinico)
            .Where(x => x.DoctorId == doctorId)
            .OrderByDescending(x => x.Fecha)
            .ThenByDescending(x => x.HoraInicio)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Cita>> ObtenerCitasParaRecordatorioAsync(
        DateTime desdeUtc,
        DateTime hastaUtc)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Now);
        var manana = hoy.AddDays(1);

        return await Context.Citas
            .Include(x => x.Paciente)
            .Include(x => x.Doctor)
            .Include(x => x.ServicioClinico)
            .Where(x =>
                x.Fecha >= hoy &&
                x.Fecha <= manana &&
                x.Paciente.Celular != null &&
                x.Paciente.Celular != "" &&
                (
                    x.Estado == Clinica.Domain.Enums.EstadoCita.Pendiente ||
                    x.Estado == Clinica.Domain.Enums.EstadoCita.Confirmada ||
                    x.Estado == Clinica.Domain.Enums.EstadoCita.Reprogramada
                ))
            .OrderBy(x => x.Fecha)
            .ThenBy(x => x.HoraInicio)
            .ToListAsync();
    }
}