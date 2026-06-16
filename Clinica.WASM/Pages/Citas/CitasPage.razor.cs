using Clinica.WASM.Components.Citas;
using Clinica.WASM.Constants;
using Clinica.WASM.DTOs.Citas;
using Clinica.WASM.DTOs.Doctores;
using Clinica.WASM.DTOs.Pacientes;
using Clinica.WASM.DTOs.ServiciosClinicos;
using Clinica.WASM.Services.Api;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Clinica.WASM.Pages.Citas;

public partial class CitasPage : ComponentBase
{
    [Inject] private CitaApiService CitaApiService { get; set; } = default!;
    [Inject] private DoctorApiService DoctorApiService { get; set; } = default!;
    [Inject] private ServicioClinicoApiService ServicioClinicoApiService { get; set; } = default!;
    [Inject] private PacienteApiService PacienteApiService { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;

    private List<CitaResponseDto> Citas { get; set; } = new();
    private List<DoctorResponseDto> DoctoresList { get; set; } = new();
    private List<ServicioClinicoResponseDto> ServiciosList { get; set; } = new();
    private List<PacienteItemDto> PacientesList { get; set; } = new();


    private CrearCitaDto NuevaCita { get; set; } = new();

    private bool EstaCargando = true;
    private bool EstaProcesando;
    private bool MostrandoFormulario;
    private bool HayErrorCarga;

    private string SearchText = string.Empty;
    private string MensajeError = string.Empty;
    private string MensajeExito = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await CargarDatosInicialesAsync();
    }

    private async Task CargarDatosInicialesAsync()
    {
        EstaCargando = true;
        HayErrorCarga = false;
        try
        {
            await Task.WhenAll(
                CargarCitasAsync(),
                CargarCatalogosAsync()
            );
        }
        catch
        {
            HayErrorCarga = true;
            MensajeError = "Error al cargar datos.";
        }
        finally
        {
            EstaCargando = false;
        }
    }

    private async Task CargarCitasAsync()
    {
        Citas = await CitaApiService.ObtenerTodasAsync();
    }

    private async Task CargarCatalogosAsync()
    {
        var tareas = new List<Task>
        {
            Task.Run(async () => DoctoresList = await DoctorApiService.ObtenerTodosAsync()),
            Task.Run(async () => ServiciosList = await ServicioClinicoApiService.ObtenerTodosAsync()),
            Task.Run(async () =>
            {
                var pacientes = await PacienteApiService.ObtenerTodosAsync();
                PacientesList = pacientes.Select(p => new PacienteItemDto { Id = p.Id, NombreCompleto = p.NombreCompleto }).ToList();
            })
        };
        await Task.WhenAll(tareas);
    }

    private void AbrirFormularioNuevo()
    {
        NuevaCita = new CrearCitaDto();
        MostrandoFormulario = true;
        MensajeError = string.Empty;
        MensajeExito = string.Empty;
    }

    private void CerrarFormulario()
    {
        MostrandoFormulario = false;
    }

    private async Task GuardarCitaAsync()
    {
        EstaProcesando = true;
        MensajeError = string.Empty;
        try
        {
            var resultado = await CitaApiService.CrearAsync(NuevaCita);
            if (resultado.Exitoso)
            {
                MensajeExito = resultado.Mensaje;
                Snackbar.Add("Cita programada.", Severity.Success);
                MostrandoFormulario = false;
                await CargarCitasAsync();
            }
            else
            {
                MensajeError = resultado.Mensaje;
                Snackbar.Add(resultado.Mensaje, Severity.Error);
            }
        }
        catch
        {
            MensajeError = "Error de conexión.";
        }
        finally
        {
            EstaProcesando = false;
        }
    }

    private async Task VerCita(CitaResponseDto cita)
    {
        var parameters = new DialogParameters { ["Cita"] = cita };
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Medium, FullWidth = true };
        await DialogService.ShowAsync<CitaDetalleDialog>("Detalle de cita", parameters, options);
    }

    private async Task AbrirReprogramacion(CitaResponseDto cita)
    {
        var parameters = new DialogParameters
        {
            ["Cita"] = cita,
            ["DoctoresList"] = DoctoresList
        };
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true };
        var dialog = await DialogService.ShowAsync<CitaReprogramarDialog>("Reprogramar cita", parameters, options);
        var result = await dialog.Result;
        if (!result.Canceled && result.Data is ReprogramarCitaDto dto)
        {
            var exito = await CitaApiService.ReprogramarAsync(cita.Id, dto);
            if (exito.Exitoso)
            {
                Snackbar.Add("Cita reprogramada.", Severity.Success);
                await CargarCitasAsync();
            }
            else
            {
                Snackbar.Add(exito.Mensaje, Severity.Error);
            }
        }
    }

    private async Task AbrirCancelacion(CitaResponseDto cita)
    {
        var parameters = new DialogParameters { ["Cita"] = cita };
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true };
        var dialog = await DialogService.ShowAsync<CitaCancelarDialog>("Cancelar cita", parameters, options);
        var result = await dialog.Result;
        if (!result.Canceled && result.Data is CancelarCitaDto dto)
        {
            var exito = await CitaApiService.CancelarAsync(cita.Id, dto);
            if (exito.Exitoso)
            {
                Snackbar.Add("Cita cancelada.", Severity.Success);
                await CargarCitasAsync();
            }
            else
            {
                Snackbar.Add(exito.Mensaje, Severity.Error);
            }
        }
    }
}