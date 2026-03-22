namespace HexagonalLab.API.Tests.Fixtures;

using HexagonalLab.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Custom WebApplicationFactory para E2E tests.
/// Configura API com EF Core usando SQLite em memória para testes.
/// 
/// FIX: Sets test environment BEFORE host is created, then overrides database provider
/// MUDANÇA Phase 4:
/// - Antes: In-Memory adapter fake
/// - Agora: Real EF Core adapter com SQLite (testa comportamento real)
/// </summary>
public class ItemApiWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set test environment so Program.cs can detect it
        builder.UseEnvironment("Test");

        builder.ConfigureServices(services =>
        {
            // Remove ALL DbContext and DbContextOptions registrations to prevent provider conflicts
            var descriptorsToRemove = services
                .Where(d => 
                    d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                    d.ServiceType == typeof(AppDbContext) ||
                    (d.ServiceType?.IsGenericType == true && 
                     d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>)))
                .ToList();

            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            // Adicionar DbContext com SQLite em memória para testes APENAS
            // This prevents SqlServer provider from being registered
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite("Data Source=:memory:"), 
                ServiceLifetime.Scoped);
        });

        builder.UseContentRoot(Directory.GetCurrentDirectory());
    }

    /// <summary>
    /// Inicializa o banco de dados em memória com migrations.
    /// Chamado antes de cada teste.
    /// </summary>
    public async Task InitializeDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.EnsureCreatedAsync();
    }
}

