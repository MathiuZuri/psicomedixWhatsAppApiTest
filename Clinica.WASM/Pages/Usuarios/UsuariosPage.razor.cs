using Clinica.WASM.Components.Usuarios;
using Clinica.WASM.Constants;
using Clinica.WASM.DTOs.Usuarios;
using Clinica.WASM.DTOs.Roles;
using Clinica.WASM.Services.Api;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Clinica.WASM.Pages.Usuarios;

public partial class UsuariosPage : ComponentBase
{
    [Inject] private UsuarioApiService UsuarioApiService { get; set; } = default!;
    [Inject] private RolApiService RolApiService { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;

    private List<UsuarioResponseDto> Usuarios { get; set; } = new();
    private List<RolResponseDto> RolesList { get; set; } = new();

    private CrearUsuarioDto NuevoUsuario { get; set; } = new();
    private EditarUsuarioDto EdicionUsuario { get; set; } = new();
    private UsuarioResponseDto? UsuarioSeleccionado { get; set; }

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
                CargarUsuariosAsync(),
                CargarRolesAsync()
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

    private async Task CargarUsuariosAsync()
    {
        Usuarios = await UsuarioApiService.ObtenerTodosAsync();
    }

    private async Task CargarRolesAsync()
    {
        RolesList = await RolApiService.ObtenerTodosAsync();
    }

    private void AbrirFormularioNuevo()
    {
        NuevoUsuario = new CrearUsuarioDto();
        MostrandoFormulario = true;
        MostrandoEdicion = false;
        MensajeError = string.Empty;
        MensajeExito = string.Empty;
    }
    
    private async Task CambiarEstadoUsuario(UsuarioResponseDto usuario)
    {
        var parameters = new DialogParameters { ["Usuario"] = usuario };
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true };
        var dialog = await DialogService.ShowAsync<CambiarEstadoUsuarioDialog>("Cambiar estado del usuario", parameters, options);
        var result = await dialog.Result;

        if (!result.Canceled && result.Data is CambiarEstadoUsuarioDto dto)
        {
            var resultado = await UsuarioApiService.CambiarEstadoAsync(usuario.Id, dto);
            if (resultado.Exitoso)
            {
                Snackbar.Add("Estado actualizado correctamente.", Severity.Success);
                await CargarUsuariosAsync();
            }
            else
            {
                Snackbar.Add(resultado.Mensaje, Severity.Error);
            }
        }
    }

    private void CerrarFormulario()
    {
        MostrandoFormulario = false;
        MostrandoEdicion = false;
        UsuarioSeleccionado = null;
        MensajeError = string.Empty;
    }

    private async Task GuardarUsuarioAsync()
    {
        EstaProcesando = true;
        MensajeError = string.Empty;
        try
        {
            var resultado = await UsuarioApiService.CrearAsync(NuevoUsuario);
            if (resultado.Exitoso)
            {
                MensajeExito = resultado.Mensaje;
                Snackbar.Add("Usuario creado.", Severity.Success);
                MostrandoFormulario = false;
                await CargarUsuariosAsync();
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

    private async Task VerUsuario(UsuarioResponseDto usuario)
    {
        var parameters = new DialogParameters { ["Usuario"] = usuario };
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Medium, FullWidth = true };
        await DialogService.ShowAsync<UsuarioDetalleDialog>("Detalle del usuario", parameters, options);
    }

    private void AbrirEdicion(UsuarioResponseDto usuario)
    {
        UsuarioSeleccionado = usuario;
        EdicionUsuario = new EditarUsuarioDto
        {
            Nombres = usuario.Nombres,
            Apellidos = usuario.Apellidos,
            UserName = usuario.UserName,
            Correo = usuario.Correo
        };
        MostrandoFormulario = false;
        MostrandoEdicion = true;
        MensajeError = string.Empty;
        MensajeExito = string.Empty;
    }

    private async Task ActualizarUsuarioAsync()
    {
        if (UsuarioSeleccionado is null) return;

        EstaProcesando = true;
        MensajeError = string.Empty;
        try
        {
            var resultado = await UsuarioApiService.ActualizarAsync(UsuarioSeleccionado.Id, EdicionUsuario);
            if (resultado.Exitoso)
            {
                Snackbar.Add("Usuario actualizado.", Severity.Success);
                MostrandoEdicion = false;
                UsuarioSeleccionado = null;
                await CargarUsuariosAsync();
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

    private async Task AbrirAsignacionRol(UsuarioResponseDto usuario)
    {
        var parameters = new DialogParameters
        {
            ["Usuario"] = usuario,
            ["RolesList"] = RolesList
        };
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true };
        var dialog = await DialogService.ShowAsync<AsignarRolDialog>("Asignar rol", parameters, options);
        var result = await dialog.Result;
        if (!result.Canceled && result.Data is AsignarRolUsuarioDto dto)
        {
            var exito = await UsuarioApiService.AsignarRolAsync(dto);
            if (exito.Exitoso)
            {
                Snackbar.Add("Rol asignado.", Severity.Success);
            }
            else
            {
                Snackbar.Add(exito.Mensaje, Severity.Error);
            }
        }
    }
}