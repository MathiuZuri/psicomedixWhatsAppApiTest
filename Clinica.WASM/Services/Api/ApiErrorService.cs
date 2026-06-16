using System.Net.Http.Json;
using Clinica.WASM.DTOs.Common;

namespace Clinica.WASM.Services.Api;

public class ApiErrorService
{
    public async Task<string> ObtenerMensajeErrorAsync(HttpResponseMessage response)
    {
        try
        {
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();

            if (apiResponse is null)
                return "Ocurrió un error inesperado.";

            if (!string.IsNullOrWhiteSpace(apiResponse.Mensaje))
                return apiResponse.Mensaje;

            if (apiResponse.Errores.Count > 0)
                return string.Join(" ", apiResponse.Errores);
        }
        catch
        {
            // Ignorar error de lectura y devolver mensaje genérico.
        }

        return response.StatusCode switch
        {
            System.Net.HttpStatusCode.BadRequest => "La solicitud enviada no es válida.",
            System.Net.HttpStatusCode.Unauthorized => "Tu sesión no es válida o ha expirado.",
            System.Net.HttpStatusCode.Forbidden => "No tienes permisos para realizar esta acción.",
            System.Net.HttpStatusCode.NotFound => "El recurso solicitado no fue encontrado.",
            System.Net.HttpStatusCode.InternalServerError => "Ocurrió un error interno en el servidor.",
            _ => "No se pudo completar la operación."
        };
    }
}