using System.Net.Http.Json;
using Clinica.WASM.Constants;
using Clinica.WASM.DTOs.Common;
using Clinica.WASM.DTOs.Pacientes;

namespace Clinica.WASM.Services.Api;

public class PacienteApiService
{
    private readonly HttpClient _httpClient;
    private readonly ApiErrorService _apiErrorService;

    public PacienteApiService(HttpClient httpClient, ApiErrorService apiErrorService)
    {
        _httpClient = httpClient;
        _apiErrorService = apiErrorService;
    }

    public async Task<List<PacienteResponseDto>> ObtenerTodosAsync()
    {
        var respuesta = await _httpClient
            .GetFromJsonAsync<ApiResponse<List<PacienteResponseDto>>>(ApiEndpoints.Pacientes);

        return respuesta?.Data ?? new List<PacienteResponseDto>();
    }

    public async Task<PacienteResponseDto?> ObtenerPorIdAsync(Guid id)
    {
        var respuesta = await _httpClient
            .GetFromJsonAsync<ApiResponse<PacienteResponseDto>>($"{ApiEndpoints.Pacientes}/{id}");

        return respuesta?.Data;
    }

    public async Task<PacienteResponseDto?> ObtenerPorDniAsync(string dni)
    {
        var respuesta = await _httpClient
            .GetFromJsonAsync<ApiResponse<PacienteResponseDto>>($"{ApiEndpoints.Pacientes}/dni/{dni}");

        return respuesta?.Data;
    }

    public async Task<(bool Exitoso, string Mensaje)> CrearAsync(CrearPacienteDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync(ApiEndpoints.Pacientes, dto);

        if (response.IsSuccessStatusCode)
            return (true, "Paciente registrado correctamente.");

        var mensaje = await _apiErrorService.ObtenerMensajeErrorAsync(response);
        return (false, mensaje);
    }

    public async Task<(bool Exitoso, string Mensaje)> ActualizarContactoAsync(
        Guid id,
        ActualizarContactoPacienteDto dto)
    {
        var response = await _httpClient.PutAsJsonAsync(
            $"{ApiEndpoints.Pacientes}/{id}/contacto",
            dto);

        if (response.IsSuccessStatusCode)
            return (true, "Contacto actualizado correctamente.");

        var mensaje = await _apiErrorService.ObtenerMensajeErrorAsync(response);
        return (false, mensaje);
    }
    
    public async Task<(bool Exitoso, string Mensaje)> CambiarEstadoAsync(Guid id, CambiarEstadoPacienteDto dto)
    {
        var response = await _httpClient.PutAsJsonAsync($"{ApiEndpoints.Pacientes}/{id}/estado", dto);

        if (response.IsSuccessStatusCode)
            return (true, "Estado del paciente actualizado correctamente.");

        var mensaje = await _apiErrorService.ObtenerMensajeErrorAsync(response);
        return (false, mensaje);
    }
}