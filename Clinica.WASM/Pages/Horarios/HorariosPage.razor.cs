using Clinica.WASM.Components.Horarios;
using Clinica.WASM.Constants;
using Clinica.WASM.DTOs.Horarios;
using Clinica.WASM.DTOs.Doctores;
using Clinica.WASM.Services.Api;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Clinica.WASM.Pages.Horarios;

public partial class HorariosPage : ComponentBase
{
    [Inject] private HorarioDoctorApiService HorarioDoctorApiService { get; set; } = default!;
    [Inject] private DoctorApiService DoctorApiService { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;

    private List<HorarioDoctorResponseDto> Horarios { get; set; } = new();
    private List<DoctorResponseDto> DoctoresList { get; set; } = new();

    private CrearHorarioDoctorDto NuevoHorario { get; set; } = new();
    private EditarHorarioDoctorDto EdicionHorario { get; set; } = new();
    private HorarioDoctorResponseDto? DoctorSeleccionado { get; set; }

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
        await CargarDatosAsync();
    }

    private async Task CargarDatosAsync()
    {
        EstaCargando = true;
        HayErrorCarga = false;
        MensajeError = string.Empty;

        try
        {
            await Task.WhenAll(
                CargarHorariosAsync(),
                CargarDoctoresAsync()
            );
        }
        catch
        {
            HayErrorCarga = true;
            MensajeError = "No se pudieron cargar los datos.";
        }
        finally
        {
            EstaCargando = false;
        }
    }

    private async Task CargarHorariosAsync()
    {
        Horarios = await HorarioDoctorApiService.ObtenerTodosAsync();
    }

    private async Task CargarDoctoresAsync()
    {
        DoctoresList = await DoctorApiService.ObtenerTodosAsync();
    }

    private void AbrirFormularioNuevo()
    {
        NuevoHorario = new CrearHorarioDoctorDto();
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

    private async Task GuardarHorarioAsync()
    {
        EstaProcesando = true;
        MensajeError = string.Empty;
        MensajeExito = string.Empty;

        try
        {
            var resultado = await HorarioDoctorApiService.CrearAsync(NuevoHorario);
            if (resultado.Exitoso)
            {
                MensajeExito = resultado.Mensaje;
                Snackbar.Add("Horario registrado correctamente.", Severity.Success);
                MostrandoFormulario = false;
                await CargarHorariosAsync();
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

    private async Task VerHorario(HorarioDoctorResponseDto horario)
    {
        var parameters = new DialogParameters { ["Horario"] = horario };
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Medium, FullWidth = true };
        await DialogService.ShowAsync<HorarioDetalleDialog>("Detalle del horario", parameters, options);
    }

    private void AbrirEdicion(HorarioDoctorResponseDto horario)
    {
        DoctorSeleccionado = horario;
        EdicionHorario = new EditarHorarioDoctorDto
        {
            DiaSemana = horario.DiaSemana,
            HoraInicio = horario.HoraInicio,
            HoraFin = horario.HoraFin,
            FechaInicioVigencia = horario.FechaInicioVigencia,
            FechaFinVigencia = horario.FechaFinVigencia,
            Activo = horario.Activo
        };

        MostrandoFormulario = false;
        MostrandoEdicion = true;
        MensajeError = string.Empty;
        MensajeExito = string.Empty;
    }

    private async Task ActualizarHorarioAsync()
    {
        if (DoctorSeleccionado is null)
        {
            MensajeError = "No se seleccionó ningún horario.";
            return;
        }

        EstaProcesando = true;
        MensajeError = string.Empty;
        MensajeExito = string.Empty;

        try
        {
            var resultado = await HorarioDoctorApiService.ActualizarAsync(DoctorSeleccionado.Id, EdicionHorario);
            if (resultado.Exitoso)
            {
                MensajeExito = resultado.Mensaje;
                Snackbar.Add("Horario actualizado correctamente.", Severity.Success);
                MostrandoEdicion = false;
                DoctorSeleccionado = null;
                await CargarHorariosAsync();
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