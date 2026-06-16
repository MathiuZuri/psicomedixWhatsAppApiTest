using Clinica.WASM.Components.Roles;
using Clinica.WASM.Constants;
using Clinica.WASM.DTOs.Roles;
using Clinica.WASM.DTOs.Permisos;
using Clinica.WASM.Services.Api;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Clinica.WASM.Pages.Roles;

public partial class RolesPage : ComponentBase
{
    [Inject] private RolApiService RolApiService { get; set; } = default!;
    [Inject] private PermisoApiService PermisoApiService { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;

    private List<RolResponseDto> Roles { get; set; } = new();
    private List<PermisoResponseDto> PermisosList { get; set; } = new();

    private CrearRolDto NuevoRol { get; set; } = new();
    private EditarRolDto EdicionRol { get; set; } = new();
    private RolResponseDto? RolSeleccionado { get; set; }

    private bool EstaCargando = true;
    private bool EstaProcesando;
    private bool MostrandoFormulario;
    private bool MostrandoEdicion;
    private bool HayErrorCarga;

    private string SearchText = string.Empty;
    private string MensajeError = string.Empty;
    private string MensajeExito = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await CargarDatosAsync();
    }

    private async Task CargarDatosAsync()
    {
        EstaCargando = true;
        HayErrorCarga = false;
        try
        {
            await Task.WhenAll(
                CargarRolesAsync(),
                CargarPermisosAsync()
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

    private async Task CargarRolesAsync()
    {
        Roles = await RolApiService.ObtenerTodosAsync();
    }

    private async Task CargarPermisosAsync()
    {
        PermisosList = await PermisoApiService.ObtenerTodosAsync();
    }

    private void AbrirFormularioNuevo()
    {
        NuevoRol = new CrearRolDto();
        MostrandoFormulario = true;
        MostrandoEdicion = false;
        MensajeError = string.Empty;
        MensajeExito = string.Empty;
    }

    private void CerrarFormulario()
    {
        MostrandoFormulario = false;
        MostrandoEdicion = false;
        RolSeleccionado = null;
        MensajeError = string.Empty;
    }

    private async Task GuardarRolAsync()
    {
        EstaProcesando = true;
        MensajeError = string.Empty;
        try
        {
            var resultado = await RolApiService.CrearAsync(NuevoRol);
            if (resultado.Exitoso)
            {
                MensajeExito = resultado.Mensaje;
                Snackbar.Add("Rol creado.", Severity.Success);
                MostrandoFormulario = false;
                await CargarRolesAsync();
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

    private async Task VerRol(RolResponseDto rol)
    {
        var parameters = new DialogParameters { ["Rol"] = rol };
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Medium, FullWidth = true };
        await DialogService.ShowAsync<RolDetalleDialog>("Detalle del rol", parameters, options);
    }

    private void AbrirEdicion(RolResponseDto rol)
    {
        RolSeleccionado = rol;
        EdicionRol = new EditarRolDto
        {
            Nombre = rol.Nombre,
            Descripcion = rol.Descripcion,
            Activo = rol.Activo
        };
        MostrandoFormulario = false;
        MostrandoEdicion = true;
        MensajeError = string.Empty;
        MensajeExito = string.Empty;
    }

    private async Task ActualizarRolAsync()
    {
        if (RolSeleccionado is null) return;

        EstaProcesando = true;
        MensajeError = string.Empty;
        try
        {
            var resultado = await RolApiService.ActualizarAsync(RolSeleccionado.Id, EdicionRol);
            if (resultado.Exitoso)
            {
                Snackbar.Add("Rol actualizado.", Severity.Success);
                MostrandoEdicion = false;
                RolSeleccionado = null;
                await CargarRolesAsync();
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

    private async Task AbrirAsignacionPermisos(RolResponseDto rol)
    {
        var parameters = new DialogParameters
        {
            ["Rol"] = rol,
            ["TodosLosPermisos"] = PermisosList
        };
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Medium, FullWidth = true };
        var dialog = await DialogService.ShowAsync<AsignarPermisosDialog>("Asignar permisos", parameters, options);
        var result = await dialog.Result;
        if (!result.Canceled && result.Data is AsignarPermisosRolDto dto)
        {
            var exito = await RolApiService.AsignarPermisosAsync(dto);
            if (exito.Exitoso)
            {
                Snackbar.Add("Permisos asignados.", Severity.Success);
            }
            else
            {
                Snackbar.Add(exito.Mensaje, Severity.Error);
            }
        }
    }
}