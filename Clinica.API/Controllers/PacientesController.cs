using Clinica.API.Authorization;
using Clinica.API.Models;
using Clinica.API.Services;
using Clinica.Domain.DTOs.Pacientes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.API.Controllers;

/// <summary>
/// Controlador para la gestión completa de pacientes del sistema.
/// </summary>
/// <remarks>
/// **Ciclo de vida del paciente:**
/// 1. **Registro:** Se crea un paciente con sus datos básicos (DNI, nombres, fecha de nacimiento, etc.).
/// 2. **Apertura de historial:** Automáticamente se crea su historial clínico al registrarse.
/// 3. **Actualización:** Se pueden modificar sus datos de contacto (celular, correo, dirección).
/// 4. **Cambio de estado:** El paciente puede estar Activo, Inactivo, Bloqueado, Fallecido o Eliminado (lógica).
/// 
/// **Nota de Arquitectura:** La eliminación lógica de pacientes se realiza mediante el cambio de estado a "Eliminado".
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Tags("Gestión de Pacientes")]
public class PacientesController : ControllerBase
{
    private readonly IPacienteService _pacienteService;

    public PacientesController(IPacienteService pacienteService)
    {
        _pacienteService = pacienteService;
    }

    /// <summary>
    /// Obtiene la lista de todos los pacientes registrados en el sistema.
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Devuelve la lista completa de todos los pacientes, independientemente de su estado.
    /// Útil para la administración general y la búsqueda de pacientes.
    /// </remarks>
    /// <returns>Lista de objetos <see cref="PacienteResponseDto"/> con los datos de cada paciente.</returns>
    /// <response code="200">Pacientes obtenidos correctamente.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Acceso denegado.</response>
    [Authorize(Policy = PermisosPolicies.PacienteVer)]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PacienteResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll()
    {
        var pacientes = await _pacienteService.ObtenerTodosAsync();
        return Ok(ApiResponse<object>.Ok(pacientes, "Pacientes obtenidos correctamente."));
    }

    /// <summary>
    /// Obtiene los datos de un paciente por su ID.
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Permite recuperar la información completa de un paciente individual, incluyendo su historial clínico asociado.
    /// Útil para la ficha del paciente y pantallas de detalle.
    /// </remarks>
    /// <param name="id">Identificador único del paciente (GUID).</param>
    /// <returns>Objeto <see cref="PacienteResponseDto"/> con los datos del paciente.</returns>
    /// <response code="200">Paciente obtenido correctamente.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Acceso denegado.</response>
    /// <response code="404">Paciente no encontrado.</response>
    [Authorize(Policy = PermisosPolicies.PacienteVer)]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PacienteResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var paciente = await _pacienteService.ObtenerPorIdAsync(id);

        if (paciente == null)
            throw new KeyNotFoundException("Paciente no encontrado.");

        return Ok(ApiResponse<object>.Ok(paciente, "Paciente obtenido correctamente."));
    }

    /// <summary>
    /// Obtiene los datos de un paciente por su número de DNI.
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Permite buscar un paciente rápidamente utilizando su DNI.
    /// Útil para la admisión y la búsqueda en la recepción de la clínica.
    /// </remarks>
    /// <param name="dni">Número de DNI del paciente (8 dígitos).</param>
    /// <returns>Objeto <see cref="PacienteResponseDto"/> con los datos del paciente.</returns>
    /// <response code="200">Paciente obtenido correctamente.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Acceso denegado.</response>
    /// <response code="404">Paciente no encontrado.</response>
    [Authorize(Policy = PermisosPolicies.PacienteVer)]
    [HttpGet("dni/{dni}")]
    [ProducesResponseType(typeof(ApiResponse<PacienteResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByDni(string dni)
    {
        var paciente = await _pacienteService.ObtenerPorDniAsync(dni);

        if (paciente == null)
            throw new KeyNotFoundException("Paciente no encontrado.");

        return Ok(ApiResponse<object>.Ok(paciente, "Paciente obtenido correctamente."));
    }

    /// <summary>
    /// Registra un nuevo paciente en el sistema.
    /// </summary>
    /// <remarks>
    /// **Proceso de negocio:**
    /// 1. Valida que el DNI no esté ya registrado (es único).
    /// 2. Crea el paciente con estado "Activo".
    /// 3. Apertura automática de su historial clínico.
    /// 4. Registra un detalle de apertura en el historial.
    /// 
    /// **Nota:** Todos los pacientes creados quedan vinculados al usuario que los registra.
    /// </remarks>
    /// <param name="dto">Objeto <see cref="CrearPacienteDto"/> con los datos del nuevo paciente.</param>
    /// <returns>Objeto con el ID del paciente creado.</returns>
    /// <response code="201">Paciente registrado correctamente.</response>
    /// <response code="400">Datos inválidos o DNI ya existente.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Permisos insuficientes.</response>
    [Authorize(Policy = PermisosPolicies.PacienteCrear)]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CrearPacienteDto dto)
    {
        var id = await _pacienteService.CrearAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id },
            ApiResponse<object>.Ok(new { id }, "Paciente registrado correctamente.", 201)
        );
    }

    /// <summary>
    /// Actualiza los datos de contacto de un paciente.
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Permite actualizar solo los datos de contacto (celular, correo, dirección) de un paciente.
    /// 
    /// **Nota:** Para cambiar el estado del paciente, use el endpoint <see cref="CambiarEstado"/>.
    /// </remarks>
    /// <param name="id">Identificador único del paciente (GUID).</param>
    /// <param name="dto">Objeto <see cref="ActualizarContactoPacienteDto"/> con los nuevos datos de contacto.</param>
    /// <response code="200">Contacto actualizado correctamente.</response>
    /// <response code="400">Datos inválidos.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Permisos insuficientes.</response>
    /// <response code="404">Paciente no encontrado.</response>
    [Authorize(Policy = PermisosPolicies.PacienteEditar)]
    [HttpPut("{id:guid}/contacto")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateContact(Guid id, [FromBody] ActualizarContactoPacienteDto dto)
    {
        await _pacienteService.ActualizarContactoAsync(id, dto);

        return Ok(ApiResponse<object>.Ok(new { id }, "Contacto del paciente actualizado correctamente."));
    }

    /// <summary>
    /// Cambia el estado de un paciente.
    /// </summary>
    /// <remarks>
    /// **Propósito:**
    /// Permite cambiar el estado de un paciente (Activo, Inactivo, Bloqueado, Fallecido, Eliminado).
    /// 
    /// **Nota de Arquitectura:** La "eliminación" de un paciente es lógica y se realiza asignando el estado "Eliminado".
    /// Los pacientes eliminados no se pueden reactivar.
    /// </remarks>
    /// <param name="id">Identificador único del paciente (GUID).</param>
    /// <param name="dto">Objeto <see cref="CambiarEstadoPacienteDto"/> con el nuevo estado.</param>
    /// <response code="200">Estado actualizado correctamente.</response>
    /// <response code="400">El paciente ya está eliminado.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="403">Permisos insuficientes.</response>
    /// <response code="404">Paciente no encontrado.</response>
    [Authorize(Policy = PermisosPolicies.PacienteEditar)]
    [HttpPut("{id:guid}/estado")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CambiarEstado(Guid id, [FromBody] CambiarEstadoPacienteDto dto)
    {
        await _pacienteService.CambiarEstadoAsync(id, dto);
        return Ok(ApiResponse<object>.Ok(new { id }, "Estado del paciente actualizado correctamente."));
    }
}