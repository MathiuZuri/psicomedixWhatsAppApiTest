using Clinica.WASM.Components.Pacientes;
using Clinica.WASM.DTOs.Pacientes;
using Clinica.WASM.Services.Api;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Clinica.WASM.Pages.Pacientes;

public partial class PacientesPage : ComponentBase
{
    [Inject] private PacienteApiService PacienteApiService { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;

    protected List<PacienteResponseDto> Pacientes { get; set; } = new();

    protected CrearPacienteDto NuevoPaciente { get; set; } = new()
    {
        FechaNacimiento = DateTime.Today,
        Sexo = "F"
    };

    protected ActualizarContactoPacienteDto ContactoPaciente { get; set; } = new();

    protected PacienteResponseDto? PacienteSeleccionado { get; set; }

    protected bool EstaCargando { get; set; } = true;
    protected bool EstaProcesando { get; set; }
    protected bool MostrandoFormulario { get; set; }
    protected bool MostrandoEdicionContacto { get; set; }
    protected bool HayErrorCarga { get; set; }

    protected string SearchText { get; set; } = string.Empty;
    protected string MensajeError { get; set; } = string.Empty;
    protected string MensajeExito { get; set; } = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await CargarPacientesAsync();
    }

    protected async Task CargarPacientesAsync()
    {
        EstaCargando = true;
        HayErrorCarga = false;
        MensajeError = string.Empty;

        try
        {
            Pacientes = await PacienteApiService.ObtenerTodosAsync();
        }
        catch
        {
            HayErrorCarga = true;
            MensajeError = "No se pudo cargar la lista de pacientes.";
        }
        finally
        {
            EstaCargando = false;
        }
    }

    protected void AbrirFormularioNuevo()
    {
        NuevoPaciente = new CrearPacienteDto
        {
            FechaNacimiento = DateTime.Today,
            Sexo = "F"
        };

        PacienteSeleccionado = null;
        MostrandoEdicionContacto = false;
        MensajeError = string.Empty;
        MensajeExito = string.Empty;
        MostrandoFormulario = true;
    }

    protected void CerrarFormulario()
    {
        MostrandoFormulario = false;
        MostrandoEdicionContacto = false;
        PacienteSeleccionado = null;
        MensajeError = string.Empty;
    }

    protected async Task GuardarPacienteAsync()
    {
        EstaProcesando = true;
        MensajeError = string.Empty;
        MensajeExito = string.Empty;

        try
        {
            var resultado = await PacienteApiService.CrearAsync(NuevoPaciente);

            if (resultado.Exitoso)
            {
                MensajeExito = resultado.Mensaje;
                Snackbar.Add("Paciente registrado correctamente.", Severity.Success);

                MostrandoFormulario = false;
                await CargarPacientesAsync();
                return;
            }

            MensajeError = resultado.Mensaje;
            Snackbar.Add(resultado.Mensaje, Severity.Error);
        }
        catch
        {
            MensajeError = "No se pudo conectar con el servidor.";
            Snackbar.Add(MensajeError, Severity.Error);
        }
        finally
        {
            EstaProcesando = false;
        }
    }

    protected async Task VerPaciente(PacienteResponseDto paciente)
    {
        var parameters = new DialogParameters
        {
            ["Paciente"] = paciente
        };

        var options = new DialogOptions
        {
            CloseButton = true,
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        };

        await DialogService.ShowAsync<PacienteDetalleDialog>(
            "Detalle del paciente",
            parameters,
            options
        );
    }

    protected void EditarContacto(PacienteResponseDto paciente)
    {
        PacienteSeleccionado = paciente;

        ContactoPaciente = new ActualizarContactoPacienteDto
        {
            Celular = paciente.Celular,
            Correo = paciente.Correo,
            Direccion = paciente.Direccion
        };

        MostrandoFormulario = false;
        MostrandoEdicionContacto = true;
        MensajeError = string.Empty;
        MensajeExito = string.Empty;
    }
    
    protected async Task CambiarEstadoPaciente(PacienteResponseDto paciente)
    {
        var parameters = new DialogParameters
        {
            ["Paciente"] = paciente
        };
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true };
        var dialog = await DialogService.ShowAsync<CambiarEstadoPacienteDialog>("Cambiar estado del paciente", parameters, options);
        var result = await dialog.Result;

        if (!result.Canceled && result.Data is CambiarEstadoPacienteDto dto)
        {
            var resultado = await PacienteApiService.CambiarEstadoAsync(paciente.Id, dto);

            if (resultado.Exitoso)
            {
                Snackbar.Add("Estado actualizado correctamente.", Severity.Success);
                await CargarPacientesAsync();
            }
            else
            {
                Snackbar.Add(resultado.Mensaje, Severity.Error);
            }
        }
    }

    protected async Task ActualizarContactoAsync()
    {
        if (PacienteSeleccionado is null)
        {
            MensajeError = "No se seleccionó ningún paciente.";
            return;
        }

        EstaProcesando = true;
        MensajeError = string.Empty;
        MensajeExito = string.Empty;

        try
        {
            var resultado = await PacienteApiService.ActualizarContactoAsync(
                PacienteSeleccionado.Id,
                ContactoPaciente
            );

            if (resultado.Exitoso)
            {
                MensajeExito = resultado.Mensaje;
                Snackbar.Add("Contacto actualizado correctamente.", Severity.Success);

                MostrandoEdicionContacto = false;
                PacienteSeleccionado = null;

                await CargarPacientesAsync();
                return;
            }

            MensajeError = resultado.Mensaje;
            Snackbar.Add(resultado.Mensaje, Severity.Error);
        }
        catch
        {
            MensajeError = "No se pudo conectar con el servidor.";
            Snackbar.Add(MensajeError, Severity.Error);
        }
        finally
        {
            EstaProcesando = false;
        }
    }
}