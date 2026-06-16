using System.Net.Http.Json;
using Clinica.WASM.Constants;
using Clinica.WASM.DTOs.Common;
using Clinica.WASM.DTOs.Permisos;

namespace Clinica.WASM.Services.Api;

public class PermisoApiService
{
    private readonly HttpClient _httpClient;

    public PermisoApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<PermisoResponseDto>> ObtenerTodosAsync()
    {
        var respuesta = await _httpClient.GetFromJsonAsync<ApiResponse<List<PermisoResponseDto>>>(ApiEndpoints.PermisosEndpoint);
        return respuesta?.Data ?? new List<PermisoResponseDto>();
    }
}