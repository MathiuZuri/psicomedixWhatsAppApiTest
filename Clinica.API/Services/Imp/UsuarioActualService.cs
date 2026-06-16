using System.Security.Claims;
using Clinica.Domain.Interfaces;

namespace Clinica.API.Services.Imp;

public class UsuarioActualService : IUsuarioActualService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UsuarioActualService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid ObtenerUsuarioId()
    {
        var usuarioId = ObtenerUsuarioIdOpcional();

        if (usuarioId is null)
            throw new UnauthorizedAccessException("No se pudo identificar al usuario autenticado.");

        return usuarioId.Value;
    }

    public Guid? ObtenerUsuarioIdOpcional()
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (user == null || user.Identity?.IsAuthenticated != true)
            return null;

        var idClaim =
            user.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
            user.FindFirst("sub")?.Value ??
            user.FindFirst("usuarioId")?.Value ??
            user.FindFirst("UsuarioId")?.Value;

        if (Guid.TryParse(idClaim, out var usuarioId))
            return usuarioId;

        return null;
    }
}