using Clinica.WASM.Constants;
using Clinica.WASM.DTOs.Auth;
using Clinica.WASM.Services.Api;
using Clinica.WASM.Services.Auth;
using Clinica.WASM.Themes;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Clinica.WASM.Pages.Auth;

public partial class LoginPage : ComponentBase
{
    [Inject] private AuthApiService AuthApiService { get; set; } = default!;
    [Inject] private TokenStorageService TokenStorageService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    protected MudTheme Theme { get; } = new ClinicaTheme();

    protected IniciarSesionDto LoginModel { get; set; } = new();

    protected bool EstaProcesando { get; set; }

    protected bool MostrarPassword { get; set; }

    protected bool IsDarkMode { get; set; }

    protected string MensajeError { get; set; } = string.Empty;

    protected async Task IniciarSesionAsync()
    {
        EstaProcesando = true;
        MensajeError = string.Empty;

        try
        {
            var respuesta = await AuthApiService.IniciarSesionAsync(LoginModel);

            if (respuesta?.Exitoso == true && respuesta.Data is not null)
            {
                await TokenStorageService.GuardarSesionAsync(
                    respuesta.Data.Token,
                    respuesta.Data.CodigoUsuario,
                    respuesta.Data.NombreCompleto,
                    respuesta.Data.Correo,
                    respuesta.Data.Roles,
                    respuesta.Data.Permisos
                );

                NavigationManager.NavigateTo(AppRoutes.Dashboard, replace: true);
                return;
            }

            MensajeError = respuesta?.Mensaje ?? "No se pudieron validar las credenciales.";
        }
        catch (HttpRequestException)
        {
            MensajeError = "Error de enlace: El servidor de la API SIGEC no responde.";
        }
        catch (Exception)
        {
            MensajeError = "Ocurrió un error inesperado al intentar iniciar sesión.";
        }
        finally
        {
            EstaProcesando = false;
        }
    }

    protected void ToggleMostrarPassword()
    {
        MostrarPassword = !MostrarPassword;
    }
}