using System.Text;
using AdaByron.API.Hubs;
using AdaByron.Infrastructure.Realtime;
using AdaByron.API.Middleware;
using AdaByron.Application;
using AdaByron.Infrastructure;
using AdaByron.Infrastructure.Persistence.DbContext;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ── Infraestructura (EF Core + PostGIS + repositorios + TokenService) ─────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");

builder.Services.AddInfrastructure(connectionString);

// ── Casos de uso (Application) ────────────────────────────────────────────────
builder.Services.AddApplication();

// ── API + Swagger ─────────────────────────────────────────────────────────────
<<<<<<< HEAD
builder.Services.AddControllers().AddJsonOptions(options => 
{
    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});
=======
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

>>>>>>> 2b5c96daeac49700c41e7e0ebb7091bac2fbad55
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Ada Byron API", Version = "v1", Description = "API REST de reservas de los espacios del Edificio Ada Byron." });
    
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// ── Autenticación JWT ─────────────────────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("La clave JWT no está configurada.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        };
    });

builder.Services.AddAuthorization();

// ── SignalR (configuración de identificador de usuario) ──────────────────────────
builder.Services.AddSignalR();

// Re-configuración para que ASP.NET identifique el Claim de Email como el identificador de usuario en los Hubs
builder.Services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters.NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress";
});

// ── CORS (desarrollo — Vite dev server) ───────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

// ═════════════════════════════════════════════════════════════════════════════

var app = builder.Build();

// ── Middleware ────────────────────────────────────────────────────────────────
app.UseMiddleware<ExceptionHandlingMiddleware>();

// HABILITADO PARA TODOS LOS ENTORNOS (incluyendo Production) para el prototipo.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ada Byron API v1");
    // Al dejar RoutePrefix vacío, SwaggerUI abre en la raíz (http://localhost:5000/)
    c.RoutePrefix = string.Empty; 
});

app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthentication();   // ← Antes de Authorization
app.UseAuthorization();

// ── Endpoints ─────────────────────────────────────────────────────────────────
app.MapControllers();
app.MapHub<ReservasHub>("/hubs/reservations");
app.MapHub<NotificationHub>("/hubs/notifications");

// ── Test de Conexión a Base de Datos (PostGIS) al Iniciar ─────────────────────
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<AplicacionDbContext>();
        logger.LogInformation("Verificando conexión a la base de datos PostGIS en {Host}...", connectionString.Split(';').FirstOrDefault(x => x.StartsWith("Host")));
        
        // Aplica migraciones pendientes y verifica conectividad
        context.Database.Migrate();
        
        logger.LogInformation("✅ ¡Conexión exitosa! La base de datos está lista y actualizada.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ ERROR FATAL: No se pudo conectar a la base de datos PostGIS. Verifica que el contenedor 'adabyron_db' esté corriendo en el puerto 5433.");
        // En entornos de producción podrías querer detener la ejecución, 
        // pero en desarrollo dejamos que siga para ver los errores en la consola.
    }
}

app.Run();
