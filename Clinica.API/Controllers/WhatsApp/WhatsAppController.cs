using Clinica.API.Services;
using Clinica.API.Services.Imp.WhastAppImp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.API.Controllers.WhatsApp;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Protegido para que solo usuarios de la clínica lo consulten
public class WhatsAppController : ControllerBase
{
    private readonly INotificacionWhatsAppService _whatsAppService;

    public WhatsAppController(INotificacionWhatsAppService whatsAppService)
    {
        _whatsAppService = whatsAppService;
    }

    [HttpGet("obtener-qr")]
    public async Task<IActionResult> GetQrCode()
    {
        var resultado = await _whatsAppService.ObtenerQrInstanciaAsync();
        
        if (string.IsNullOrEmpty(resultado))
            return BadRequest(new { mensaje = "No se pudo generar el código QR en este momento." });

        return Ok(new { data = resultado });
    }
}