using System.Net.Http.Json;
using Clinica.WASM.Constants;
using Clinica.WASM.DTOs.Common;
using Clinica.WASM.DTOs.ServiciosClinicos;

namespace Clinica.WASM.Services.Api;

public class ServicioClinicoApiService
{
    private readonly HttpClient _httpClient;

    public ServicioClinicoApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<ServicioClinicoResponseDto>> ObtenerTodosAsync()
    {
        var respuesta = await _httpClient.GetFromJsonAsync<ApiResponse<List<ServicioClinicoResponseDto>>>(ApiEndpoints.ServiciosClinicos);
        return respuesta?.Data ?? new List<ServicioClinicoResponseDto>();
    }

    public async Task<List<ServicioClinicoResponseDto>> ObtenerActivosAsync()
    {
        var respuesta = await _httpClient.GetFromJsonAsync<ApiResponse<List<ServicioClinicoResponseDto>>>($"{ApiEndpoints.ServiciosClinicos}/activos");
        return respuesta?.Data ?? new List<ServicioClinicoResponseDto>();
    }

    public async Task<ServicioClinicoResponseDto?> ObtenerPorIdAsync(Guid id)
    {
        var respuesta = await _httpClient.GetFromJsonAsync<ApiResponse<ServicioClinicoResponseDto>>($"{ApiEndpoints.ServiciosClinicos}/{id}");
        return respuesta?.Data;
    }
}