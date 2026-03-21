using HexagonalLab.API.Endpoints;
using HexagonalLab.Core.Ports;
using HexagonalLab.Core.UseCases;
using HexagonalLab.Infrastructure.Data;
using HexagonalLab.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

// ========================================================================
// BOOTSTRAP - Dependency Injection Configuration
// ========================================================================
// NOTA: Este é o ÚNICO lugar onde DI é configurado!
// Se trocar adapter: apenas AQUI muda, nunca no Core.
// ========================================================================

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────────────────────────────
// 1. Add API Services
// ─────────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// ─────────────────────────────────────────────────────────────────
// 2. Register Core UseCases (Input Ports)
// ─────────────────────────────────────────────────────────────────
// Phase 3: Usando In-Memory adapter ainda
// Phase 4: Trocar para EF Core adapter (UMA LINHA SÓ!)
// ─────────────────────────────────────────────────────────────────

builder.Services.AddScoped<IItemInputPort, GetItemUseCase>();

// ─────────────────────────────────────────────────────────────────
// 3. Register Output Port Adapters
// ─────────────────────────────────────────────────────────────────
// FASE 3: In-Memory Adapter (teste)
// FASE 4: EF Core Adapter (real) ✅ AGORA!
// ─────────────────────────────────────────────────────────────────

// PHASE 4: Configure Database & EF Core Adapter
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IItemRepositoryPort, EfCoreRepositoryAdapter>();

// ─────────────────────────────────────────────────────────────────
// 4. Build App
// ─────────────────────────────────────────────────────────────────

var app = builder.Build();

// ─────────────────────────────────────────────────────────────────
// Database Initialization
// ─────────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();  // Apply pending migrations
}

if (app.Environment.IsDevelopment())
{
    // Swagger removed - use OpenAPI spec directly from endpoints
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();

// ─────────────────────────────────────────────────────────────────
// 5. Map Minimal API Endpoints
// ─────────────────────────────────────────────────────────────────

app.MapItemEndpoints();  // Registra endpoints

// ─────────────────────────────────────────────────────────────────

app.Run();
