namespace Clinica.Domain.Interfaces;

public interface IUsuarioActualService
{
    Guid ObtenerUsuarioId();
    Guid? ObtenerUsuarioIdOpcional();
}