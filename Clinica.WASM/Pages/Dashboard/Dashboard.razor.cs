
using Microsoft.AspNetCore.Components;

namespace Clinica.WASM.Pages.Dashboard;

public partial class Dashboard : ComponentBase
{
    protected int ContadorPacientes { get; set; }
    protected int ContadorCitas { get; set; }
    protected int ContadorAtenciones { get; set; }
    protected decimal BalanceCaja { get; set; }
    protected bool CargandoMetricas { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        await SimulatedFetchDataAsync();
    }
    
    private async Task SimulatedFetchDataAsync()
    {
        CargandoMetricas = true;
        try
        {
            // Simula el tiempo de respuesta de red del Back-end distribuido (.NET 9 Web API)
            await Task.Delay(650);

            ContadorPacientes = 1248;
            ContadorCitas = 14;
            ContadorAtenciones = 382;
            BalanceCaja = 14850.60m;
        }
        catch
        {
            // Espacio limpio para el control estructural de excepciones de auditoría contable
        }
        finally
        {
            CargandoMetricas = false;
        }
    }
}