using Clinica.API.Authorization;
using Clinica.API.Models;
using Clinica.API.Services;
using Clinica.Domain.DTOs.Citas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.API.Controllers;

/// <summary>
/// Controlador para la gestión integral de citas médicas.
/// </summary>
/// <remarks>
/// **Ciclo de vida de una cita:**
/// 1. **Programación:** Se crea una cita con el paciente, doctor, servicio y fecha/hora.
/// 2. **Verificación:** El sistema verifica automáticamente que no haya conflictos de horario con el doctor seleccionado.
/// 3. **Modificaciones:** La cita puede ser reprogramada (cambiar fecha/hora/doctor) o cancelada (con un motivo).
/// 4. **Finalización:** Cuando el paciente es atendido, la cita pasa al estado "Atendida" y se vincula a la atención médica correspondiente.
/// 
/// **Nota de Arquitectura:** Para garantizar la integridad de la agenda, las citas canceladas no se eliminan físicamente; se cambia su estado a "Cancelada" para mantener la trazabilidad y evitar vacíos en el historial del paciente.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Tags("Gestión de Citas")]
public class CitasController : ControllerBase
{
    private readonly ICitaService _citaService;

    public CitasController(ICitaService citaService)
    {
        _citaService = citaService;
    }

    /// <summary>
    /// Obtiene todas las citas registradas en el sistema.
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Este endpoint devuelve el listado completo de todas las citas, independientemente de su estado.
    /// Incluye información del paciente, doctor, servicio, fecha, hora y estado de la cita.
    /// 
    /// **Nota de rendimiento:** En entornos con muchas citas, se recomienda usar filtros por fecha o estado para mejorar el rendimiento de la consulta.
    /// </remarks>
    /// <returns>Lista de objetos <see cref="CitaResponseDto"/> con los datos de cada cita.</returns>
    /// <response code="200">Citas obtenidas correctamente.</response>
    /// <response code="401">No autorizado. Token JWT inválido o ausente.</response>
    /// <response code="403">Acceso denegado. El usuario no tiene el permiso requerido.</response>
    [Authorize(Policy = PermisosPolicies.CitaVer)]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CitaResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll()
    {
        var citas = await _citaService.ObtenerTodasAsync();
        return Ok(ApiResponse<object>.Ok(citas, "Citas obtenidas correctamente."));
    }

    /// <summary>
    /// Obtiene los detalles de una cita específica por su ID.
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Permite recuperar la información completa de una cita individual.
    /// Útil para la pantalla de detalle de la cita o para validar datos antes de realizar una acción.
    /// </remarks>
    /// <param name="id">Identificador único de la cita (formato GUID).</param>
    /// <returns>Objeto <see cref="CitaResponseDto"/> con los datos de la cita.</returns>
    /// <response code="200">Cita obtenida correctamente.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Acceso denegado.</response>
    /// <response code="404">Cita no encontrada.</response>
    [Authorize(Policy = PermisosPolicies.CitaVer)]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CitaResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var cita = await _citaService.ObtenerPorIdAsync(id);

        if (cita == null)
            throw new KeyNotFoundException("Cita no encontrada.");

