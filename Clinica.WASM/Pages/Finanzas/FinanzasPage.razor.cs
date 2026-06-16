using Clinica.WASM.Components.Finanzas;
using Clinica.WASM.Constants;
using Clinica.WASM.DTOs.Finanzas;
using Clinica.WASM.DTOs.Pacientes;
using Clinica.WASM.Services.Api;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Clinica.WASM.Pages.Finanzas;

public partial class FinanzasPage : ComponentBase
{
    [Inject] private FinanzasApiService FinanzasApiService { get; set; } = default!;
    [Inject] private PacienteApiService PacienteApiService { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    private int Anio = DateTime.Today.Year;
    private int Mes = DateTime.Today.Month;

    private ResumenFinancieroMensualCompletoDto? ResumenMensual;
    private List<EstadoPagoAtencionDto>? DeudasReales;
    private EstadoCuentaPacienteDto? EstadoCuenta;
    private List<AjusteFinancieroDto>? Ajustes;

    private List<PacienteItemDto> Pacientes { get; set; } = new();
    private Guid PacienteSeleccionadoId;
    private bool CargandoPacientes = true;

    protected override async Task OnInitializedAsync()
    {
        await CargarPacientesAsync();
    }

    private async Task CargarPacientesAsync()
    {
        CargandoPacientes = true;
        try
        {
            var pacientes = await PacienteApiService.ObtenerTodosAsync();
            Pacientes = pacientes.Select(p => new PacienteItemDto { Id = p.Id, NombreCompleto = p.NombreCompleto }).ToList();
        }
        catch { }
        finally { CargandoPacientes = false; }
    }

    private async Task CargarResumenMensualAsync()
    {
        try
        {
            ResumenMensual = await FinanzasApiService.ObtenerResumenMensualCompletoAsync(Anio, Mes);
        }
        catch { Snackbar.Add("Error al cargar resumen mensual.", Severity.Error); }
    }

    private async Task CargarDeudasRealesAsync()
    {
        try
        {
            DeudasReales = await FinanzasApiService.ObtenerDeudasRealesAsync();
        }
        catch { Snackbar.Add("Error al cargar deudas reales.", Severity.Error); }
    }

    private async Task CargarEstadoCuentaAsync()
    {
        try
        {
            EstadoCuenta = await FinanzasApiService.ObtenerEstadoCuentaPacienteAsync(PacienteSeleccionadoId);
        }
        catch { Snackbar.Add("Error al cargar estado de cuenta.", Severity.Error); }
    }

    private async Task CargarAjustesAsync()
    {
        try
        {
            Ajustes = await FinanzasApiService.ObtenerAjustesFinancierosAsync();
        }
        catch { Snackbar.Add("Error al cargar ajustes.", Severity.Error); }
    }
}