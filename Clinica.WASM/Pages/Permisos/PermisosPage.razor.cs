using Clinica.WASM.Constants;
using Clinica.WASM.DTOs.Permisos;
using Clinica.WASM.Services.Api;
using Microsoft.AspNetCore.Components;

namespace Clinica.WASM.Pages.Permisos;

public partial class PermisosPage : ComponentBase
{
    [Inject] private PermisoApiService PermisoApiService { get; set; } = default!;

    private List<PermisoResponseDto> Permisos { get; set; } = new();
    private bool EstaCargando = true;
    private bool HayErrorCarga;

    private string SearchText = string.Empty;
    private string MensajeError = string.Empty;

    private IEnumerable<PermisoResponseDto> PermisosFiltrados =>
        string.IsNullOrWhiteSpace(SearchText)
            ? Permisos
            : Permisos.Where(x =>
                x.Codigo.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                x.Nombre.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                x.Modulo.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

    protected override async Task OnInitializedAsync()
    {
        await CargarPermisosAsync();
    }

    private async Task CargarPermisosAsync()
    {
        EstaCargando = true;
        HayErrorCarga = false;
        try
        {
            Permisos = await PermisoApiService.ObtenerTodosAsync();
        }
        catch
        {
            HayErrorCarga = true;
            MensajeError = "Error al cargar los permisos.";
        }
        finally
        {
            EstaCargando = false;
        }
    }
}