using Clinica.WASM.Components.ServiciosClinicos;
using Clinica.WASM.Constants;
using Clinica.WASM.DTOs.ServiciosClinicos;
using Clinica.WASM.Services.Api;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Clinica.WASM.Pages.ServiciosClinicos;

public partial class ServiciosClinicosPage : ComponentBase
{
    [Inject] private ServicioClinicoApiService ServicioClinicoApiService { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;

    private List<ServicioClinicoResponseDto> Servicios { get; set; } = new();

    private bool EstaCargando = true;
    private bool HayErrorCarga;

    private string SearchText = string.Empty;
    private string MensajeError = string.Empty;
    private string MensajeExito = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await CargarServiciosAsync();
    }

    private async Task CargarServiciosAsync()
    {
        EstaCargando = true;
        HayErrorCarga = false;
        MensajeError = string.Empty;

        try
        {
            Servicios = await ServicioClinicoApiService.ObtenerTodosAsync();
        }
        catch
        {
            HayErrorCarga = true;
            MensajeError = "No se pudo cargar la lista de servicios clínicos.";
        }
        finally
        {
            EstaCargando = false;
        }
    }

    private async Task VerServicio(ServicioClinicoResponseDto servicio)
    {
        var parameters = new DialogParameters { ["Servicio"] = servicio };
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Medium, FullWidth = true };
        await DialogService.ShowAsync<ServicioClinicoDetalleDialog>("Detalle del servicio", parameters, options);
    }
}