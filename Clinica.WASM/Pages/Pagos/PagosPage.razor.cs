using Clinica.WASM.Components.Pagos;
using Clinica.WASM.Constants;
using Clinica.WASM.DTOs.Pagos;
using Clinica.WASM.DTOs.Pacientes;
using Clinica.WASM.DTOs.ServiciosClinicos;
using Clinica.WASM.Services.Api;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Clinica.WASM.DTOs.Finanzas;

namespace Clinica.WASM.Pages.Pagos;

public partial class PagosPage : ComponentBase
{
    [Inject] private PagoApiService PagoApiService { get; set; } = default!;
    [Inject] private PacienteApiService PacienteApiService { get; set; } = default!;
    [Inject] private ServicioClinicoApiService ServicioClinicoApiService { get; set; } = default!;
    [Inject] private FinanzasApiService FinanzasApiService { get; set; } = default!; // NUEVO
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;

    private List<PacienteItemDto> Pacientes { get; set; } = new();
    private List<ServicioClinicoResponseDto> Servicios { get; set; } = new();

    private Guid PacienteSeleccionadoId { get; set; }
    private bool EstaCargandoPacientes = true;

    private List<PagoResponseDto> Pagos { get; set; } = new();
    private bool EstaCargando;
    private bool HayErrorCarga;

    private string MensajeError = string.Empty;
    private string MensajeExito = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await CargarCatalogosAsync();
    }
    
    private async Task CambiarEstadoPago(PagoResponseDto pago)
    {
        var parameters = new DialogParameters { ["Pago"] = pago };
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true };
        var dialog = await DialogService.ShowAsync<CambiarEstadoPagoDialog>("Cambiar estado del pago", parameters, options);
        var result = await dialog.Result;

        if (!result.Canceled && result.Data is CambiarEstadoPagoDto dto)
        {
            var resultado = await PagoApiService.CambiarEstadoAsync(pago.Id, dto);
            if (resultado.Exitoso)
            {
                Snackbar.Add("Estado del pago actualizado correctamente.", Severity.Success);
                await CargarPagosAsync();
            }
            else
            {
                Snackbar.Add(resultado.Mensaje, Severity.Error);
            }
        }
    }

    private async Task CargarCatalogosAsync()
    {
        EstaCargandoPacientes = true;
        try
        {
            var tareas = new List<Task>
            {
                Task.Run(async () =>
                {
                    var pacientes = await PacienteApiService.ObtenerTodosAsync();
                    Pacientes = pacientes.Select(p => new PacienteItemDto { Id = p.Id, NombreCompleto = p.NombreCompleto }).ToList();
                }),
                Task.Run(async () =>
                {
                    Servicios = await ServicioClinicoApiService.ObtenerTodosAsync();
                })
            };
            await Task.WhenAll(tareas);
        }
        catch
        {
            MensajeError = "No se pudieron cargar los catálogos.";
        }
        finally
        {
            EstaCargandoPacientes = false;
        }
    }

    private async Task CargarPagosAsync()
    {
        if (PacienteSeleccionadoId == Guid.Empty) return;

        EstaCargando = true;
        HayErrorCarga = false;
        MensajeError = string.Empty;

        try
        {
            Pagos = await PagoApiService.ObtenerPorPacienteAsync(PacienteSeleccionadoId);
        }
        catch
        {
            HayErrorCarga = true;
            MensajeError = "Error al cargar los pagos del paciente.";
        }
        finally
        {
            EstaCargando = false;
        }
    }

    private async Task AbrirDialogoRegistro()
    {
        var parameters = new DialogParameters
        {
            ["Pacientes"] = Pacientes,
            ["Servicios"] = Servicios
        };
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Medium, FullWidth = true };
        var dialog = await DialogService.ShowAsync<RegistrarPagoDialog>("Registrar pago", parameters, options);
        var result = await dialog.Result;
        if (!result.Canceled && result.Data is RegistrarPagoDto dto)
        {
            var exito = await PagoApiService.RegistrarAsync(dto);
            if (exito.Exitoso)
            {
                Snackbar.Add("Pago registrado.", Severity.Success);
                PacienteSeleccionadoId = dto.PacienteId;
                await CargarPagosAsync();
            }
            else
            {
                Snackbar.Add(exito.Mensaje, Severity.Error);
            }
        }
    }

    // NUEVO MÉTODO
    private async Task AbrirAjustesDelPago(PagoResponseDto pago)
    {
        try
        {
            var ajustes = await FinanzasApiService.ObtenerAjustesPorPagoAsync(pago.Id);
            var parameters = new DialogParameters
            {
                ["Ajustes"] = ajustes,
                ["PagoId"] = pago.Id
            };
            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Medium, FullWidth = true };
            var dialog = await DialogService.ShowAsync<AjustesPagoDialog>("Ajustes del Pago " + pago.CodigoPago, parameters, options);
            var result = await dialog.Result;
            if (!result.Canceled && result.Data is RegistrarAjusteFinancieroDto nuevoAjuste)
            {
                var exito = await FinanzasApiService.RegistrarAjusteFinancieroAsync(nuevoAjuste);
                if (exito.Exitoso)
                {
                    Snackbar.Add("Ajuste registrado correctamente.", Severity.Success);
                    await CargarPagosAsync();
                }
                else
                {
                    Snackbar.Add(exito.Mensaje, Severity.Error);
                }
            }
        }
        catch
        {
            Snackbar.Add("Error al obtener los ajustes del pago.", Severity.Error);
        }
    }
}