namespace Clinica.WASM.DTOs.Pacientes;

public class PacienteResponseDto
{
    public Guid Id { get; set; }
    public string CodigoPaciente { get; set; } = string.Empty;
    public string DNI { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string NombreCompleto => $"{Nombres} {Apellidos}";
    public DateTime FechaNacimiento { get; set; }
    public string Sexo { get; set; } = string.Empty;
    public string? Celular { get; set; }
    public string? Correo { get; set; }
    public string? Direccion { get; set; }
    public EstadoPaciente Estado { get; set; }
    public DateTime FechaRegistro { get; set; }
    public string? CodigoHistorial { get; set; }
}