        return Ok(ApiResponse<object>.Ok(cita, "Cita obtenida correctamente."));
    }

    /// <summary>
    /// Obtiene todas las citas asociadas a un paciente específico.
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Permite consultar el historial completo de citas de un paciente.
    /// Útil en la ficha del paciente para ver su actividad en la clínica.
    /// </remarks>
    /// <param name="pacienteId">Identificador único del paciente (formato GUID).</param>
    /// <returns>Lista de objetos <see cref="CitaResponseDto"/> con las citas del paciente.</returns>
    /// <response code="200">Citas del paciente obtenidas correctamente.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Acceso denegado.</response>
    [Authorize(Policy = PermisosPolicies.CitaVer)]
    [HttpGet("paciente/{pacienteId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CitaResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByPaciente(Guid pacienteId)
    {
        var citas = await _citaService.ObtenerPorPacienteAsync(pacienteId);
        return Ok(ApiResponse<object>.Ok(citas, "Citas del paciente obtenidas correctamente."));
    }

    /// <summary>
    /// Obtiene todas las citas asociadas a un doctor específico.
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Permite consultar la agenda completa de un doctor.
    /// Útil para el panel del doctor o para la gestión de disponibilidad.
    /// </remarks>
    /// <param name="doctorId">Identificador único del doctor (formato GUID).</param>
    /// <returns>Lista de objetos <see cref="CitaResponseDto"/> con las citas del doctor.</returns>
    /// <response code="200">Citas del doctor obtenidas correctamente.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Acceso denegado.</response>
    [Authorize(Policy = PermisosPolicies.CitaVer)]
    [HttpGet("doctor/{doctorId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CitaResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByDoctor(Guid doctorId)
    {
        var citas = await _citaService.ObtenerPorDoctorAsync(doctorId);
        return Ok(ApiResponse<object>.Ok(citas, "Citas del doctor obtenidas correctamente."));
    }

    /// <summary>
    /// Programa una nueva cita.
    /// </summary>
    /// <remarks>
    /// **Proceso de negocio:**
    /// 1. Valida que el paciente y el doctor existan.
    /// 2. Verifica que no haya conflictos de horario para el doctor en la fecha y hora seleccionadas.
    /// 3. Crea la cita con estado inicial "Pendiente".
    /// 4. Registra automáticamente un detalle en el historial clínico del paciente.
    /// 
    /// **Validaciones clave:**
    /// - La fecha de la cita no puede ser en el pasado.
    /// - La hora de fin debe ser posterior a la hora de inicio.
    /// - El doctor no puede tener otra cita en el mismo horario.
    /// </remarks>
    /// <param name="dto">Objeto <see cref="CrearCitaDto"/> con los datos de la cita.</param>
    /// <returns>Objeto con el ID de la cita creada.</returns>
    /// <response code="201">Cita creada correctamente. Retorna el ID de la cita.</response>
    /// <response code="400">Datos inválidos o conflicto de horario.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Permisos insuficientes para programar citas.</response>
    /// <response code="404">Paciente, doctor o servicio no encontrado.</response>
    [Authorize(Policy = PermisosPolicies.CitaProgramar)]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CrearCitaDto dto)
    {
        var id = await _citaService.CrearAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id },
            ApiResponse<object>.Ok(new { id }, "Cita programada correctamente.", 201)
        );
    }

    /// <summary>
    /// Reprograma una cita existente.
    /// </summary>
    /// <remarks>
    /// **Proceso de negocio:**
    /// 1. Valida que la cita exista.
    /// 2. Verifica que no haya conflictos de horario con el doctor en la nueva fecha/hora.
    /// 3. Actualiza la cita con los nuevos datos y cambia su estado a "Reprogramada".
    /// 4. Registra el motivo de la reprogramación en las observaciones.
    /// 
    /// **Nota:** La reprogramación puede implicar un cambio de doctor, fecha, hora o cualquier combinación.
    /// </remarks>
    /// <param name="id">Identificador de la cita a reprogramar (GUID).</param>
    /// <param name="dto">Objeto <see cref="ReprogramarCitaDto"/> con los nuevos datos.</param>
    /// <response code="200">Cita reprogramada correctamente.</response>
    /// <response code="400">Conflicto de horario o datos inválidos.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Permisos insuficientes para reprogramar citas.</response>
    /// <response code="404">Cita no encontrada.</response>
    [Authorize(Policy = PermisosPolicies.CitaReprogramar)]
    [HttpPut("{id:guid}/reprogramar")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reprogramar(Guid id, [FromBody] ReprogramarCitaDto dto)
    {
        await _citaService.ReprogramarAsync(id, dto);
        return Ok(ApiResponse<object>.Ok(new { id }, "Cita reprogramada correctamente."));
    }

    /// <summary>
    /// Cancela una cita existente.
    /// </summary>
    /// <remarks>
    /// **Proceso de negocio:**
    /// 1. Valida que la cita exista.
    /// 2. Cambia el estado de la cita a "Cancelada" (eliminación lógica).
    /// 3. Registra el motivo de cancelación en las observaciones.
    /// 
    /// **Nota de Arquitectura:** Las citas no se eliminan físicamente para mantener la trazabilidad y el historial del paciente.
    /// </remarks>
    /// <param name="id">Identificador de la cita a cancelar (GUID).</param>
    /// <param name="dto">Objeto <see cref="CancelarCitaDto"/> con el motivo de cancelación.</param>
    /// <response code="200">Cita cancelada correctamente.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Permisos insuficientes para cancelar citas.</response>
    /// <response code="404">Cita no encontrada.</response>
    [Authorize(Policy = PermisosPolicies.CitaCancelar)]
    [HttpPut("{id:guid}/cancelar")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancelar(Guid id, [FromBody] CancelarCitaDto dto)
    {
        await _citaService.CancelarAsync(id, dto);
        return Ok(ApiResponse<object>.Ok(new { id }, "Cita cancelada correctamente."));
    }
}