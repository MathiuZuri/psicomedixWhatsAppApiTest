using System.Net.Http.Json;
using Clinica.WASM.Constants;
using Clinica.WASM.DTOs.Common;
using Clinica.WASM.DTOs.Finanzas;

namespace Clinica.WASM.Services.Api;

public class FinanzasApiService
{
    private readonly HttpClient _httpClient;
    private readonly ApiErrorService _apiErrorService;

    public FinanzasApiService(HttpClient httpClient, ApiErrorService apiErrorService)
    {
        _httpClient = httpClient;
        _apiErrorService = apiErrorService;
    }

    public async Task<ResumenFinancieroMensualCompletoDto?> ObtenerResumenMensualCompletoAsync(int anio, int mes)
    {
        var respuesta = await _httpClient.GetFromJsonAsync<ApiResponse<ResumenFinancieroMensualCompletoDto>>($"{ApiEndpoints.FinanzasResumenMensualCompleto}?anio={anio}&mes={mes}");
        return respuesta?.Data;
    }

    public async Task<List<EstadoPagoAtencionDto>> ObtenerDeudasRealesAsync()
    {
        var respuesta = await _httpClient.GetFromJsonAsync<ApiResponse<List<EstadoPagoAtencionDto>>>(ApiEndpoints.FinanzasDeudasReales);
        return respuesta?.Data ?? new();
    }

    public async Task<EstadoCuentaPacienteDto?> ObtenerEstadoCuentaPacienteAsync(Guid pacienteId)
    {
        var respuesta = await _httpClient.GetFromJsonAsync<ApiResponse<EstadoCuentaPacienteDto>>($"{ApiEndpoints.FinanzasEstadoCuentaPaciente}/{pacienteId}/estado-cuenta");
        return respuesta?.Data;
    }

    public async Task<List<AjusteFinancieroDto>> ObtenerAjustesFinancierosAsync()
    {
        var respuesta = await _httpClient.GetFromJsonAsync<ApiResponse<List<AjusteFinancieroDto>>>(ApiEndpoints.FinanzasAjustes);
        return respuesta?.Data ?? new();
    }

    public async Task<List<AjusteFinancieroDto>> ObtenerAjustesPorPagoAsync(Guid pagoId)
    {
        var respuesta = await _httpClient.GetFromJsonAsync<ApiResponse<List<AjusteFinancieroDto>>>($"{ApiEndpoints.FinanzasAjustesPorPago}/{pagoId}/ajustes-financieros");
        return respuesta?.Data ?? new();
    }

    public async Task<(bool Exitoso, string Mensaje)> RegistrarAjusteFinancieroAsync(RegistrarAjusteFinancieroDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync(ApiEndpoints.FinanzasAjustesRegistrar, dto);
        if (response.IsSuccessStatusCode)
            return (true, "Ajuste registrado.");
        var msg = await _apiErrorService.ObtenerMensajeErrorAsync(response);
        return (false, msg);
    }
}