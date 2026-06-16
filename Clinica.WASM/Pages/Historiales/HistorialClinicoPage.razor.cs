using Clinica.WASM.Components.Historiales;

using Clinica.WASM.DTOs.Historiales;
using Clinica.WASM.DTOs.Pacientes;
using Clinica.WASM.Services.Api;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Clinica.WASM.Pages.Historiales;

public partial class HistorialClinicoPage : ComponentBase
{
    [Inject] private PacienteApiService PacienteApiService { get; set; } = default!;
    [Inject] private HistorialClinicoApiService HistorialClinicoApiService { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;

    private List<PacienteItemDto> Pacientes { get; set; } = new();
    private Guid PacienteSeleccionadoId { get; set; }
    private bool EstaCargandoPacientes = true;

    private HistorialClinicoResponseDto? HistorialActual { get; set; }
    private bool HistorialNoEncontrado { get; set; }
    private string MensajeError = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await CargarPacientesAsync();
    }

    private async Task CargarPacientesAsync()
    {
        EstaCargandoPacientes = true;
        try
        {
            var pacientes = await PacienteApiService.ObtenerTodosAsync();
            Pacientes = pacientes.Select(p => new PacienteItemDto { Id = p.Id, NombreCompleto = p.NombreCompleto }).ToList();
        }
        catch
        {
            MensajeError = "No se pudo cargar la lista de pacientes.";
        }
        finally
        {
            EstaCargandoPacientes = false;
        }
    }

    private async Task CargarHistorialAsync()
    {
        if (PacienteSeleccionadoId == Guid.Empty) return;

        MensajeError = string.Empty;
        HistorialActual = null;
        HistorialNoEncontrado = false;

        try
        {
            var historial = await HistorialClinicoApiService.ObtenerPorPacienteAsync(PacienteSeleccionadoId);
            if (historial == null)
            {
                HistorialNoEncontrado = true;
                return;
            }
            HistorialActual = historial;
        }
        catch
        {
            MensajeError = "Error al cargar el historial clínico.";
        }
    }

    private async Task VerDetalle(HistorialDetalleResponseDto detalle)
    {
        var parameters = new DialogParameters { ["Detalle"] = detalle };
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Medium, FullWidth = true };
        await DialogService.ShowAsync<HistorialDetalleDialog>("Detalle del movimiento", parameters, options);
    }

    private async Task<IEnumerable<Guid>> BuscarPaciente(string search, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(search))
            return Pacientes.Select(p => p.Id);
        
        return Pacientes
            .Where(p => p.NombreCompleto.Contains(search, StringComparison.OrdinalIgnoreCase))
            .Select(p => p.Id);
    }

    private static string ObtenerNombreTipo(TipoMovimientoHistorial tipo) =>
        tipo switch
        {
            TipoMovimientoHistorial.RegistroUsuario => "Registro de usuario",
            TipoMovimientoHistorial.AperturaHistorial => "Apertura de historial",
            TipoMovimientoHistorial.CitaProgramada => "Cita programada",
            TipoMovimientoHistorial.CitaReprogramada => "Cita reprogramada",
            TipoMovimientoHistorial.CitaCancelada => "Cita cancelada",
            TipoMovimientoHistorial.CitaAtendida => "Cita atendida",
            TipoMovimientoHistorial.AtencionRegistrada => "Atención registrada",
            TipoMovimientoHistorial.AtencionCerrada => "Atención cerrada",
            TipoMovimientoHistorial.PagoRegistrado => "Pago registrado",
            TipoMovimientoHistorial.PagoParcial => "Pago parcial",
            TipoMovimientoHistorial.PagoCompletado => "Pago completado",
            TipoMovimientoHistorial.SeguimientoRegistrado => "Seguimiento registrado",
            TipoMovimientoHistorial.ObservacionClinica => "Observación clínica",
            TipoMovimientoHistorial.ActualizacionDatosPaciente => "Actualización de datos",
            TipoMovimientoHistorial.EliminacionLogica => "Eliminación lógica",
            _ => tipo.ToString()
        };
}