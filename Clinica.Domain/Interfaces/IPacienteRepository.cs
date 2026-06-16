using Clinica.Domain.Entities;

namespace Clinica.Domain.Interfaces;

public interface IPacienteRepository : IGenericRepository<Paciente>
{
    Task<Paciente?> ObtenerPorDniAsync(string dni);
    Task<Paciente?> ObtenerConHistorialAsync(Guid pacienteId);
}