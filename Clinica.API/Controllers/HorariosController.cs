using Clinica.API.Authorization;
using Clinica.API.Models;
using Clinica.API.Services;
using Clinica.Domain.DTOs.Horarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.API.Controllers;

/// <summary>
/// Controlador para la gestión de horarios y disponibilidad de los doctores.
/// </summary>
/// <remarks>
/// **Módulo de Horarios:** Permite definir, consultar y modificar los bloques horarios de disponibilidad de los doctores.
/// La matriz semanal es una herramienta visual que muestra la disponibilidad de un doctor para una semana específica.
/// 
/// **Nota de Arquitectura:** Los horarios son la base para la programación de citas. El sistema verifica automáticamente los conflictos de horario.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Tags("Horarios y Disponibilidad")]
public class HorariosController : ControllerBase
{
    private readonly IHorarioDoctorService _horarioService;

    public HorariosController(IHorarioDoctorService horarioService)
    {
        _horarioService = horarioService;
    }

    /// <summary>
    /// Obtiene todos los horarios registrados en el sistema.
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Devuelve la lista completa de todos los horarios definidos, independientemente del doctor.
    /// Útil para la administración general de disponibilidad.
    /// </remarks>
    /// <returns>Lista de objetos <see cref="HorarioDoctorResponseDto"/> con los datos de cada horario.</returns>
    /// <response code="200">Horarios obtenidos correctamente.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Acceso denegado.</response>
    [Authorize(Policy = PermisosPolicies.HorarioVer)]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<HorarioDoctorResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll()
    {
        var horarios = await _horarioService.ObtenerTodosAsync();
        return Ok(ApiResponse<object>.Ok(horarios, "Horarios obtenidos correctamente."));
    }

    /// <summary>
    /// Obtiene todos los horarios asociados a un doctor específico.
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Permite consultar la disponibilidad completa de un doctor, incluyendo sus bloques de tiempo y vigencias.
    /// Útil para la pantalla de gestión de horarios del doctor.
    /// </remarks>
    /// <param name="doctorId">Identificador único del doctor (GUID).</param>
    /// <returns>Lista de objetos <see cref="HorarioDoctorResponseDto"/> con los horarios del doctor.</returns>
    /// <response code="200">Horarios del doctor obtenidos correctamente.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Acceso denegado.</response>
    [Authorize(Policy = PermisosPolicies.HorarioVer)]
    [HttpGet("doctor/{doctorId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<HorarioDoctorResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByDoctor(Guid doctorId)
    {
        var horarios = await _horarioService.ObtenerPorDoctorAsync(doctorId);
        return Ok(ApiResponse<object>.Ok(horarios, "Horarios del doctor obtenidos correctamente."));
    }

    /// <summary>
    /// Crea un nuevo horario para un doctor.
    /// </summary>
    /// <remarks>
    /// **Proceso de negocio:**
    /// 1. Valida que el doctor exista.
    /// 2. Valida que la hora de fin sea posterior a la hora de inicio.
    /// 3. Valida que la fecha de fin de vigencia (si existe) sea posterior a la fecha de inicio.
    /// 4. Crea el horario con estado "Activo".
    /// 
    /// **Nota:** El horario se crea con una vigencia específica (fecha de inicio y opcionalmente fecha de fin).
    /// </remarks>
    /// <param name="dto">Objeto <see cref="CrearHorarioDoctorDto"/> con los datos del horario.</param>
    /// <returns>Objeto con el ID del horario creado.</returns>
    /// <response code="200">Horario registrado correctamente.</response>
    /// <response code="400">Datos inválidos o conflicto de horario.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Permisos insuficientes.</response>
    /// <response code="404">Doctor no encontrado.</response>
    [Authorize(Policy = PermisosPolicies.HorarioCrear)]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CrearHorarioDoctorDto dto)
    {
        var id = await _horarioService.CrearAsync(dto);
        return Ok(ApiResponse<object>.Ok(new { id }, "Horario registrado correctamente."));
    }

    /// <summary>
    /// Actualiza un horario existente.
    /// </summary>
    /// <remarks>
    /// **Proceso de negocio:**
    /// 1. Valida que el horario exista.
    /// 2. Valida que la hora de fin sea posterior a la hora de inicio.
    /// 3. Valida que la fecha de fin de vigencia (si existe) sea posterior a la fecha de inicio.
    /// 4. Actualiza los datos del horario.
    /// 
    /// **Nota de auditoría:** Esta acción queda registrada automáticamente.
    /// </remarks>
    /// <param name="id">Identificador único del horario a actualizar (GUID).</param>
    /// <param name="dto">Objeto <see cref="EditarHorarioDoctorDto"/> con los datos actualizados.</param>
    /// <response code="200">Horario actualizado correctamente.</response>
    /// <response code="400">Datos inválidos.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Permisos insuficientes.</response>
    /// <response code="404">Horario no encontrado.</response>
    [Authorize(Policy = PermisosPolicies.HorarioEditar)]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] EditarHorarioDoctorDto dto)
    {
        await _horarioService.ActualizarAsync(id, dto);
        return Ok(ApiResponse<object>.Ok(new { id }, "Horario actualizado correctamente."));
    }

    /// <summary>
    /// Obtiene la matriz semanal de disponibilidad de un doctor.
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Proporciona una representación visual (matriz) de la disponibilidad de un doctor para una semana completa.
    /// La matriz muestra cada bloque de 30 minutos y su estado (FueraHorario, Disponible, Ocupado).
    /// 
    /// **Nota de arquitectura:** La matriz se calcula combinando los horarios definidos para el doctor con las citas ya programadas.
    /// </remarks>
    /// <param name="doctorId">Identificador único del doctor (GUID).</param>
    /// <param name="fecha">Fecha de referencia para calcular la semana (por defecto, la fecha actual). Formato: yyyy-MM-dd.</param>
    /// <returns>Objeto <see cref="MatrizSemanalDto"/> con la matriz de disponibilidad.</returns>
    /// <response code="200">Matriz semanal calculada correctamente.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Acceso denegado.</response>
    /// <response code="404">Doctor no encontrado.</response>
    [Authorize(Policy = PermisosPolicies.HorarioVer)]
    [HttpGet("doctor/{doctorId:guid}/matriz")]
    [ProducesResponseType(typeof(ApiResponse<MatrizSemanalDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMatrizSemanal(Guid doctorId, [FromQuery] string? fecha)
    {
        // Si no se envía fecha, tomamos el día de hoy por defecto
        var fechaFiltro = string.IsNullOrEmpty(fecha) 
            ? DateOnly.FromDateTime(DateTime.Today) 
            : DateOnly.Parse(fecha);

        var matriz = await _horarioService.ObtenerMatrizSemanalAsync(doctorId, fechaFiltro);
        return Ok(ApiResponse<object>.Ok(matriz, "Matriz semanal calculada correctamente."));
    }
}