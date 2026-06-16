namespace Clinica.Domain.DTOs.Horarios;

public class EditarHorarioDoctorDto
{
    public DayOfWeek DiaSemana { get; set; }

    public TimeOnly HoraInicio { get; set; }

    public TimeOnly HoraFin { get; set; }

    public DateOnly FechaInicioVigencia { get; set; }

    public DateOnly? FechaFinVigencia { get; set; }

    public bool Activo { get; set; } = true;
}