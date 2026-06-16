using Clinica.API.Configurations;
using Clinica.Domain.Entities;
using Clinica.Domain.Enums;
using Clinica.Domain.Interfaces;
using Microsoft.Extensions.Options;
using System.Linq;

namespace Clinica.API.Services.Background;
// esto es exclusivo de evolution api, no incluir al sistema
public class RecordatorioCitasBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly WhatsAppOptions _options;
    private readonly ILogger<RecordatorioCitasBackgroundService> _logger;

    public RecordatorioCitasBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<WhatsAppOptions> options,
        ILogger<RecordatorioCitasBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Servicio de recordatorios WhatsApp deshabilitado.");
            return;
        }

        _logger.LogInformation(
            "RecordatorioCitasBackgroundService iniciado. Enabled: {Enabled}, ReminderHoursBefore: {ReminderHoursBefore}, CheckIntervalMinutes: {CheckIntervalMinutes}",
            _options.Enabled,
            _options.ReminderHoursBefore,
            _options.CheckIntervalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcesarRecordatoriosAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando recordatorios de citas.");
            }

            await Task.Delay(
                TimeSpan.FromMinutes(_options.CheckIntervalMinutes),
                stoppingToken);
        }
    }

    private async Task ProcesarRecordatoriosAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var citaRepository = scope.ServiceProvider.GetRequiredService<ICitaRepository>();
        var notificacionRepository = scope.ServiceProvider.GetRequiredService<INotificacionCitaRepository>();
        var whatsAppService = scope.ServiceProvider.GetRequiredService<INotificacionWhatsAppService>();

        var ahoraUtc = DateTime.UtcNow;

        var desde = ahoraUtc.AddHours(_options.ReminderHoursBefore - 1);
        var hasta = ahoraUtc.AddHours(_options.ReminderHoursBefore + 1);

        var citas = await citaRepository.ObtenerCitasParaRecordatorioAsync(desde, hasta);
        _logger.LogInformation(
            "Citas encontradas para recordatorio: {Cantidad}",
            citas.Count());

        foreach (var cita in citas)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            var yaExiste = await notificacionRepository.ExisteRecordatorioParaCitaAsync(cita.Id);

            if (yaExiste)
                continue;

            var fechaHoraCita = cita.Fecha.ToDateTime(cita.HoraInicio);
            var fechaProgramada = fechaHoraCita.AddHours(-_options.ReminderHoursBefore);

            var mensaje = ConstruirMensaje(cita);

            var notificacion = new NotificacionCita
            {
                CitaId = cita.Id,
                PacienteId = cita.PacienteId,
                TelefonoDestino = cita.Paciente.Celular!,
                Canal = CanalNotificacion.WhatsApp,
                Mensaje = mensaje,
                FechaProgramadaEnvio = fechaProgramada.ToUniversalTime(),
                Estado = EstadoNotificacion.Pendiente,
                Intentos = 0,
                FechaCreacion = DateTime.UtcNow
            };

            await notificacionRepository.AddAsync(notificacion);
        }

        var pendientes = await notificacionRepository.ObtenerPendientesAsync(
            ahoraUtc,
            _options.MaxIntentos);

        foreach (var notificacion in pendientes)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            try
            {
                notificacion.Intentos++;
                notificacion.FechaActualizacion = DateTime.UtcNow;

                await whatsAppService.EnviarMensajeAsync(
                    notificacion.TelefonoDestino,
                    notificacion.Mensaje,
                    cancellationToken);

                notificacion.Estado = EstadoNotificacion.Enviado;
                notificacion.FechaEnvio = DateTime.UtcNow;
                notificacion.Error = null;
            }
            catch (Exception ex)
            {
                notificacion.Error = ex.Message;
                notificacion.FechaActualizacion = DateTime.UtcNow;

                if (notificacion.Intentos >= _options.MaxIntentos)
                    notificacion.Estado = EstadoNotificacion.Fallido;

                _logger.LogWarning(
                    ex,
                    "No se pudo enviar recordatorio de cita. NotificacionId: {NotificacionId}",
                    notificacion.Id);
            }

            await notificacionRepository.ActualizarAsync(notificacion);
        }
    }

    private static string ConstruirMensaje(Cita cita)
    {
        var paciente = $"{cita.Paciente.Nombres} {cita.Paciente.Apellidos}".Trim().ToUpper();
        var doctor = $"{cita.Doctor.Nombres} {cita.Doctor.Apellidos}".Trim().ToUpper();

        var fechaHora = cita.Fecha.ToDateTime(cita.HoraInicio);

        var cultura = new System.Globalization.CultureInfo("es-PE");

        var fechaFormateada = fechaHora.ToString("dddd dd/MM/yyyy", cultura);
        var horaFormateada = cita.HoraInicio.ToString("HH:mm");

        return
            "Recordatorio de cita\n\n" +
            "CLÍNICA SANTA MÓNICA🏥\n" +
            $"Estimado(a) {paciente}, tiene agendada una cita:\n\n" +
            $"📆 Fecha: {fechaFormateada}\n\n" +
            $"⏰ Hora: {horaFormateada}\n\n" +
            $"🏥 Doctor: {doctor}\n\n" +
            "Estaremos pendientes a su confirmación.\n" +
            "¡Esperamos su asistencia!\n\n" +
            "Horario de atención: De lunes a viernes, turno mañana 8:30 a. m. a 1:00 p. m. - turno tarde 3:00 p. m. a 8:00 p. m.";
    }
}