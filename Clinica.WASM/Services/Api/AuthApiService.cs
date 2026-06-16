using System.Net.Http.Json;
using Clinica.WASM.Constants;
using Clinica.WASM.DTOs.Auth;
using Clinica.WASM.DTOs.Common;

namespace Clinica.WASM.Services.Api;

public class AuthApiService
{
    private readonly HttpClient _httpClient;

    public AuthApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ApiResponse<RespuestaInicioSesionDto>?> IniciarSesionAsync(IniciarSesionDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync(ApiEndpoints.AuthLogin, dto);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<ApiResponse<RespuestaInicioSesionDto>>();
            return error ?? new ApiResponse<RespuestaInicioSesionDto>
            {
                Exitoso = false,
                Mensaje = "No se pudo iniciar sesión.",
                Codigo = (int)response.StatusCode
            };
        }

        return await response.Content.ReadFromJsonAsync<ApiResponse<RespuestaInicioSesionDto>>();
    }
}