using HexagonalLab.API.Endpoints;
using HexagonalLab.Core.Ports;
using HexagonalLab.Core.UseCases;
using HexagonalLab.Infrastructure.Data;
using HexagonalLab.Infrastructure.Repositories;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OpenApi;
using Scalar.AspNetCore;

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
builder.Services.AddOpenApi();
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
builder.Services.AddScoped<IGetAllItemsInputPort, GetAllItemsUseCase>();

// ─────────────────────────────────────────────────────────────────
// 3. Register Output Port Adapters
// ─────────────────────────────────────────────────────────────────
// FASE 3: In-Memory Adapter (teste)
// FASE 4: EF Core Adapter (real) ✅ AGORA!
// ─────────────────────────────────────────────────────────────────

// PHASE 4: Configure Database & EF Core Adapter
// NOTE: WebApplicationFactory for tests will override this via ConfigureWebHost
// Only register SqlServer if NOT in Test environment (to avoid provider conflicts)
if (!builder.Environment.IsEnvironment("Test"))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            sqlOptions =>
            {
                // ✅ Retry strategy for transient SQL Server errors
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5
                );
                
                // ✅ Increase command timeout from default 30s to 300s (5 minutes)
                sqlOptions.CommandTimeout(300);
            }
        );
    });
}

// PHASE 7: Add Memory Cache + Decorator Pattern
builder.Services.AddMemoryCache();

// Register base adapter (EF Core)
builder.Services.AddScoped<EfCoreRepositoryAdapter>();

// Register with Decorator (Cached wrapper)
// PADRÃO CRÍTICO: Mesmo que DI mude, Core não muda!
// 🔧 TEMPORARILY DISABLED FOR TESTING - uncomment to enable cache
// builder.Services.AddScoped<IItemRepositoryPort>(serviceProvider =>
//     new CachedRepositoryAdapter(
//         serviceProvider.GetRequiredService<EfCoreRepositoryAdapter>(),
//         serviceProvider.GetRequiredService<IMemoryCache>(),
//         TimeSpan.FromMinutes(5)  // Cache duration
//     )
// );

// 🔧 DIRECT ADAPTER (no cache) - for immediate testing
builder.Services.AddScoped<IItemRepositoryPort, EfCoreRepositoryAdapter>();

// ─────────────────────────────────────────────────────────────────
// 4. Build App
// ─────────────────────────────────────────────────────────────────

var app = builder.Build();

// ─────────────────────────────────────────────────────────────────
// Database Initialization
// ─────────────────────────────────────────────────────────────────
// Only initialize if NOT in Test environment (tests handle DB init separately)
if (!app.Environment.IsEnvironment("Test"))
{
    try
    {
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await context.Database.MigrateAsync();  // Apply pending migrations (SQL Server)
        }
    }
    catch (Exception ex)
    {
        // Log warning but don't crash the app - allows testing Scalar without database
        Console.WriteLine($"⚠️ Database initialization failed: {ex.Message}");
        Console.WriteLine("ℹ️ Application will continue without database (useful for testing API docs)");
    }
}

// ─────────────────────────────────────────────────────────────────
// Middleware Pipeline
// ─────────────────────────────────────────────────────────────────

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapStaticAssets();

// ─────────────────────────────────────────────────────────────────
// OpenAPI & Scalar Configuration (Development only)
// MUST come AFTER UseRouting() and UseAuthorization()
// Reference: https://learn.microsoft.com/pt-br/aspnet/core/fundamentals/openapi/using-openapi-documents?view=aspnetcore-10.0
// ─────────────────────────────────────────────────────────────────

if (app.Environment.IsDevelopment())
{
    // Enable OpenAPI endpoint at /openapi/v1.json
    app.MapOpenApi().AllowAnonymous();
}

// ─────────────────────────────────────────────────────────────────
// 5. Map Minimal API Endpoints
// ─────────────────────────────────────────────────────────────────

app.MapItemEndpoints();  // Registra endpoints

if (app.Environment.IsDevelopment())
{
    // Enable Scalar UI at /scalar
    // (Must come AFTER item endpoints to ensure proper routing)
    app.MapScalarApiReference().AllowAnonymous();
}

// ─────────────────────────────────────────────────────────────────

app.Run();
