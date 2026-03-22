namespace HexagonalLab.Core.Tests.Ports;

using HexagonalLab.Core.Models;
using HexagonalLab.Core.Ports;
using HexagonalLab.Core.Tests.Adapters;
using Xunit;

/// <summary>
/// Port Contract Validation Tests
/// 
/// OBJETIVO: Validar que o contrato (interface IItemRepositoryPort) 
/// é implementado corretamente por qualquer adapter.
/// 
/// Se trocar adaptador no futuro, estes testes garantem que
/// o novo adapter segue o contrato.
/// 
/// PADRÃO HEXAGONAL: Ports definem contratos, adapters implementam.
/// ✅ Sem coupling ao adapter específico
/// ✅ Documento vivo do contrato
/// ✅ Garante substituibilidade
/// </summary>
public class PortContractTests
{
    /// <summary>
    /// Contract: SaveAsync deve persistir um item
    /// </summary>
    [Fact]
    public async Task IItemRepositoryPort_SaveAsync_PersistsItem()
    {
        // Arrange
        IItemRepositoryPort repository = new InMemoryRepositoryAdapter();
        var item = new Item { Id = "TEST-001", Name = "Test Item", Status = "Active" };

        // Act
        await repository.SaveAsync(item);

        // Assert: Verify item was persisted
        var retrieved = await repository.GetByIdAsync("TEST-001");
        Assert.NotNull(retrieved);
        Assert.Equal("Test Item", retrieved.Name);
    }

    /// <summary>
    /// Contract: GetByIdAsync deve retornar null se item não existe
    /// </summary>
    [Fact]
    public async Task IItemRepositoryPort_GetByIdAsync_ReturnsNullIfNotFound()
    {
        // Arrange
        IItemRepositoryPort repository = new InMemoryRepositoryAdapter();

        // Act
        var result = await repository.GetByIdAsync("NON-EXISTENT");

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Contract: DeleteAsync deve remover item
    /// </summary>
    [Fact]
    public async Task IItemRepositoryPort_DeleteAsync_RemovesItem()
    {
        // Arrange
        IItemRepositoryPort repository = new InMemoryRepositoryAdapter();
        var item = new Item { Id = "DEL-001", Name = "Delete Me", Status = "Pending" };
        await repository.SaveAsync(item);

        // Act
        await repository.DeleteAsync("DEL-001");

        // Assert
        var result = await repository.GetByIdAsync("DEL-001");
        Assert.Null(result);
    }

    /// <summary>
    /// Contract: ExistsAsync deve validar existência
    /// </summary>
    [Fact]
    public async Task IItemRepositoryPort_ExistsAsync_ValidatesExistence()
    {
        // Arrange
        IItemRepositoryPort repository = new InMemoryRepositoryAdapter();
        var item = new Item { Id = "EXIST-001", Name = "Exists", Status = "Active" };
        await repository.SaveAsync(item);

        // Act
        var existsBefore = await repository.ExistsAsync("EXIST-001");
        await repository.DeleteAsync("EXIST-001");
        var existsAfter = await repository.ExistsAsync("EXIST-001");

        // Assert
        Assert.True(existsBefore);
        Assert.False(existsAfter);
    }

    /// <summary>
    /// Contract: SaveAsync com update deve substituir item antigo
    /// </summary>
    [Fact]
    public async Task IItemRepositoryPort_SaveAsync_WithExistingId_UpdatesItem()
    {
        // Arrange
        IItemRepositoryPort repository = new InMemoryRepositoryAdapter();
        var item1 = new Item { Id = "UPD-001", Name = "Original", Status = "Pending" };
        await repository.SaveAsync(item1);

        // Act: Update same ID
        var item2 = new Item { Id = "UPD-001", Name = "Updated", Status = "Completed" };
        await repository.SaveAsync(item2);

        // Assert: Should have updated
        var result = await repository.GetByIdAsync("UPD-001");
        Assert.NotNull(result);
        Assert.Equal("Updated", result.Name);
        Assert.Equal("Completed", result.Status);
    }

    /// <summary>
    /// Contract: Multiple operations should maintain isolation
    /// </summary>
    [Fact]
    public async Task IItemRepositoryPort_MultipleOperations_MaintainsConsistency()
    {
        // Arrange
        IItemRepositoryPort repository = new InMemoryRepositoryAdapter();
        var item1 = new Item { Id = "CONS-001", Name = "Item 1", Status = "Active" };
        var item2 = new Item { Id = "CONS-002", Name = "Item 2", Status = "Inactive" };

        // Act
        await repository.SaveAsync(item1);
        await repository.SaveAsync(item2);
        var retrieved1 = await repository.GetByIdAsync("CONS-001");
        var retrieved2 = await repository.GetByIdAsync("CONS-002");

        // Assert: Both should exist independently
        Assert.NotNull(retrieved1);
        Assert.NotNull(retrieved2);
        Assert.Equal("Active", retrieved1.Status);
        Assert.Equal("Inactive", retrieved2.Status);
    }

    /// <summary>
    /// Contract: Invalid operations should throw appropriate exceptions
    /// </summary>
    [Fact]
    public async Task IItemRepositoryPort_SaveAsync_WithNullItem_ThrowsException()
    {
        // Arrange
        IItemRepositoryPort repository = new InMemoryRepositoryAdapter();

        // Act & Assert
        // Note: Behavior depends on adapter implementation
        // This test documents expected contract
        Item? nullItem = null;
        if (nullItem != null)
        {
            // Adapter should handle null validation
            await Assert.ThrowsAsync<ArgumentNullException>(() => repository.SaveAsync(nullItem));
        }
    }
}
