using Clinica.WASM.Constants;
using Clinica.WASM.Services.Api;
using Clinica.WASM.Services.Auth;
using Clinica.WASM.Themes;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Clinica.WASM.Layout;

public partial class MainLayout : LayoutComponentBase
{
    [Inject] private AuthStateService AuthStateService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    protected MudTheme Theme { get; } = new ClinicaTheme();

    protected bool IsDarkMode { get; set; }

    protected bool DrawerOpen { get; set; } = true;

    protected string UserName { get; set; } = "Usuario";
    protected string UserEmail { get; set; } = "Sin correo";
    protected string UserCode { get; set; } = "SIN-CODIGO";
    protected string UserRole { get; set; } = "Sin rol";
    protected string UserInitials { get; set; } = "U";
    
    [Inject] private AuthRedirectService AuthRedirectService { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        AuthRedirectService.SetNavigationManager(NavigationManager);
        
        UserName = await AuthStateService.ObtenerNombreUsuarioAsync();
        UserEmail = await AuthStateService.ObtenerCorreoUsuarioAsync();
        UserCode = await AuthStateService.ObtenerCodigoUsuarioAsync();
        UserRole = await AuthStateService.ObtenerRolPrincipalAsync();

        UserInitials = ObtenerIniciales(UserName);
    }

    protected void ToggleDarkMode()
    {
        IsDarkMode = !IsDarkMode;
    }

    protected void ToggleDrawer()
    {
        DrawerOpen = !DrawerOpen;
    }

    protected async Task CerrarSesionAsync()
    {
        await AuthStateService.CerrarSesionAsync();
        NavigationManager.NavigateTo(AppRoutes.Login, replace: true);
    }

    private static string ObtenerIniciales(string nombreCompleto)
    {
        if (string.IsNullOrWhiteSpace(nombreCompleto))
            return "U";

        var partes = nombreCompleto
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (partes.Length == 1)
            return partes[0][0].ToString().ToUpperInvariant();

        return $"{partes[0][0]}{partes[^1][0]}".ToUpperInvariant();
    }
}