namespace HexagonalLab.Infrastructure.Tests.Repositories;

using HexagonalLab.Core.Models;
using HexagonalLab.Core.Ports;
using HexagonalLab.Infrastructure.Data;
using HexagonalLab.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

/// <summary>
/// Integration Tests: EF Core Repository Adapter
/// 
/// PONTO CRÍTICO:
/// ✅ Testa adapter com BANCO REAL (EF Core In-Memory)
/// ✅ Valida: queries, updates, deletes funcionam
/// ✅ Testa interactions com EF Core
/// ✅ MAS Core não é testado (apenas adapter)
/// 
/// PADRÃO HEXAGONAL: Este teste valida adapter específico,
/// NÃO o contrato (aquele está em Ports/PortContractTests.cs)
/// </summary>
public class EfCoreRepositoryAdapterTests
{
    /// <summary>
    /// Helper: Cria DbContext em-memória para testes
    /// </summary>
    private AppDbContext CreateTestContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"HexagonalLabTest_{Guid.NewGuid()}")
            .Options;

        return new AppDbContext(options);
    }

    /// <summary>
    /// Integration: SaveAsync deve persistir item no banco
    /// </summary>
    [Fact]
    public async Task SaveAsync_WithNewItem_InsertsIntoDatabase()
    {
        // Arrange
        using var context = CreateTestContext();
        var adapter = new EfCoreRepositoryAdapter(context);
        var item = new Item { Id = "EF-001", Name = "EF Core Test", Status = "Active" };

        // Act
        await adapter.SaveAsync(item);

        // Assert: Verify persisted in database
        var retrieved = await adapter.GetByIdAsync("EF-001");
        Assert.NotNull(retrieved);
        Assert.Equal("EF Core Test", retrieved.Name);
        Assert.Equal("Active", retrieved.Status);
    }

    /// <summary>
    /// Integration: SaveAsync com ID existente deve fazer UPDATE
    /// </summary>
    [Fact]
    public async Task SaveAsync_WithExistingItem_Updates()
    {
        // Arrange
        using var context = CreateTestContext();
        var adapter = new EfCoreRepositoryAdapter(context);
        var item1 = new Item { Id = "EF-UPD", Name = "Version 1", Status = "Pending" };
        await adapter.SaveAsync(item1);

        // Act: Update same item
        var item2 = new Item { Id = "EF-UPD", Name = "Version 2", Status = "Completed" };
        await adapter.SaveAsync(item2);

        // Assert
        var updated = await adapter.GetByIdAsync("EF-UPD");
        Assert.NotNull(updated);
        Assert.Equal("Version 2", updated.Name);
        Assert.Equal("Completed", updated.Status);
    }

    /// <summary>
    /// Integration: GetByIdAsync retorna null se não existe
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        using var context = CreateTestContext();
        var adapter = new EfCoreRepositoryAdapter(context);

        // Act
        var result = await adapter.GetByIdAsync("NON-EXISTENT");

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Integration: DeleteAsync remove item do banco
    /// </summary>
    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        // Arrange
        using var context = CreateTestContext();
        var adapter = new EfCoreRepositoryAdapter(context);
        var item = new Item { Id = "EF-DEL", Name = "Delete Me", Status = "Active" };
        await adapter.SaveAsync(item);

        // Act
        await adapter.DeleteAsync("EF-DEL");

        // Assert
        var result = await adapter.GetByIdAsync("EF-DEL");
        Assert.Null(result);
    }

    /// <summary>
    /// Integration: ExistsAsync valida presença no banco
    /// </summary>
    [Fact]
    public async Task ExistsAsync_ChecksItemPresence()
    {
        // Arrange
        using var context = CreateTestContext();
        var adapter = new EfCoreRepositoryAdapter(context);
        var item = new Item { Id = "EF-CHECK", Name = "Check", Status = "Active" };
        await adapter.SaveAsync(item);

        // Act
        var existsBefore = await adapter.ExistsAsync("EF-CHECK");
        await adapter.DeleteAsync("EF-CHECK");
        var existsAfter = await adapter.ExistsAsync("EF-CHECK");

        // Assert
        Assert.True(existsBefore);
        Assert.False(existsAfter);
    }

    /// <summary>
    /// Integration: Múltiplos itens mantêm isolamento
    /// </summary>
    [Fact]
    public async Task SaveAsync_WithMultipleItems_MaintainsIsolation()
    {
        // Arrange
        using var context = CreateTestContext();
        var adapter = new EfCoreRepositoryAdapter(context);
        var items = new[]
        {
            new Item { Id = "EF-M1", Name = "Item 1", Status = "Active" },
            new Item { Id = "EF-M2", Name = "Item 2", Status = "Inactive" },
            new Item { Id = "EF-M3", Name = "Item 3", Status = "Pending" }
        };

        // Act
        foreach (var item in items)
            await adapter.SaveAsync(item);

        // Assert
        var r1 = await adapter.GetByIdAsync("EF-M1");
        var r2 = await adapter.GetByIdAsync("EF-M2");
        var r3 = await adapter.GetByIdAsync("EF-M3");

        Assert.NotNull(r1);
        Assert.NotNull(r2);
        Assert.NotNull(r3);
        Assert.Equal("Active", r1.Status);
        Assert.Equal("Inactive", r2.Status);
        Assert.Equal("Pending", r3.Status);
    }

    /// <summary>
    /// Integration: Adapter implementa IItemRepositoryPort corretamente
    /// </summary>
    [Fact]
    public void EfCoreRepositoryAdapter_ImplementsIItemRepositoryPort()
    {
        // Arrange & Act
        using var context = CreateTestContext();
        var adapter = new EfCoreRepositoryAdapter(context);

        // Assert
        Assert.IsAssignableFrom<IItemRepositoryPort>(adapter);
    }

    /// <summary>
    /// Integration: Transações são isoladas por DbContext
    /// </summary>
    [Fact]
    public async Task SaveAsync_WithMultipleContexts_MaintainsIsolation()
    {
        // Note: Este teste mostra que cada context tem seu próprio estado
        // Em produção, contexts compartilham database real

        // Arrange: Dois contexts diferentes
        using var context1 = CreateTestContext();
        using var context2 = CreateTestContext();

        var adapter1 = new EfCoreRepositoryAdapter(context1);
        var adapter2 = new EfCoreRepositoryAdapter(context2);

        var item = new Item { Id = "EF-TX", Name = "Transaction Test", Status = "Active" };

        // Act: Save em context1
        await adapter1.SaveAsync(item);

        // Assert: context2 não tem acesso (in-memory isolation por DB name)
        var result2 = await adapter2.GetByIdAsync("EF-TX");
        Assert.Null(result2); // Null porque são DBs diferentes
    }
}
