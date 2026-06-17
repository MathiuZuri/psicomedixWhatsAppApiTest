using Clinica.WASM.DTOs.Chats;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Clinica.WASM.Components.Chats; 

public partial class ListaContactos : ComponentBase
{
    [Parameter] public List<ChatResponseDto> Chats { get; set; } = new();
    [Parameter] public Guid? ChatSeleccionadoId { get; set; }
    [Parameter] public EventCallback<ChatResponseDto> OnChatSeleccionado { get; set; }

    protected ChatResponseDto? ChatSeleccionado { get; set; }
    protected string FiltroBusqueda { get; set; } = string.Empty;

    // Control de Estados de la Autopista de WhatsApp
    protected bool _cargandoEstado = true;
    protected bool _estaConectado;
    protected string? _qrBase64;

    protected IEnumerable<ChatResponseDto> ChatsFiltrados =>
        string.IsNullOrWhiteSpace(FiltroBusqueda)
            ? Chats
            : Chats.Where(c => c.NombreContacto.Contains(FiltroBusqueda, StringComparison.OrdinalIgnoreCase) ||
                               c.TelefonoWhatsApp.Contains(FiltroBusqueda));

    // Al iniciar el componente, verificamos la salud de la conexión de forma automática
    protected override async Task OnInitializedAsync()
    {
        await VerificarConexionAsync();
    }

    protected async Task VerificarConexionAsync()
    {
        _cargandoEstado = true;
        _qrBase64 = null;
        _estaConectado = false;
        StateHasChanged(); // Forzar refresco visual para pintar el spinner

        // Golpeamos nuestro servicio API para consultar a Evolution
        var resultado = await WhatsAppApiService.SolicitarQrDeVinculacionAsync();

        if (resultado == "CONNECTED")
        {
            _estaConectado = true;
        }
        else if (!string.IsNullOrEmpty(resultado))
        {
            _qrBase64 = resultado;
            _estaConectado = false;
        }
        else
        {
            Snackbar.Add("No se pudo leer el estado del servidor de WhatsApp.", Severity.Warning);
        }

        _cargandoEstado = false;
        StateHasChanged(); // Pintar la UI final según el estado obtenido
    }

    protected async Task SeleccionarChat(ChatResponseDto chat)
    {
        ChatSeleccionado = chat;
        await OnChatSeleccionado.InvokeAsync(chat);
    }
}