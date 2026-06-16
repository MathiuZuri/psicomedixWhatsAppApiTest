using System.Net.Http.Json;
using Clinica.WASM.Constants;
using Clinica.WASM.DTOs.Citas;
using Clinica.WASM.DTOs.Common;

namespace Clinica.WASM.Services.Api;

public class CitaApiService
{
    private readonly HttpClient _httpClient;
    private readonly ApiErrorService _apiErrorService;

    public CitaApiService(HttpClient httpClient, ApiErrorService apiErrorService)
    {
        _httpClient = httpClient;
        _apiErrorService = apiErrorService;
    }

    public async Task<List<CitaResponseDto>> ObtenerTodasAsync()
    {
        var respuesta = await _httpClient.GetFromJsonAsync<ApiResponse<List<CitaResponseDto>>>(ApiEndpoints.Citas);
        return respuesta?.Data ?? new List<CitaResponseDto>();
    }

    public async Task<CitaResponseDto?> ObtenerPorIdAsync(Guid id)
    {
        var respuesta = await _httpClient.GetFromJsonAsync<ApiResponse<CitaResponseDto>>($"{ApiEndpoints.Citas}/{id}");
        return respuesta?.Data;
    }

    public async Task<List<CitaResponseDto>> ObtenerPorPacienteAsync(Guid pacienteId)
    {
        var respuesta = await _httpClient.GetFromJsonAsync<ApiResponse<List<CitaResponseDto>>>($"{ApiEndpoints.Citas}/paciente/{pacienteId}");
        return respuesta?.Data ?? new List<CitaResponseDto>();
    }

    public async Task<List<CitaResponseDto>> ObtenerPorDoctorAsync(Guid doctorId)
    {
        var respuesta = await _httpClient.GetFromJsonAsync<ApiResponse<List<CitaResponseDto>>>($"{ApiEndpoints.Citas}/doctor/{doctorId}");
        return respuesta?.Data ?? new List<CitaResponseDto>();
    }

    public async Task<(bool Exitoso, string Mensaje)> CrearAsync(CrearCitaDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync(ApiEndpoints.Citas, dto);
        if (response.IsSuccessStatusCode)
            return (true, "Cita programada correctamente.");

        var mensaje = await _apiErrorService.ObtenerMensajeErrorAsync(response);
        return (false, mensaje);
    }

    public async Task<(bool Exitoso, string Mensaje)> ReprogramarAsync(Guid id, ReprogramarCitaDto dto)
    {
        var response = await _httpClient.PutAsJsonAsync($"{ApiEndpoints.Citas}/{id}/reprogramar", dto);
        if (response.IsSuccessStatusCode)
            return (true, "Cita reprogramada correctamente.");

        var mensaje = await _apiErrorService.ObtenerMensajeErrorAsync(response);
        return (false, mensaje);
    }

    public async Task<(bool Exitoso, string Mensaje)> CancelarAsync(Guid id, CancelarCitaDto dto)
    {
        var response = await _httpClient.PutAsJsonAsync($"{ApiEndpoints.Citas}/{id}/cancelar", dto);
        if (response.IsSuccessStatusCode)
            return (true, "Cita cancelada correctamente.");

        var mensaje = await _apiErrorService.ObtenerMensajeErrorAsync(response);
        return (false, mensaje);
    }
}