using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Clinica.API.Configurations;
using Microsoft.Extensions.Options;

namespace Clinica.API.Services.Imp;
// esto es exclusivo de evolution api, no incluir al sistema
public class EvolutionWhatsAppService : INotificacionWhatsAppService
{
    private readonly HttpClient _httpClient;
    private readonly WhatsAppOptions _options;
    private readonly ILogger<EvolutionWhatsAppService> _logger;

    public EvolutionWhatsAppService(
        HttpClient httpClient,
        IOptions<WhatsAppOptions> options,
        ILogger<EvolutionWhatsAppService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task EnviarMensajeAsync(
        string telefonoDestino,
        string mensaje,
        CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("WhatsApp deshabilitado. No se envió el mensaje.");
            return;
        }

        var telefonoNormalizado = NormalizarTelefonoPeru(telefonoDestino);

        var payload = new
        {
            number = telefonoNormalizado,
            text = mensaje
        };

        var json = JsonSerializer.Serialize(payload);

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/message/sendText/{_options.InstanceName}");

        request.Headers.Add("apikey", _options.ApiKey);

        request.Content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.SendAsync(request, cancellationToken);

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Error enviando WhatsApp. StatusCode: {StatusCode}. Body: {Body}",
                response.StatusCode,
                responseBody);

            throw new InvalidOperationException(
                $"Error enviando mensaje por WhatsApp: {response.StatusCode}");
        }

        _logger.LogInformation(
            "Mensaje WhatsApp enviado correctamente a {Telefono}",
            telefonoNormalizado);
    }

    private static string NormalizarTelefonoPeru(string telefono)
    {
        var limpio = telefono
            .Replace("+", "")
            .Replace(" ", "")
            .Replace("-", "")
            .Replace("(", "")
            .Replace(")", "");

        if (limpio.StartsWith("51"))
            return limpio;

        if (limpio.Length == 9)
            return $"51{limpio}";

        return limpio;
    }
}