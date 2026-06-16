using Clinica.WASM.DTOs.Chats;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Clinica.WASM.Components.WhatsApp;

public partial class VentanaConversacion : ComponentBase
{
    [Parameter] public ChatResponseDto? ChatActivo { get; set; }
    [Parameter] public List<MensajeChatResponseDto> Mensajes { get; set; } = new();
    [Parameter] public EventCallback<string> OnEnviarMensaje { get; set; }

    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    protected string NuevoMensajeTexto { get; set; } = string.Empty;
    protected bool Enviando { get; set; }

    protected async Task ManejarTeclaEnter(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !string.IsNullOrWhiteSpace(NuevoMensajeTexto) && !Enviando)
        {
            await DispararEnvio();
        }
    }

    protected async Task DispararEnvio()
    {
        Enviando = true;
        var textoParaEnviar = NuevoMensajeTexto;
        NuevoMensajeTexto = string.Empty; // Limpieza inmediata de UI

        await OnEnviarMensaje.InvokeAsync(textoParaEnviar);
        Enviando = false;
    }

    // Método de ingeniería para forzar que el scroll baje automáticamente al recibir mensajes
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (ChatActivo != null)
        {
            await JsRuntime.InvokeVoidAsync("eval", "ref = document.getElementById('msg-scroll-container'); if(ref) ref.scrollTop = ref.scrollHeight;");
        }
    }
}