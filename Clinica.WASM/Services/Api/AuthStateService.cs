namespace Clinica.WASM.Services.Auth;

public class AuthStateService
{
    private readonly TokenStorageService _tokenStorage;
    
    private List<string>? _cachedRoles;
    private List<string>? _cachedPermisos;
    private bool _initialized;
    
    private async Task EnsureInitializedAsync()
    {
        if (!_initialized)
        {
            _cachedRoles = await _tokenStorage.ObtenerRolesAsync();
            _cachedPermisos = await _tokenStorage.ObtenerPermisosAsync();
            _initialized = true;
        }
    }

    public AuthStateService(TokenStorageService tokenStorage)
    {
        _tokenStorage = tokenStorage;
    }

    public async Task<bool> EstaAutenticadoAsync()
    {
        var token = await _tokenStorage.ObtenerTokenAsync();
        return !string.IsNullOrWhiteSpace(token) && !TokenExpirado(token);
    }

    public async Task<bool> TienePermisoAsync(string permiso)
    {
        await EnsureInitializedAsync();
        return _cachedPermisos?.Contains(permiso) ?? false;
    }

    public async Task<bool> TieneRolAsync(string rol)
    {
        await EnsureInitializedAsync();
        return _cachedRoles?.Contains(rol) ?? false;
    }

    public async Task<string> ObtenerCodigoUsuarioAsync()
    {
        return await _tokenStorage.ObtenerCodigoUsuarioAsync() ?? "SIN-CODIGO";
    }

    public async Task<string> ObtenerNombreUsuarioAsync()
    {
        return await _tokenStorage.ObtenerNombreUsuarioAsync() ?? "Usuario";
    }

    public async Task<string> ObtenerCorreoUsuarioAsync()
    {
        return await _tokenStorage.ObtenerCorreoUsuarioAsync() ?? "Sin correo";
    }

    public async Task<string> ObtenerRolPrincipalAsync()
    {
        var roles = await _tokenStorage.ObtenerRolesAsync();
        return roles.FirstOrDefault() ?? "Sin rol";
    }

    public async Task CerrarSesionAsync()
    {
        await _tokenStorage.LimpiarSesionAsync();
        _initialized = false;
    }
    
    private static bool TokenExpirado(string token)
    {
        try
        {
            var payload = token.Split('.')[1];
            var jsonBytes = Convert.FromBase64String(PaddingBase64(payload));
            var json = System.Text.Json.JsonDocument.Parse(jsonBytes);
            if (json.RootElement.TryGetProperty("exp", out var expElement))
            {
                var exp = expElement.GetInt64();
                var expDate = DateTimeOffset.FromUnixTimeSeconds(exp);
                return expDate <= DateTimeOffset.UtcNow;
            }
        }
        catch { }
        return true; // Si falla decodificación, considerar expirado
    }

    private static string PaddingBase64(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return base64;
    }
}