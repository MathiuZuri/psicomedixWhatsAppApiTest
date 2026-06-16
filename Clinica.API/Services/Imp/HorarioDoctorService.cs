using Clinica.Domain.DTOs.Horarios;
using Clinica.Domain.Entities;
using Clinica.Domain.Interfaces;

namespace Clinica.API.Services.Imp;

public class HorarioDoctorService : IHorarioDoctorService
{
    private readonly IHorarioDoctorRepository _horarioRepository;
    private readonly IDoctorRepository _doctorRepository;
    private readonly ICitaRepository _citaRepository;

    public HorarioDoctorService(
        IHorarioDoctorRepository horarioRepository,
        IDoctorRepository doctorRepository,
        ICitaRepository citaRepository)
    {
        _horarioRepository = horarioRepository;
        _doctorRepository = doctorRepository;
        _citaRepository = citaRepository;
    }
    
public async Task<MatrizSemanalDto> ObtenerMatrizSemanalAsync(Guid doctorId, DateOnly fechaReferencia)
    {
        var doctor = await _doctorRepository.GetByIdAsync(doctorId)
            ?? throw new KeyNotFoundException("Doctor no encontrado.");

        int diasDiferencia = (int)fechaReferencia.DayOfWeek - (int)DayOfWeek.Monday;
        if (diasDiferencia < 0) diasDiferencia += 7;
        
        var fechaLunes = fechaReferencia.AddDays(-diasDiferencia);
        var fechaDomingo = fechaLunes.AddDays(6);

        var turnosPlantilla = await _horarioRepository.ObtenerPorDoctorAsync(doctorId);
        
        // CORREGIDO: Ahora llamamos de forma correcta al repositorio de citas correspondientes
        var citasSemanales = await _citaRepository.ObtenerCitasParaRecordatorioAsync(
            fechaLunes.ToDateTime(TimeOnly.MinValue), 
            fechaDomingo.ToDateTime(TimeOnly.MaxValue)
        );

        var citasDoctor = citasSemanales.Where(x => x.DoctorId == doctorId && 
            x.Estado != Domain.Enums.EstadoCita.Cancelada && 
            x.Estado != Domain.Enums.EstadoCita.Eliminada).ToList();

        var matriz = new MatrizSemanalDto
        {
            DoctorId = doctorId,
            DoctorNombre = $"{doctor.Nombres} {doctor.Apellidos}",
            Especialidad = doctor.Especialidad,
            FechaInicioSemana = fechaLunes,
            FechaFinSemana = fechaDomingo
        };

        var horaApertura = new TimeOnly(8, 0);
        var horaCierre = new TimeOnly(18, 0);
        var intervaloBase = TimeSpan.FromMinutes(30);

        var horaActual = horaApertura;

        while (horaActual < horaCierre)
        {
            var horaSiguiente = horaActual.Add(intervaloBase);
            var fila = new FilaMatrizDto
            {
                HoraInicio = horaActual,
                RangoHora = $"{horaActual.ToString("HH:mm")} - {horaSiguiente.ToString("HH:mm")}"
            };

            for (int i = 0; i < 7; i++)
            {
                var diaAnalizar = (DayOfWeek)(((int)DayOfWeek.Monday + i) % 7);
                var fechaCelda = fechaLunes.AddDays(i);

                var celda = new CeldaMatrizDto
                {
                    DiaSemana = diaAnalizar,
                    FechaCelda = fechaCelda,
                    Estado = "FueraHorario"
                };

                var plantillaDia = turnosPlantilla.FirstOrDefault(x => x.DiaSemana == diaAnalizar && x.Activo &&
                    fechaCelda >= x.FechaInicioVigencia && (!x.FechaFinVigencia.HasValue || fechaCelda <= x.FechaFinVigencia.Value));

                if (plantillaDia != null && horaActual >= plantillaDia.HoraInicio && horaSiguiente <= plantillaDia.HoraFin)
                {
                    celda.Estado = "Disponible";
                }

                var citaBloque = citasDoctor.FirstOrDefault(x => x.Fecha == fechaCelda &&
                    ((horaActual >= x.HoraInicio && horaActual < x.HoraFin) || (x.HoraInicio >= horaActual && x.HoraInicio < horaSiguiente)));

                if (citaBloque != null)
                {
                    celda.Estado = "Ocupado";
                    celda.CitaId = citaBloque.Id;
                    celda.CodigoCita = citaBloque.CodigoCita;
                    celda.PacienteNombre = citaBloque.Paciente != null ? $"{citaBloque.Paciente.Nombres} {citaBloque.Paciente.Apellidos}" : "Paciente Anónimo";
                    celda.ServicioNombre = citaBloque.ServicioClinico?.Nombre ?? "Consulta General";
                }

                fila.CeldasDias.Add(celda);
            }

            matriz.Filas.Add(fila);
            horaActual = horaSiguiente;
        }

        return matriz;
    }
    
