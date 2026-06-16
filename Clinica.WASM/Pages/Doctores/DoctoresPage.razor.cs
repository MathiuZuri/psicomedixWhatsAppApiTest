using Clinica.WASM.Components.Doctores;
using Clinica.WASM.Constants;
using Clinica.WASM.DTOs.Doctores;
using Clinica.WASM.Services.Api;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Clinica.WASM.Pages.Doctores;

public partial class DoctoresPage : ComponentBase
{
    [Inject] private DoctorApiService DoctorApiService { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;

    private List<DoctorResponseDto> Doctores { get; set; } = new();

    private CrearDoctorDto NuevoDoctor { get; set; } = new();
    private EditarDoctorDto EdicionDoctor { get; set; } = new();
    private DoctorResponseDto? DoctorSeleccionado { get; set; }

    private bool EstaCargando = true;
    private bool EstaProcesando;
    private bool MostrandoFormulario;
    private bool MostrandoEdicion;
    private bool HayErrorCarga;

    private string SearchText = string.Empty;
    private string MensajeError = string.Empty;
    private string MensajeExito = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await CargarDoctoresAsync();
    }

    private async Task CargarDoctoresAsync()
    {
        EstaCargando = true;
        HayErrorCarga = false;
        MensajeError = string.Empty;

        try
        {
            Doctores = await DoctorApiService.ObtenerTodosAsync();
        }
        catch
        {
            HayErrorCarga = true;
            MensajeError = "No se pudo cargar la lista de doctores.";
        }
        finally
        {
            EstaCargando = false;
        }
    }

    private void AbrirFormularioNuevo()
    {
        NuevoDoctor = new CrearDoctorDto();
        MostrandoFormulario = true;
        MostrandoEdicion = false;
        MensajeError = string.Empty;
        MensajeExito = string.Empty;
    }

    private void CerrarFormulario()
    {
        MostrandoFormulario = false;
        MostrandoEdicion = false;
        DoctorSeleccionado = null;
        MensajeError = string.Empty;
    }

    private async Task GuardarDoctorAsync()
    {
        EstaProcesando = true;
        MensajeError = string.Empty;
        MensajeExito = string.Empty;

        try
        {
            var resultado = await DoctorApiService.CrearAsync(NuevoDoctor);
            if (resultado.Exitoso)
            {
                MensajeExito = resultado.Mensaje;
                Snackbar.Add("Doctor registrado correctamente.", Severity.Success);
                MostrandoFormulario = false;
                await CargarDoctoresAsync();
            }
            else
            {
                MensajeError = resultado.Mensaje;
                Snackbar.Add(resultado.Mensaje, Severity.Error);
            }
        }
        catch
        {
            MensajeError = "No se pudo conectar con el servidor.";
        }
        finally
        {
            EstaProcesando = false;
        }
    }

    private async Task VerDoctor(DoctorResponseDto doctor)
    {
        var parameters = new DialogParameters { ["Doctor"] = doctor };
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Medium, FullWidth = true };
        await DialogService.ShowAsync<DoctorDetalleDialog>("Detalle del doctor", parameters, options);
    }

    private void AbrirEdicion(DoctorResponseDto doctor)
    {
        DoctorSeleccionado = doctor;
        EdicionDoctor = new EditarDoctorDto
        {
            CMP = doctor.CMP,
            Nombres = doctor.Nombres,
            Apellidos = doctor.Apellidos,
            Especialidad = doctor.Especialidad,
            Celular = doctor.Celular,
            Correo = doctor.Correo,
            FechaInicioContrato = doctor.FechaInicioContrato,
            FechaFinContrato = doctor.FechaFinContrato,
            Estado = doctor.Estado
        };

        MostrandoFormulario = false;
        MostrandoEdicion = true;
        MensajeError = string.Empty;
        MensajeExito = string.Empty;
    }

    private async Task ActualizarDoctorAsync()
    {
        if (DoctorSeleccionado is null)
        {
            MensajeError = "No se seleccionó ningún doctor.";
            return;
        }

        EstaProcesando = true;
        MensajeError = string.Empty;
        MensajeExito = string.Empty;

        try
        {
            var resultado = await DoctorApiService.ActualizarAsync(DoctorSeleccionado.Id, EdicionDoctor);
            if (resultado.Exitoso)
            {
                MensajeExito = resultado.Mensaje;
                Snackbar.Add("Doctor actualizado correctamente.", Severity.Success);
                MostrandoEdicion = false;
                DoctorSeleccionado = null;
                await CargarDoctoresAsync();
            }
            else
            {
                MensajeError = resultado.Mensaje;
                Snackbar.Add(resultado.Mensaje, Severity.Error);
            }
        }
        catch
        {
            MensajeError = "No se pudo conectar con el servidor.";
        }
        finally
        {
            EstaProcesando = false;
        }
    }
}