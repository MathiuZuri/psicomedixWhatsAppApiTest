using Clinica.Domain.DTOs.Auth;

namespace Clinica.API.Services;

public interface IAuthService
{
    Task<RespuestaInicioSesionDto> IniciarSesionAsync(IniciarSesionDto dto);
}