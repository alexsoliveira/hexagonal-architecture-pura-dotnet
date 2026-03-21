namespace HexagonalLab.Infrastructure.Data;

using HexagonalLab.Core.Models;
using HexagonalLab.Infrastructure.Data.EntityConfigurations;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// DbContext: Configuração de acesso a dados com EF Core.
/// 
/// CRÍTICO: Este arquivo:
/// ✅ Contém APENAS EF Core concerns
/// ✅ NUNCA vaza para Core
/// ✅ Adapter layer, não Core
/// ✅ Pode ser trocado por Dapper sem impacto
/// </summary>
public class AppDbContext : DbContext
{
    public DbSet<Item> Items { get; set; } = null!;

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplicar entity configurations
        modelBuilder.ApplyConfiguration(new ItemConfiguration());
    }
}
