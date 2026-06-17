using Clinica.WASM.DTOs.Chats;
using Clinica.WASM.Services.Api;
using Clinica.WASM.Services.Api.WhatsApp;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazor;

namespace Clinica.WASM.Pages.PsicomedixChat;

public partial class PsicomedixChat : ComponentBase, IAsyncDisposable
{
    [Inject] private ChatApiService ChatApiService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    protected List<ChatResponseDto> ChatsActivos { get; set; } = new();
    protected List<MensajeChatResponseDto> HistorialMensajes { get; set; } = new();
    protected ChatResponseDto? ChatSeleccionado { get; set; }

    private HubConnection? _hubConnection;

    protected override async Task OnInitializedAsync()
    {
        // 1. Cargar las conversaciones iniciales desde la Base de Datos
        ChatsActivos = await ChatApiService.ObtenerChatsActivosAsync();

        // 2. Configurar la autopista de SignalR apuntando al Hub del Backend
        _hubConnection = new HubConnectionBuilder()
            // En lugar de NavigationManager, apunta directo al puerto de tu BACKEND API
            .WithUrl("https://localhost:7241/chathub") 
            .WithAutomaticReconnect()
            .Build();

        // 3. Encender la oreja para capturar los mensajes en tiempo real (entrantes y salientes)
        _hubConnection.On<SignalrChatPayload>("RecibirNuevoMensaje", async (payload) =>
        {
            await InvokeAsync(() => ProcesarMensajeEnVivo(payload));
        });

        // 4. Arrancar la conexión inalámbrica
        try
        {
            await _hubConnection.StartAsync();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error de conexión en tiempo real: {ex.Message}", Severity.Error);
        }
    }

    private void ProcesarMensajeEnVivo(SignalrChatPayload payload)
    {
        // Caso A: El mensaje pertenece al chat que la secretaria tiene actualmente abierto en pantalla
        if (ChatSeleccionado != null && ChatSeleccionado.Id == payload.ChatId)
        {
            HistorialMensajes.Add(new MensajeChatResponseDto
            {
                Id = Guid.NewGuid(),
                ChatId = payload.ChatId,
                Texto = payload.Texto,
                EsMio = payload.EsMio,
                FechaEnvio = DateTime.Parse(payload.Fecha)
            });
            
            // Forzar limpieza de alertas visuales en la lista izquierda
            var chatLista = ChatsActivos.FirstOrDefault(c => c.Id == payload.ChatId);
            if (chatLista != null)
            {
                chatLista.UltimoMensaje = payload.Texto;
                chatLista.FechaUltimaInteraccion = DateTime.UtcNow;
                chatLista.MensajesNoLeidos = 0;
            }
        }
        // Caso B: El mensaje es de otro paciente o llegó estando en segundo plano
        else
        {
            var chatExistente = ChatsActivos.FirstOrDefault(c => c.Id == payload.ChatId);
            if (chatExistente != null)
            {
                chatExistente.UltimoMensaje = payload.Texto;
                chatExistente.FechaUltimaInteraccion = DateTime.UtcNow;
                chatExistente.MensajesNoLeidos = payload.MensajesNoLeidos;
            }
            else
            {
                // Si el chat es completamente nuevo en el sistema, refrescamos la lista completa
                InvokeAsync(async () => { ChatsActivos = await ChatApiService.ObtenerChatsActivosAsync(); StateHasChanged(); });
            }
        }

        // Reordenar la lista izquierda de inmediato para que el chat activo/reciente suba al top
        ChatsActivos = ChatsActivos.OrderByDescending(c => c.FechaUltimaInteraccion).ToList();
        StateHasChanged();
    }

    protected async Task CambiarChatActivoAsync(ChatResponseDto chat)
    {
        ChatSeleccionado = chat;
        chat.MensajesNoLeidos = 0; // Al hacer clic, limpiamos visualmente las alertas
        
        // Cargar las burbujas de texto de este paciente
        HistorialMensajes = await ChatApiService.ObtenerHistorialMensajesAsync(chat.Id);
    }

    protected async Task EnviarMensajeAlPacienteAsync(string textoMensaje)
    {
        if (ChatSeleccionado == null) return;

        var dto = new EnviarMensajeFrontDto
        {
            ChatId = ChatSeleccionado.Id,
            Texto = textoMensaje
        };

        // El endpoint HTTP se encarga de enviarlo a Evolution API y guardarlo en Postgres.
        // No necesitamos agregarlo manualmente a la lista aquí, porque el backend responderá 
        // disparando un evento de SignalR que nuestro método 'ProcesarMensajeEnVivo' capturará.
        var resultado = await ChatApiService.EnviarMensajeAsync(dto);

        if (!resultado.Exitoso)
        {
            Snackbar.Add($"No se pudo despachar por WhatsApp: {resultado.Mensaje}", Severity.Warning);
        }
    }

    // Liberar la conexión del WebSocket de forma limpia al salir de la página
    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
        }
    }

    // Clase auxiliar interna para tipar el JSON dinámico que viene desde SignalR
    public class SignalrChatPayload
    {
        public Guid ChatId { get; set; }
        public string Telefono { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Texto { get; set; } = string.Empty;
        public bool EsMio { get; set; }
        public string Fecha { get; set; } = string.Empty;
        public int MensajesNoLeidos { get; set; }
    }
}