    public async Task<IEnumerable<HorarioDoctorResponseDto>> ObtenerTodosAsync()
    {
        var horarios = await _horarioRepository.ObtenerTodosConDoctorAsync();

        return horarios.Select(MapearHorario);
    }

    public async Task<IEnumerable<HorarioDoctorResponseDto>> ObtenerPorDoctorAsync(Guid doctorId)
    {
        var horarios = await _horarioRepository.ObtenerPorDoctorAsync(doctorId);

        return horarios.Select(MapearHorario);
    }

    public async Task<Guid> CrearAsync(CrearHorarioDoctorDto dto)
    {
        var doctor = await _doctorRepository.GetByIdAsync(dto.DoctorId);
        if (doctor == null)
            throw new KeyNotFoundException("Doctor no encontrado.");

        if (dto.HoraFin <= dto.HoraInicio)
            throw new InvalidOperationException("La hora de fin debe ser mayor que la hora de inicio.");
        
        if (dto.FechaFinVigencia.HasValue && dto.FechaFinVigencia.Value < dto.FechaInicioVigencia)
            throw new InvalidOperationException("La fecha de fin de vigencia no puede ser menor que la fecha de inicio.");

        var horario = new HorarioDoctor
        {
            Id = Guid.NewGuid(),
            DoctorId = dto.DoctorId,
            DiaSemana = dto.DiaSemana,
            HoraInicio = dto.HoraInicio,
            HoraFin = dto.HoraFin,
            FechaInicioVigencia = dto.FechaInicioVigencia,
            FechaFinVigencia = dto.FechaFinVigencia,
            Activo = true
        };

        await _horarioRepository.AddAsync(horario);
        await _horarioRepository.SaveChangesAsync();

        return horario.Id;
    }

    public async Task ActualizarAsync(Guid id, EditarHorarioDoctorDto dto)
    {
        var horario = await _horarioRepository.GetByIdAsync(id);
        if (horario == null)
            throw new KeyNotFoundException("Horario no encontrado.");

        if (dto.HoraFin <= dto.HoraInicio)
            throw new InvalidOperationException("La hora de fin debe ser mayor que la hora de inicio.");
        
        if (dto.FechaFinVigencia.HasValue && dto.FechaFinVigencia.Value < dto.FechaInicioVigencia)
            throw new InvalidOperationException("La fecha de fin de vigencia no puede ser menor que la fecha de inicio.");

        horario.DiaSemana = dto.DiaSemana;
        horario.HoraInicio = dto.HoraInicio;
        horario.HoraFin = dto.HoraFin;
        horario.FechaInicioVigencia = dto.FechaInicioVigencia;
        horario.FechaFinVigencia = dto.FechaFinVigencia;
        horario.Activo = dto.Activo;

        _horarioRepository.Update(horario);
        await _horarioRepository.SaveChangesAsync();
    }

    private static HorarioDoctorResponseDto MapearHorario(HorarioDoctor horario)
    {
        return new HorarioDoctorResponseDto
        {
            Id = horario.Id,
            DoctorId = horario.DoctorId,
            DoctorNombre = horario.Doctor == null ? string.Empty : $"{horario.Doctor.Nombres} {horario.Doctor.Apellidos}",
            DiaSemana = horario.DiaSemana,
            HoraInicio = horario.HoraInicio,
            HoraFin = horario.HoraFin,
            FechaInicioVigencia = horario.FechaInicioVigencia,
            FechaFinVigencia = horario.FechaFinVigencia,
            Activo = horario.Activo
        };
    }
}