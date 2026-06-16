using System.Net.Http.Json;
using Clinica.WASM.Constants;
using Clinica.WASM.DTOs.Common;
using Clinica.WASM.DTOs.Doctores;

namespace Clinica.WASM.Services.Api;

public class DoctorApiService
{
    private readonly HttpClient _httpClient;
    private readonly ApiErrorService _apiErrorService;

    public DoctorApiService(HttpClient httpClient, ApiErrorService apiErrorService)
    {
        _httpClient = httpClient;
        _apiErrorService = apiErrorService;
    }

    public async Task<List<DoctorResponseDto>> ObtenerTodosAsync()
    {
        var respuesta = await _httpClient.GetFromJsonAsync<ApiResponse<List<DoctorResponseDto>>>(ApiEndpoints.Doctores);
        return respuesta?.Data ?? new List<DoctorResponseDto>();
    }

    public async Task<List<DoctorResponseDto>> ObtenerActivosAsync()
    {
        var respuesta = await _httpClient.GetFromJsonAsync<ApiResponse<List<DoctorResponseDto>>>($"{ApiEndpoints.Doctores}/activos");
        return respuesta?.Data ?? new List<DoctorResponseDto>();
    }

    public async Task<DoctorResponseDto?> ObtenerPorIdAsync(Guid id)
    {
        var respuesta = await _httpClient.GetFromJsonAsync<ApiResponse<DoctorResponseDto>>($"{ApiEndpoints.Doctores}/{id}");
        return respuesta?.Data;
    }

    public async Task<(bool Exitoso, string Mensaje)> CrearAsync(CrearDoctorDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync(ApiEndpoints.Doctores, dto);
        if (response.IsSuccessStatusCode)
            return (true, "Doctor registrado correctamente.");

        var mensaje = await _apiErrorService.ObtenerMensajeErrorAsync(response);
        return (false, mensaje);
    }

    public async Task<(bool Exitoso, string Mensaje)> ActualizarAsync(Guid id, EditarDoctorDto dto)
    {
        var response = await _httpClient.PutAsJsonAsync($"{ApiEndpoints.Doctores}/{id}", dto);
        if (response.IsSuccessStatusCode)
            return (true, "Doctor actualizado correctamente.");

        var mensaje = await _apiErrorService.ObtenerMensajeErrorAsync(response);
        return (false, mensaje);
    }
}