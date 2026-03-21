namespace HexagonalLab.API.Tests.Fixtures;

using HexagonalLab.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Custom WebApplicationFactory para E2E tests.
/// Configura API com EF Core usando SQLite em memória para testes.
/// 
/// MUDANÇA Phase 4:
/// - Antes: In-Memory adapter fake
/// - Agora: Real EF Core adapter com SQLite (testa comportamento real)
/// </summary>
public class ItemApiWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove o AppDbContext real (SQL Server)
            var dbContextDescriptor = services.FirstOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (dbContextDescriptor != null)
                services.Remove(dbContextDescriptor);

            // Adicionar DbContext com SQLite em memória para testes
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite("Data Source=:memory:"));
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

