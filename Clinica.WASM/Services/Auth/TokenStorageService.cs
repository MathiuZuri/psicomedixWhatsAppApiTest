using Microsoft.JSInterop;

namespace Clinica.WASM.Services.Auth;

public class TokenStorageService
{
    private const string TokenKey = "clinica_token";
    private const string UserCodeKey = "clinica_user_code";
    private const string UserNameKey = "clinica_user_name";
    private const string UserEmailKey = "clinica_user_email";
    private const string RolesKey = "clinica_roles";
    private const string PermisosKey = "clinica_permisos";

    private readonly IJSRuntime _jsRuntime;

    public TokenStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task GuardarSesionAsync(
        string token,
        string codigoUsuario,
        string nombreCompleto,
        string correo,
        List<string> roles,
        List<string> permisos)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TokenKey, token);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", UserCodeKey, codigoUsuario);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", UserNameKey, nombreCompleto);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", UserEmailKey, correo);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", RolesKey, string.Join(",", roles));
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", PermisosKey, string.Join(",", permisos));
    }

    public async Task<string?> ObtenerTokenAsync()
    {
        return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", TokenKey);
    }

    public async Task<string?> ObtenerCodigoUsuarioAsync()
    {
        return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", UserCodeKey);
    }

    public async Task<string?> ObtenerNombreUsuarioAsync()
    {
        return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", UserNameKey);
    }

    public async Task<string?> ObtenerCorreoUsuarioAsync()
    {
        return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", UserEmailKey);
    }

    public async Task<List<string>> ObtenerRolesAsync()
    {
        var roles = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", RolesKey);

        return string.IsNullOrWhiteSpace(roles)
            ? new List<string>()
            : roles.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    public async Task<List<string>> ObtenerPermisosAsync()
    {
        var permisos = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", PermisosKey);

        return string.IsNullOrWhiteSpace(permisos)
            ? new List<string>()
            : permisos.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    public async Task LimpiarSesionAsync()
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", TokenKey);
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", UserCodeKey);
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", UserNameKey);
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", UserEmailKey);
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", RolesKey);
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", PermisosKey);
    }
}