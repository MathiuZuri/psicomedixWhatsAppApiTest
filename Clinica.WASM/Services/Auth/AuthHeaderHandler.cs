using System.Net.Http.Headers;
using Clinica.WASM.Services.Api;
using Clinica.WASM.Services.Auth;

namespace Clinica.WASM.Services.Auth;

public class AuthHeaderHandler : DelegatingHandler
{
    private readonly TokenStorageService _tokenStorageService;
    private readonly AuthRedirectService _authRedirectService;

    public AuthHeaderHandler(TokenStorageService tokenStorageService, AuthRedirectService authRedirectService)
    {
        _tokenStorageService = tokenStorageService;
        _authRedirectService = authRedirectService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _tokenStorageService.ObtenerTokenAsync();
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            await _tokenStorageService.LimpiarSesionAsync();
            _authRedirectService.RedirectToLogin();
        }

        return response;
    }
}