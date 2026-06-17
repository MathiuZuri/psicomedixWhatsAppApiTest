using System.Net.Http.Json;

namespace Clinica.WASM.Services.Api.WhatsApp;

public class WhatsAppApiService
{
    private readonly HttpClient _httpClient;

    public WhatsAppApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string?> SolicitarQrDeVinculacionAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponseQr>("api/whatsapp/obtener-qr");
            return response?.Data;
        }
        catch
        {
            return null;
        }
    }

    private class ApiResponseQr { public string Data { get; set; } = string.Empty; }
}