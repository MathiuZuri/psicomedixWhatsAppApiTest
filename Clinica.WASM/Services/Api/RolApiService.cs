using System.Net.Http.Json;
using Clinica.WASM.Constants;
using Clinica.WASM.DTOs.Common;
using Clinica.WASM.DTOs.Roles;

namespace Clinica.WASM.Services.Api;

public class RolApiService
{
    private readonly HttpClient _httpClient;
    private readonly ApiErrorService _apiErrorService;

    public RolApiService(HttpClient httpClient, ApiErrorService apiErrorService)
    {
        _httpClient = httpClient;
        _apiErrorService = apiErrorService;
    }

    public async Task<List<RolResponseDto>> ObtenerTodosAsync()
    {
        var respuesta = await _httpClient.GetFromJsonAsync<ApiResponse<List<RolResponseDto>>>(ApiEndpoints.Roles);
        return respuesta?.Data ?? new List<RolResponseDto>();
    }

    public async Task<RolResponseDto?> ObtenerPorIdAsync(Guid id)
    {
        var respuesta = await _httpClient.GetFromJsonAsync<ApiResponse<RolResponseDto>>($"{ApiEndpoints.Roles}/{id}");
        return respuesta?.Data;
    }

    public async Task<(bool Exitoso, string Mensaje)> CrearAsync(CrearRolDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync(ApiEndpoints.Roles, dto);
        if (response.IsSuccessStatusCode)
            return (true, "Rol creado correctamente.");

        var mensaje = await _apiErrorService.ObtenerMensajeErrorAsync(response);
        return (false, mensaje);
    }

    public async Task<(bool Exitoso, string Mensaje)> ActualizarAsync(Guid id, EditarRolDto dto)
    {
        var response = await _httpClient.PutAsJsonAsync($"{ApiEndpoints.Roles}/{id}", dto);
        if (response.IsSuccessStatusCode)
            return (true, "Rol actualizado correctamente.");

        var mensaje = await _apiErrorService.ObtenerMensajeErrorAsync(response);
        return (false, mensaje);
    }

    public async Task<(bool Exitoso, string Mensaje)> AsignarPermisosAsync(AsignarPermisosRolDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync(ApiEndpoints.RolesAsignarPermisos, dto);
        if (response.IsSuccessStatusCode)
            return (true, "Permisos asignados correctamente.");

        var mensaje = await _apiErrorService.ObtenerMensajeErrorAsync(response);
        return (false, mensaje);
    }
}