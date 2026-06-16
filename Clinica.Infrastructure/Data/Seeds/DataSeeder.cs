using Clinica.Domain.Entities;
using Clinica.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infrastructure.Data.Seeds;

public static class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        await context.Database.MigrateAsync();

        await SeedPermisosAsync(context);
        await SeedRolesAsync(context);
        await SeedUsuarioAdminAsync(context);
        await SeedServiciosClinicosAsync(context);

        await context.SaveChangesAsync();
    }

    private static async Task SeedPermisosAsync(ApplicationDbContext context)
    {
        var permisosBase = new List<Permiso>
        {
            new() { Codigo = "PACIENTE_VER", Nombre = "Ver pacientes", Modulo = "Pacientes", Activo = true },
            new() { Codigo = "PACIENTE_CREAR", Nombre = "Crear pacientes", Modulo = "Pacientes", Activo = true },
            new() { Codigo = "PACIENTE_EDITAR", Nombre = "Editar pacientes", Modulo = "Pacientes", Activo = true },

            new() { Codigo = "CITA_VER", Nombre = "Ver citas", Modulo = "Citas", Activo = true },
            new() { Codigo = "CITA_PROGRAMAR", Nombre = "Programar citas", Modulo = "Citas", Activo = true },
            new() { Codigo = "CITA_REPROGRAMAR", Nombre = "Reprogramar citas", Modulo = "Citas", Activo = true },
            new() { Codigo = "CITA_CANCELAR", Nombre = "Cancelar citas", Modulo = "Citas", Activo = true },

            new() { Codigo = "ATENCION_VER", Nombre = "Ver atenciones", Modulo = "Atenciones", Activo = true },
            new() { Codigo = "ATENCION_REGISTRAR", Nombre = "Registrar atención", Modulo = "Atenciones", Activo = true },
            new() { Codigo = "ATENCION_CERRAR", Nombre = "Cerrar atención", Modulo = "Atenciones", Activo = true },

            new() { Codigo = "PAGO_VER", Nombre = "Ver pagos", Modulo = "Pagos", Activo = true },
            new() { Codigo = "PAGO_REGISTRAR", Nombre = "Registrar pago", Modulo = "Pagos", Activo = true },
            
            new() { Codigo = "FINANZAS_VER", Nombre = "Ver finanzas", Modulo = "Finanzas", Activo = true },
            new() { Codigo = "FINANZAS_EXPORTAR", Nombre = "Exportar reportes financieros", Modulo = "Finanzas", Activo = true },
            new() { Codigo = "FINANZAS_AJUSTAR", Nombre = "Registrar ajustes financieros", Modulo = "Finanzas", Activo = true },

            new() { Codigo = "DOCTOR_VER", Nombre = "Ver doctores", Modulo = "Doctores", Activo = true },
            new() { Codigo = "DOCTOR_CREAR", Nombre = "Crear doctores", Modulo = "Doctores", Activo = true },
            new() { Codigo = "DOCTOR_EDITAR", Nombre = "Editar doctores", Modulo = "Doctores", Activo = true },

            new() { Codigo = "HORARIO_VER", Nombre = "Ver horarios", Modulo = "Horarios", Activo = true },
            new() { Codigo = "HORARIO_CREAR", Nombre = "Crear horarios", Modulo = "Horarios", Activo = true },
            new() { Codigo = "HORARIO_EDITAR", Nombre = "Editar horarios", Modulo = "Horarios", Activo = true },

            new() { Codigo = "SERVICIO_VER", Nombre = "Ver servicios clínicos", Modulo = "Servicios Clínicos", Activo = true },

            new() { Codigo = "HISTORIAL_VER", Nombre = "Ver historial clínico", Modulo = "Historial Clínico", Activo = true },

            new() { Codigo = "USUARIO_VER", Nombre = "Ver usuarios", Modulo = "Usuarios", Activo = true },
            new() { Codigo = "USUARIO_CREAR", Nombre = "Crear usuarios", Modulo = "Usuarios", Activo = true },
            new() { Codigo = "USUARIO_EDITAR", Nombre = "Editar usuarios", Modulo = "Usuarios", Activo = true },
            new() { Codigo = "USUARIO_ASIGNAR_ROL", Nombre = "Asignar rol a usuario", Modulo = "Usuarios", Activo = true },

            new() { Codigo = "ROL_VER", Nombre = "Ver roles", Modulo = "Roles", Activo = true },
            new() { Codigo = "ROL_CREAR", Nombre = "Crear roles", Modulo = "Roles", Activo = true },
            new() { Codigo = "ROL_EDITAR", Nombre = "Editar roles", Modulo = "Roles", Activo = true },
            new() { Codigo = "ROL_ASIGNAR_PERMISOS", Nombre = "Asignar permisos a rol", Modulo = "Roles", Activo = true },

            new() { Codigo = "PERMISO_VER", Nombre = "Ver permisos", Modulo = "Permisos", Activo = true },

            new() { Codigo = "AUDITORIA_VER", Nombre = "Ver auditoría", Modulo = "Auditoría", Activo = true },
            
            new() { Codigo = "COMPROBANTE_VER", Nombre = "Ver comprobantes", Modulo = "Comprobantes", Activo = true },
            new() { Codigo = "COMPROBANTE_EMITIR", Nombre = "Emitir comprobantes", Modulo = "Comprobantes", Activo = true },
            new() { Codigo = "COMPROBANTE_ANULAR", Nombre = "Anular comprobantes", Modulo = "Comprobantes", Activo = true },
            new() { Codigo = "COMPROBANTE_IMPRIMIR", Nombre = "Imprimir comprobantes", Modulo = "Comprobantes", Activo = true },
            new() { Codigo = "HISTORIAL_IMPRIMIR", Nombre = "Imprimir historial clínico", Modulo = "Historial Clínico", Activo = true },
        };

        var codigosExistentes = await context.Permisos
            .Select(x => x.Codigo)
            .ToListAsync();

        var nuevosPermisos = permisosBase
            .Where(x => !codigosExistentes.Contains(x.Codigo))
            .ToList();

        if (nuevosPermisos.Count > 0)
            await context.Permisos.AddRangeAsync(nuevosPermisos);
    }

    private static async Task SeedRolesAsync(ApplicationDbContext context)
    {
        var adminRol = await context.Roles.FirstOrDefaultAsync(x => x.Nombre == "Administrador");

        if (adminRol == null)
        {
            adminRol = new Rol
            {
                Nombre = "Administrador",
                Descripcion = "Rol principal del sistema con acceso total.",
                EsSistema = true,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            };

            await context.Roles.AddAsync(adminRol);
            await context.SaveChangesAsync();
        }

        if (!await context.Roles.AnyAsync(x => x.Nombre == "Recepcionista"))
        {
            await context.Roles.AddAsync(new Rol
            {
                Nombre = "Recepcionista",
                Descripcion = "Gestiona pacientes y citas.",
                EsSistema = true,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            });
        }

        if (!await context.Roles.AnyAsync(x => x.Nombre == "Doctor"))
        {
            await context.Roles.AddAsync(new Rol
            {
                Nombre = "Doctor",
                Descripcion = "Gestiona atenciones médicas.",
                EsSistema = true,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            });
        }

        if (!await context.Roles.AnyAsync(x => x.Nombre == "Caja"))
        {
            await context.Roles.AddAsync(new Rol
            {
                Nombre = "Caja",
                Descripcion = "Gestiona pagos y cobros.",
                EsSistema = true,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync();

        var permisos = await context.Permisos.ToListAsync();

        var permisosAdminExistentes = await context.RolPermisos
            .Where(x => x.RolId == adminRol.Id)
            .Select(x => x.PermisoId)
            .ToListAsync();

        foreach (var permiso in permisos)
        {
            if (permisosAdminExistentes.Contains(permiso.Id)) continue;

            context.RolPermisos.Add(new RolPermiso
            {
                RolId = adminRol.Id,
                PermisoId = permiso.Id,
                FechaAsignacion = DateTime.UtcNow
            });
        }
    }

    private static async Task SeedUsuarioAdminAsync(ApplicationDbContext context)
    {
        var admin = await context.Usuarios.FirstOrDefaultAsync(x => x.UserName == "admin");

        if (admin == null)
        {
            admin = new Usuario
            {
                CodigoUsuario = $"USR-{DateTime.UtcNow:yyyy}-ADMIN",
                Nombres = "Administrador",
                Apellidos = "Sistema",
                UserName = "admin",
                Correo = "admin@clinica.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Estado = EstadoUsuario.Activo,
                FechaRegistro = DateTime.UtcNow
            };

            await context.Usuarios.AddAsync(admin);
            await context.SaveChangesAsync();
        }
        else if (!admin.PasswordHash.StartsWith("$2"))
        {
            admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123");
            context.Usuarios.Update(admin);
            await context.SaveChangesAsync();
        }

        var rolAdmin = await context.Roles.FirstAsync(x => x.Nombre == "Administrador");

        var tieneRolAdmin = await context.UsuarioRoles
            .AnyAsync(x => x.UsuarioId == admin.Id && x.RolId == rolAdmin.Id);

        if (!tieneRolAdmin)
        {
            context.UsuarioRoles.Add(new UsuarioRol
            {
                UsuarioId = admin.Id,
                RolId = rolAdmin.Id,
                FechaAsignacion = DateTime.UtcNow,
                Activo = true
            });
        }
    }

    private static async Task SeedServiciosClinicosAsync(ApplicationDbContext context)
    {
        if (await context.ServiciosClinicos.AnyAsync()) return;

        var servicios = new List<ServicioClinico>
        {
            new()
            {
                CodigoServicio = "ATEGEN",
                Nombre = "Atención genérica",
                Descripcion = "Servicio clínico base para registrar una atención general.",
                CostoBase = 50,
                DuracionMinutos = 30,
                RequiereCita = true,
                GeneraHistorial = true,
                Estado = EstadoServicioClinico.Activo
            },

            new()
            {
                CodigoServicio = "CONOBS",
                Nombre = "Consulta obstétrica",
                Descripcion = "Consulta médica orientada al control obstétrico.",
                CostoBase = 70,
                DuracionMinutos = 30,
                RequiereCita = true,
                GeneraHistorial = true,
                Estado = EstadoServicioClinico.Activo
            }
        };

        await context.ServiciosClinicos.AddRangeAsync(servicios);
    }
}