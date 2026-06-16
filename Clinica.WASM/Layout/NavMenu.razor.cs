using Clinica.WASM.Constants;
using Clinica.WASM.Services.Auth;
using Microsoft.AspNetCore.Components;

namespace Clinica.WASM.Layout;

public partial class NavMenu : ComponentBase
{
    [Inject] private AuthStateService AuthStateService { get; set; } = default!;

    // Atención clínica
    protected bool PuedeVerPacientes { get; set; }
    protected bool PuedeVerCitas { get; set; }
    protected bool PuedeVerServicios { get; set; }
    protected bool PuedeVerHistorial { get; set; }
    protected bool PuedeVerAtenciones { get; set; }

    // Administración
    protected bool PuedeVerDoctores { get; set; }
    protected bool PuedeVerHorarios { get; set; }
    protected bool PuedeVerPagos { get; set; }
    protected bool PuedeVerFinanzas { get; set; }
    protected bool PuedeVerComprobantes { get; set; }
    protected bool PuedeVerUsuarios { get; set; }

    // Seguridad
    protected bool PuedeVerRoles { get; set; }
    protected bool PuedeVerPermisos { get; set; }
    
    //Whatssapp
    protected bool PuedeVerChatLaboratorio { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        // Atención clínica
        PuedeVerPacientes    = await AuthStateService.TienePermisoAsync(Permisos.PacienteVer);
        PuedeVerCitas        = await AuthStateService.TienePermisoAsync(Permisos.CitaVer);
        PuedeVerServicios    = await AuthStateService.TienePermisoAsync(Permisos.ServicioVer);
        PuedeVerHistorial    = await AuthStateService.TienePermisoAsync(Permisos.HistorialVer);
        PuedeVerAtenciones   = await AuthStateService.TienePermisoAsync(Permisos.AtencionVer);

        // Administración
        PuedeVerDoctores     = await AuthStateService.TienePermisoAsync(Permisos.DoctorVer);
        PuedeVerHorarios     = await AuthStateService.TienePermisoAsync(Permisos.HorarioVer);
        PuedeVerPagos        = await AuthStateService.TienePermisoAsync(Permisos.PagoVer);
        PuedeVerFinanzas     = await AuthStateService.TienePermisoAsync(Permisos.FinanzasVer);
        PuedeVerComprobantes = await AuthStateService.TienePermisoAsync(Permisos.ComprobanteVer);
        PuedeVerUsuarios     = await AuthStateService.TienePermisoAsync(Permisos.UsuarioVer);

        // Seguridad
        PuedeVerRoles        = await AuthStateService.TienePermisoAsync(Permisos.RolVer);
        PuedeVerPermisos     = await AuthStateService.TienePermisoAsync(Permisos.PermisoVer);
        
        // Por ahora lo dejamos en true para desarrollo local directo.
        // A futuro, podrías amarrarlo a: await AuthStateService.TienePermisoAsync("Psicomedix.Chat");
        PuedeVerChatLaboratorio = true;
    }
}