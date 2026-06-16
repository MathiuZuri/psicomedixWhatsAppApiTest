namespace Clinica.WASM.DTOs.Common;

public class ApiResponse<T>
{
    public bool Exitoso { get; set; }

    public string Mensaje { get; set; } = string.Empty;

    public int Codigo { get; set; }

    public T? Data { get; set; }

    public List<string> Errores { get; set; } = new();

    public DateTime Fecha { get; set; }
}