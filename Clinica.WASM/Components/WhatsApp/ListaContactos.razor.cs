using Clinica.WASM.DTOs.Chats;
using Microsoft.AspNetCore.Components;

namespace Clinica.WASM.Components.WhatsApp;

public partial class ListaContactos : ComponentBase
{
    [Parameter] public List<ChatResponseDto> Chats { get; set; } = new();
    [Parameter] public Guid? ChatSeleccionadoId { get; set; }
    [Parameter] public EventCallback<ChatResponseDto> OnChatSeleccionado { get; set; }

    protected ChatResponseDto? ChatSeleccionado { get; set; }
    protected string FiltroBusqueda { get; set; } = string.Empty;

    // Lógica de filtrado en tiempo real sin golpear el servidor
    protected IEnumerable<ChatResponseDto> ChatsFiltrados =>
        string.IsNullOrWhiteSpace(FiltroBusqueda)
            ? Chats
            : Chats.Where(c => c.NombreContacto.Contains(FiltroBusqueda, StringComparison.OrdinalIgnoreCase) ||
                               c.TelefonoWhatsApp.Contains(FiltroBusqueda));

    protected async Task SeleccionarChat(ChatResponseDto chat)
    {
        ChatSeleccionado = chat;
        await OnChatSeleccionado.InvokeAsync(chat);
    }
}