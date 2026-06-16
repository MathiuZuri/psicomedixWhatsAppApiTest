using Clinica.API.Authorization;
using Clinica.API.Models;
using Clinica.API.Services;
using Clinica.Domain.DTOs.Doctores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.API.Controllers;

/// <summary>
/// Controlador para la gestión del cuerpo médico (doctores) del sistema.
/// </summary>
/// <remarks>
/// **Ciclo de vida del doctor en el sistema:**
/// 1. **Registro:** Se crea un doctor con su CMP, datos personales y fechas de contrato.
/// 2. **Actualización:** Se pueden modificar sus datos personales, especialidad y estado.
/// 3. **Control de estado:** El doctor puede estar Activo, Inactivo, ContratoVencido, Suspendido o Eliminado (lógica).
/// 4. **Vigencia:** El sistema controla las fechas de inicio y fin de contrato para gestionar automáticamente el estado.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Tags("Gestión de Doctores")]
public class DoctoresController : ControllerBase
{
    private readonly IDoctorService _doctorService;

    public DoctoresController(IDoctorService doctorService)
    {
        _doctorService = doctorService;
    }

    /// <summary>
    /// Obtiene la lista completa de todos los doctores registrados en el sistema.
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Este endpoint devuelve todos los doctores, independientemente de su estado (activo, inactivo, etc.).
    /// Útil para la administración general del personal médico.
    /// 
    /// **Nota de rendimiento:** Para consultas frecuentes, considere usar el endpoint de doctores activos para reducir el volumen de datos.
    /// </remarks>
    /// <returns>Lista de objetos <see cref="DoctorResponseDto"/> con los datos de cada doctor.</returns>
    /// <response code="200">Doctores obtenidos correctamente.</response>
    /// <response code="401">No autorizado. Token JWT inválido o ausente.</response>
    /// <response code="403">Acceso denegado. El usuario no tiene el permiso requerido.</response>
    [Authorize(Policy = PermisosPolicies.DoctorVer)]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<DoctorResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll()
    {
        var doctores = await _doctorService.ObtenerTodosAsync();
        return Ok(ApiResponse<object>.Ok(doctores, "Doctores obtenidos correctamente."));
    }

    /// <summary>
    /// Obtiene la lista de doctores activos (aquellos con estado "Activo").
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Este endpoint es el más utilizado para la programación de citas, ya que solo se pueden asignar doctores activos.
    /// Filtra automáticamente a los doctores que están disponibles para atender pacientes.
    /// </remarks>
    /// <returns>Lista de objetos <see cref="DoctorResponseDto"/> de doctores activos.</returns>
    /// <response code="200">Doctores activos obtenidos correctamente.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Acceso denegado.</response>
    [Authorize(Policy = PermisosPolicies.DoctorVer)]
    [HttpGet("activos")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<DoctorResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetActivos()
    {
        var doctores = await _doctorService.ObtenerActivosAsync();
        return Ok(ApiResponse<object>.Ok(doctores, "Doctores activos obtenidos correctamente."));
    }

    /// <summary>
    /// Obtiene los datos de un doctor específico por su ID.
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Permite recuperar la información completa de un doctor individual.
    /// Útil para la pantalla de detalle del doctor o para validar datos antes de una actualización.
    /// </remarks>
    /// <param name="id">Identificador único del doctor (formato GUID).</param>
    /// <returns>Objeto <see cref="DoctorResponseDto"/> con los datos del doctor.</returns>
    /// <response code="200">Doctor obtenido correctamente.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Acceso denegado.</response>
    /// <response code="404">Doctor no encontrado.</response>
    [Authorize(Policy = PermisosPolicies.DoctorVer)]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<DoctorResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var doctor = await _doctorService.ObtenerPorIdAsync(id);

        if (doctor == null)
            throw new KeyNotFoundException("Doctor no encontrado.");

        return Ok(ApiResponse<object>.Ok(doctor, "Doctor obtenido correctamente."));
    }

    /// <summary>
    /// Registra un nuevo doctor en el sistema.
    /// </summary>
    /// <remarks>
    /// **Proceso de negocio:**
    /// 1. Valida que el CMP no esté ya registrado (es único).
    /// 2. Valida que las fechas de contrato sean consistentes (fin > inicio).
    /// 3. Crea el doctor con estado inicial "Activo".
    /// 4. Genera un código de doctor único basado en el CMP.
    /// 
    /// **Nota:** El usuario autenticado que realiza el registro queda registrado como responsable.
    /// </remarks>
    /// <param name="dto">Objeto <see cref="CrearDoctorDto"/> con los datos del nuevo doctor.</param>
    /// <returns>Objeto con el ID del doctor creado.</returns>
    /// <response code="201">Doctor registrado correctamente.</response>
    /// <response code="400">Datos inválidos o CMP ya existente.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Permisos insuficientes para crear doctores.</response>
    [Authorize(Policy = PermisosPolicies.DoctorCrear)]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CrearDoctorDto dto)
    {
        var id = await _doctorService.CrearAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id },
            ApiResponse<object>.Ok(new { id }, "Doctor registrado correctamente.", 201)
        );
    }

    /// <summary>
    /// Actualiza los datos de un doctor existente.
    /// </summary>
    /// <remarks>
    /// **Proceso de negocio:**
    /// 1. Valida que el doctor exista.
    /// 2. Valida que las fechas de contrato sean consistentes.
    /// 3. Actualiza todos los campos permitidos (CMP, nombres, especialidad, estado, etc.).
    /// 
    /// **Nota de auditoría:** Esta acción queda registrada automáticamente por el filtro <see cref="AuditoriaAutomaticaFilter"/>.
    /// </remarks>
    /// <param name="id">Identificador único del doctor a actualizar (GUID).</param>
    /// <param name="dto">Objeto <see cref="EditarDoctorDto"/> con los datos actualizados.</param>
    /// <response code="200">Doctor actualizado correctamente.</response>
    /// <response code="400">Datos inválidos o fechas de contrato inconsistentes.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Permisos insuficientes para editar doctores.</response>
    /// <response code="404">Doctor no encontrado.</response>
    [Authorize(Policy = PermisosPolicies.DoctorEditar)]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] EditarDoctorDto dto)
    {
        await _doctorService.ActualizarAsync(id, dto);
        return Ok(ApiResponse<object>.Ok(new { id }, "Doctor actualizado correctamente."));
    }
}