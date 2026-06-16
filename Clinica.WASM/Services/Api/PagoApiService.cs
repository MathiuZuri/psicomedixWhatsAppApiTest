using System.Net.Http.Json;
using Clinica.WASM.Constants;
using Clinica.WASM.DTOs.Common;
using Clinica.WASM.DTOs.Pagos;

namespace Clinica.WASM.Services.Api;

public class PagoApiService
{
    private readonly HttpClient _httpClient;
    private readonly ApiErrorService _apiErrorService;

    public PagoApiService(HttpClient httpClient, ApiErrorService apiErrorService)
    {
        _httpClient = httpClient;
        _apiErrorService = apiErrorService;
    }

    public async Task<List<PagoResponseDto>> ObtenerPorPacienteAsync(Guid pacienteId)
    {
        var respuesta = await _httpClient.GetFromJsonAsync<ApiResponse<List<PagoResponseDto>>>($"{ApiEndpoints.Pagos}/paciente/{pacienteId}");
        return respuesta?.Data ?? new List<PagoResponseDto>();
    }
    
    public async Task<(bool Exitoso, string Mensaje)> CambiarEstadoAsync(Guid id, CambiarEstadoPagoDto dto)
    {
        var response = await _httpClient.PutAsJsonAsync($"{ApiEndpoints.Pagos}/{id}/estado", dto);
        if (response.IsSuccessStatusCode)
            return (true, "Estado del pago actualizado correctamente.");

        var mensaje = await _apiErrorService.ObtenerMensajeErrorAsync(response);
        return (false, mensaje);
    }

    public async Task<(bool Exitoso, string Mensaje)> RegistrarAsync(RegistrarPagoDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync(ApiEndpoints.Pagos, dto);
        if (response.IsSuccessStatusCode)
            return (true, "Pago registrado correctamente.");

        var mensaje = await _apiErrorService.ObtenerMensajeErrorAsync(response);
        return (false, mensaje);
    }
}