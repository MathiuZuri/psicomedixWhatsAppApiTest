using System.Net.Http.Json;
using Clinica.WASM.Constants;
using Clinica.WASM.DTOs.Common;
using Clinica.WASM.DTOs.Horarios;

namespace Clinica.WASM.Services.Api;

public class HorarioDoctorApiService
{
    private readonly HttpClient _httpClient;
    private readonly ApiErrorService _apiErrorService;

    public HorarioDoctorApiService(HttpClient httpClient, ApiErrorService apiErrorService)
    {
        _httpClient = httpClient;
        _apiErrorService = apiErrorService;
    }

    public async Task<List<HorarioDoctorResponseDto>> ObtenerTodosAsync()
    {
        var respuesta = await _httpClient.GetFromJsonAsync<ApiResponse<List<HorarioDoctorResponseDto>>>(ApiEndpoints.Horarios);
        return respuesta?.Data ?? new List<HorarioDoctorResponseDto>();
    }

    public async Task<List<HorarioDoctorResponseDto>> ObtenerPorDoctorAsync(Guid doctorId)
    {
        var respuesta = await _httpClient.GetFromJsonAsync<ApiResponse<List<HorarioDoctorResponseDto>>>($"{ApiEndpoints.Horarios}/doctor/{doctorId}");
        return respuesta?.Data ?? new List<HorarioDoctorResponseDto>();
    }
    
    public async Task<Clinica.Domain.DTOs.Horarios.MatrizSemanalDto?> ObtenerMatrizSemanalAsync(Guid doctorId, DateOnly fecha)
    {
        var url = $"{ApiEndpoints.Horarios}/doctor/{doctorId}/matriz?fecha={fecha:yyyy-MM-dd}";
        var respuesta = await _httpClient.GetFromJsonAsync<ApiResponse<Clinica.Domain.DTOs.Horarios.MatrizSemanalDto>>(url);
        return respuesta?.Data;
    }

    public async Task<(bool Exitoso, string Mensaje)> CrearAsync(CrearHorarioDoctorDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync(ApiEndpoints.Horarios, dto);
        if (response.IsSuccessStatusCode)
            return (true, "Horario registrado correctamente.");

        var mensaje = await _apiErrorService.ObtenerMensajeErrorAsync(response);
        return (false, mensaje);
    }

    public async Task<(bool Exitoso, string Mensaje)> ActualizarAsync(Guid id, EditarHorarioDoctorDto dto)
    {
        var response = await _httpClient.PutAsJsonAsync($"{ApiEndpoints.Horarios}/{id}", dto);
        if (response.IsSuccessStatusCode)
            return (true, "Horario actualizado correctamente.");

        var mensaje = await _apiErrorService.ObtenerMensajeErrorAsync(response);
        return (false, mensaje);
    }
}