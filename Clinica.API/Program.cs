using System.Text;
using Clinica.API.Authorization;
using Clinica.API.Configurations;
using Clinica.API.Filters;
using Clinica.API.Helpers;
using Clinica.API.Middlewares;
using Clinica.API.Services;
using Clinica.API.Services.Imp;
using Clinica.Domain.Interfaces;
using Clinica.Infrastructure.Data;
using Clinica.Infrastructure.Data.Seeds;
using Clinica.Infrastructure.Documents.Comprobantes.Services;
using Clinica.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuestPDF.Infrastructure;
using Clinica.API.Services.Background;

var builder = WebApplication.CreateBuilder(args);

// ==========================================================
// CONFIGURACIÓN GENERAL
// ==========================================================
builder.Services.AddScoped<AuditoriaAutomaticaFilter>();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<AuditoriaAutomaticaFilter>();
});

builder.Services.Configure<ApiBehaviorOptions>(
    ValidationResponseConfig.ConfigurarRespuestasDeValidacion
);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "Clínica Santa Mónica API",
        Version = "v1",
        Description = "API del sistema de gestión clínica SIGEC."
    });

    // Incluir los comentarios XML de tus controladores
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<JwtHelper>();
builder.Services.AddScoped<IUsuarioActualService, UsuarioActualService>();

// ==========================================================
// AUTENTICACIÓN JWT
// ==========================================================
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            ),

            ClockSkew = TimeSpan.Zero
        };
    });

// ==========================================================
// AUTORIZACIÓN POR PERMISOS
// ==========================================================
builder.Services.AddAuthorization(options =>
{
    foreach (var permiso in PermisosPolicies.Todos)
    {
        options.AddPolicy(permiso, policy =>
            policy.RequireClaim("permiso", permiso));
    }
});

// ==========================================================
// BASE DE DATOS
// ==========================================================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ==========================================================
// REPOSITORIO GENÉRICO
// ==========================================================
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// ==========================================================
// REPOSITORIOS
// ==========================================================
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IRolRepository, RolRepository>();
builder.Services.AddScoped<IPermisoRepository, PermisoRepository>();
builder.Services.AddScoped<IAuditoriaRepository, AuditoriaRepository>();

builder.Services.AddScoped<IPacienteRepository, PacienteRepository>();
builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
builder.Services.AddScoped<IHorarioDoctorRepository, HorarioDoctorRepository>();
builder.Services.AddScoped<ICitaRepository, CitaRepository>();

builder.Services.AddScoped<IServicioClinicoRepository, ServicioClinicoRepository>();
builder.Services.AddScoped<IHistorialClinicoRepository, HistorialClinicoRepository>();
builder.Services.AddScoped<IHistorialDetalleRepository, HistorialDetalleRepository>();
builder.Services.AddScoped<IAtencionRepository, AtencionRepository>();

builder.Services.AddScoped<IPagoRepository, PagoRepository>();
builder.Services.AddScoped<IAjusteFinancieroRepository, AjusteFinancieroRepository>();

builder.Services.AddScoped<IComprobanteRepository, ComprobanteRepository>();
builder.Services.AddScoped<INotificacionCitaRepository, NotificacionCitaRepository>();

// ==========================================================
// SERVICIOS
// ==========================================================
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IRolService, RolService>();
builder.Services.AddScoped<IPermisoService, PermisoService>();
builder.Services.AddScoped<IAuditoriaService, AuditoriaService>();

builder.Services.AddScoped<IPacienteService, PacienteService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IHorarioDoctorService, HorarioDoctorService>();
builder.Services.AddScoped<ICitaService, CitaService>();

builder.Services.AddScoped<IServicioClinicoService, ServicioClinicoService>();
builder.Services.AddScoped<IHistorialClinicoService, HistorialClinicoService>();
builder.Services.AddScoped<IAtencionService, AtencionService>();

builder.Services.AddScoped<IPagoService, PagoService>();
builder.Services.AddScoped<IFinanzasService, FinanzasService>();

builder.Services.AddScoped<IComprobanteService, ComprobanteService>();
builder.Services.AddScoped<IComprobantePdfService, ComprobantePdfService>();

builder.Services.Configure<WhatsAppOptions>(builder.Configuration.GetSection("WhatsApp"));

builder.Services.AddHttpClient<INotificacionWhatsAppService, EvolutionWhatsAppService>((serviceProvider, client) =>
{
    var options = serviceProvider
        .GetRequiredService<Microsoft.Extensions.Options.IOptions<WhatsAppOptions>>()
        .Value;

    client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHostedService<RecordatorioCitasBackgroundService>();

// ==========================================================
// CORS
// ==========================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirBlazor", policy =>
    {
        policy.WithOrigins(
                "https://localhost:7299",
                "http://localhost:5091",
                "https://" // Reemplaza con tu URL real de Azure Static Web Apps cuando se cree
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// 1. En la sección de servicios (Antes del builder.Build()):
builder.Services.AddSignalR();

// ==========================================================
// QUESTPDF
// ==========================================================
QuestPDF.Settings.License = LicenseType.Community;

// ==========================================================
// PIPELINE HTTP
// ==========================================================
var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

// ─── MITIGACIÓN DE ALERTAS OWASP ZAP (MIDDLEWARE DE CABECERAS DE SEGURIDAD) ───
app.Use(async (context, next) =>
{
    // 1. Solución Anti-Clickjacking
    context.Response.Headers.Append("X-Frame-Options", "DENY");

    // 2. Solución X-Content-Type-Options (MIME Sniffing)
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

    // 3. Configuración base de Content Security Policy (CSP)
    context.Response.Headers.Append("Content-Security-Policy", 
        "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; frame-ancestors 'none';");

    await next();
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => 
    {
        //https://localhost:7241/swagger/index.html
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Clinica API v1");
    });
}
else
{
    // 4. Solución Strict-Transport-Security (HSTS) para producción
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseCors("PermitirBlazor");

app.UseAuthentication();

app.UseAuthorization();

// 2. En la sección del Pipeline HTTP (Abajo de app.MapControllers()):
app.MapHub<Clinica.API.Hubs.ChatHub>("/chathub");

app.MapControllers();

// ==========================================================
// SEEDER
// ==========================================================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await DataSeeder.SeedAsync(context);
}

app.Run(); // NOSONAR Password=root123456

public partial class Program
{
}