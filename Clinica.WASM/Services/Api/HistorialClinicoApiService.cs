using System.Net.Http.Json;
using Clinica.WASM.Constants;
using Clinica.WASM.DTOs.Common;
using Clinica.WASM.DTOs.Historiales;

namespace Clinica.WASM.Services.Api;

public class HistorialClinicoApiService
{
    private readonly HttpClient _httpClient;

    public HistorialClinicoApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<HistorialClinicoResponseDto?> ObtenerPorPacienteAsync(Guid pacienteId)
    {
        var respuesta = await _httpClient.GetFromJsonAsync<ApiResponse<HistorialClinicoResponseDto>>($"{ApiEndpoints.Historiales}/paciente/{pacienteId}");
        return respuesta?.Data;
    }

    public async Task<HistorialClinicoResponseDto?> ObtenerConDetallesAsync(Guid historialId)
    {
        var respuesta = await _httpClient.GetFromJsonAsync<ApiResponse<HistorialClinicoResponseDto>>($"{ApiEndpoints.Historiales}/{historialId}/detalles");
        return respuesta?.Data;
    }
}