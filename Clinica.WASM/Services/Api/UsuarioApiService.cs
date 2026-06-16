using System.Net.Http.Json;
using Clinica.WASM.Constants;
using Clinica.WASM.DTOs.Common;
using Clinica.WASM.DTOs.Usuarios;

namespace Clinica.WASM.Services.Api;

public class UsuarioApiService
{
    private readonly HttpClient _httpClient;
    private readonly ApiErrorService _apiErrorService;

    public UsuarioApiService(HttpClient httpClient, ApiErrorService apiErrorService)
    {
        _httpClient = httpClient;
        _apiErrorService = apiErrorService;
    }

    public async Task<List<UsuarioResponseDto>> ObtenerTodosAsync()
    {
        var respuesta = await _httpClient.GetFromJsonAsync<ApiResponse<List<UsuarioResponseDto>>>(ApiEndpoints.Usuarios);
        return respuesta?.Data ?? new List<UsuarioResponseDto>();
    }

    public async Task<UsuarioResponseDto?> ObtenerPorIdAsync(Guid id)
    {
        var respuesta = await _httpClient.GetFromJsonAsync<ApiResponse<UsuarioResponseDto>>($"{ApiEndpoints.Usuarios}/{id}");
        return respuesta?.Data;
    }

    public async Task<(bool Exitoso, string Mensaje)> CrearAsync(CrearUsuarioDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync(ApiEndpoints.Usuarios, dto);
        if (response.IsSuccessStatusCode)
            return (true, "Usuario creado correctamente.");

        var mensaje = await _apiErrorService.ObtenerMensajeErrorAsync(response);
        return (false, mensaje);
    }
    
    public async Task<(bool Exitoso, string Mensaje)> CambiarEstadoAsync(Guid id, CambiarEstadoUsuarioDto dto)
    {
        var response = await _httpClient.PutAsJsonAsync($"{ApiEndpoints.Usuarios}/{id}/estado", dto);
        if (response.IsSuccessStatusCode)
            return (true, "Estado actualizado correctamente.");

        var mensaje = await _apiErrorService.ObtenerMensajeErrorAsync(response);
        return (false, mensaje);
    }

    public async Task<(bool Exitoso, string Mensaje)> ActualizarAsync(Guid id, EditarUsuarioDto dto)
    {
        var response = await _httpClient.PutAsJsonAsync($"{ApiEndpoints.Usuarios}/{id}", dto);
        if (response.IsSuccessStatusCode)
            return (true, "Usuario actualizado correctamente.");

        var mensaje = await _apiErrorService.ObtenerMensajeErrorAsync(response);
        return (false, mensaje);
    }

    public async Task<(bool Exitoso, string Mensaje)> AsignarRolAsync(AsignarRolUsuarioDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync(ApiEndpoints.UsuariosAsignarRol, dto);
        if (response.IsSuccessStatusCode)
            return (true, "Rol asignado correctamente.");

        var mensaje = await _apiErrorService.ObtenerMensajeErrorAsync(response);
        return (false, mensaje);
    }
}