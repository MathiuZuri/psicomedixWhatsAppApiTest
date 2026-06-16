using Microsoft.AspNetCore.Components;

namespace Clinica.WASM.Services.Api;

public class AuthRedirectService
{
    private NavigationManager? _navigationManager;

    public void SetNavigationManager(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }

    public void RedirectToLogin()
    {
        _navigationManager?.NavigateTo("/login", replace: true);
    }
}