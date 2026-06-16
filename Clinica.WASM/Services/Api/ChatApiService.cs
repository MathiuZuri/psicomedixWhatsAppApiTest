using System.Net.Http.Json;
using Clinica.WASM.DTOs.Common;
using Clinica.WASM.DTOs.Chats;

namespace Clinica.WASM.Services.Api;

public class ChatApiService
{
    private readonly HttpClient _httpClient;
    private readonly ApiErrorService _apiErrorService;
    private const string BaseEndpoint = "api/chats";

    public ChatApiService(HttpClient httpClient, ApiErrorService apiErrorService)
    {
        _httpClient = httpClient;
        _apiErrorService = apiErrorService;
    }

    /// <summary>
    /// Recupera la lista de todas las conversaciones activas para la columna izquierda.
    /// </summary>
    public async Task<List<ChatResponseDto>> ObtenerChatsActivosAsync()
    {
        try
        {
            var respuesta = await _httpClient
                .GetFromJsonAsync<List<ChatResponseDto>>(BaseEndpoint);

            return respuesta ?? new List<ChatResponseDto>();
        }
        catch
        {
            return new List<ChatResponseDto>();
        }
    }

    /// <summary>
    /// Carga todo el historial de mensajes de un chat específico para renderizar la conversación central.
    /// </summary>
    public async Task<List<MensajeChatResponseDto>> ObtenerHistorialMensajesAsync(Guid chatId)
    {
        try
        {
            var respuesta = await _httpClient
                .GetFromJsonAsync<List<MensajeChatResponseDto>>($"{BaseEndpoint}/{chatId}/mensajes");

            return respuesta ?? new List<MensajeChatResponseDto>();
        }
        catch
        {
            return new List<MensajeChatResponseDto>();
        }
    }

    /// <summary>
    /// Envía una respuesta de texto desde la clínica hacia el paciente.
    /// </summary>
    public async Task<(bool Exitoso, string Mensaje)> EnviarMensajeAsync(EnviarMensajeFrontDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync($"{BaseEndpoint}/enviar", dto);

        if (response.IsSuccessStatusCode)
            return (true, "Mensaje enviado con éxito.");

        var mensajeError = await _apiErrorService.ObtenerMensajeErrorAsync(response);
        return (false, mensajeError);
    }
}