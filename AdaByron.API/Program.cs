using AdaByron.API.Hubs;
using AdaByron.API.Middleware;
using AdaByron.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ── Infraestructura (EF Core + PostGIS + repositorios) ───────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");

builder.Services.AddInfrastructure(connectionString);

// ── API + Swagger ─────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Ada Byron API", Version = "v1" });
});

// ── SignalR ───────────────────────────────────────────────────────────────────
builder.Services.AddSignalR();

// ── CORS (desarrollo — Vite dev server) ───────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());       // Necesario para SignalR
});

// ════════════════════════════════════════════════════════════════════════════════

var app = builder.Build();

// ── Middleware ────────────────────────────────────────────────────────────────
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthorization();

// ── Endpoints ─────────────────────────────────────────────────────────────────
app.MapControllers();
app.MapHub<ReservasHub>("/hubs/reservations");

app.Run();
