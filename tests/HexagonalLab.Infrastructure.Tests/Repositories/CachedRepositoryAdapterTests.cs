namespace HexagonalLab.Infrastructure.Tests.Repositories;

using HexagonalLab.Core.Models;
using HexagonalLab.Core.Ports;
using HexagonalLab.Infrastructure.Repositories;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

/// <summary>
/// Tests: Cached Repository Adapter (Decorator)
/// 
/// Validar:
/// ✅ Caching funciona
/// ✅ Cache invalidation funciona
/// ✅ Comportamento idêntico ao adapter real
/// </summary>
public class CachedRepositoryAdapterTests
{
    /// <summary>
    /// Simple in-memory adapter for testing.
    /// </summary>
    private class SimpleInMemoryAdapter : IItemRepositoryPort
    {
        private readonly Dictionary<string, Item> _store = new();

        public Task SaveAsync(Item item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            _store[item.Id] = item;
            return Task.CompletedTask;
        }

        public Task<Item?> GetByIdAsync(string itemId)
        {
            var item = _store.TryGetValue(itemId, out var value) ? value : null;
            return Task.FromResult(item);
        }

        public Task<IEnumerable<Item>> GetAllAsync()
        {
            return Task.FromResult(_store.Values.AsEnumerable());
        }

        public Task DeleteAsync(string itemId)
        {
            _store.Remove(itemId);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string itemId)
        {
            return Task.FromResult(_store.ContainsKey(itemId));
        }
    }

    [Fact]
    public async Task GetByIdAsync_SecondCall_ReturnsCachedVersion()
    {
        // Arrange
        var innerAdapter = new SimpleInMemoryAdapter();
        var cache = new MemoryCache(new MemoryCacheOptions());
        var cachedAdapter = new CachedRepositoryAdapter(innerAdapter, cache);

        var item = new Item { Id = "ITEM-001", Name = "Test", Status = "Active" };
        await innerAdapter.SaveAsync(item);

        // Act - Primeira chamada (não cacheada)
        var result1 = await cachedAdapter.GetByIdAsync("ITEM-001");

        // Modificar no inner adapter (não deveria afetar cache)
        item.Name = "Modified";
        await innerAdapter.SaveAsync(item);

        // Segunda chamada (IS cacheada)
        var result2 = await cachedAdapter.GetByIdAsync("ITEM-001");

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.Equal("Test", result1.Name);
        Assert.Equal("Test", result2.Name);  // Ainda "Test" (cached!)
    }

    [Fact]
    public async Task SaveAsync_InvalidatesCache()
    {
        // Arrange
        var innerAdapter = new SimpleInMemoryAdapter();
        var cache = new MemoryCache(new MemoryCacheOptions());
        var cachedAdapter = new CachedRepositoryAdapter(innerAdapter, cache);

        var item = new Item { Id = "ITEM-001", Name = "Original", Status = "Active" };
        await cachedAdapter.SaveAsync(item);

        // Act - Buscar (cacheia)
        await cachedAdapter.GetByIdAsync("ITEM-001");

        // Modificar (cache deve ser invalidado)
        item.Name = "Updated";
        await cachedAdapter.SaveAsync(item);

        // Buscar novamente
        var result = await cachedAdapter.GetByIdAsync("ITEM-001");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated", result.Name);  // Deve trazer versão updated
    }

    [Fact]
    public async Task DeleteAsync_InvalidatesCache()
    {
        // Arrange
        var innerAdapter = new SimpleInMemoryAdapter();
        var cache = new MemoryCache(new MemoryCacheOptions());
        var cachedAdapter = new CachedRepositoryAdapter(innerAdapter, cache);

        var item = new Item { Id = "ITEM-001", Name = "Test", Status = "Active" };
        await cachedAdapter.SaveAsync(item);
        await cachedAdapter.GetByIdAsync("ITEM-001");  // Cacheia

        // Act
        await cachedAdapter.DeleteAsync("ITEM-001");

        // Assert
        var result = await cachedAdapter.GetByIdAsync("ITEM-001");
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_CachesResults()
    {
        // Arrange
        var innerAdapter = new SimpleInMemoryAdapter();
        var cache = new MemoryCache(new MemoryCacheOptions());
        var cachedAdapter = new CachedRepositoryAdapter(innerAdapter, cache);

        var item1 = new Item { Id = "ITEM-001", Name = "Item 1", Status = "Active" };
        var item2 = new Item { Id = "ITEM-002", Name = "Item 2", Status = "Active" };
        await cachedAdapter.SaveAsync(item1);
        await cachedAdapter.SaveAsync(item2);

        // Act - Primeira chamada
        var result1 = await cachedAdapter.GetAllAsync();
        var count1 = result1.Count();

        // Adicionar item ao adapter real (sem passar pelo cache)
        var item3 = new Item { Id = "ITEM-003", Name = "Item 3", Status = "Active" };
        await innerAdapter.SaveAsync(item3);

        // Segunda chamada (deve retornar cache)
        var result2 = await cachedAdapter.GetAllAsync();
        var count2 = result2.Count();

        // Assert
        Assert.Equal(2, count1);
        Assert.Equal(2, count2);  // Ainda 2 (cached!)
    }

    [Fact]
    public async Task DecoratorPattern_CoreRemains_Unchanged()
    {
        // Arrange
        var innerAdapter = new SimpleInMemoryAdapter();
        var cache = new MemoryCache(new MemoryCacheOptions());
        var cachedAdapter = new CachedRepositoryAdapter(innerAdapter, cache);

        var item = new Item { Id = "ITEM-001", Name = "Test", Status = "Active" };

        // Act
        await cachedAdapter.SaveAsync(item);
        var result = await cachedAdapter.GetByIdAsync("ITEM-001");

        // Assert
        // Comportamento idêntico: salvou, recuperou, tudo funciona
        Assert.NotNull(result);
        Assert.Equal("ITEM-001", result.Id);
        Assert.Equal("Test", result.Name);

        // PONTO CRÍTICO: UseCase nunca sabe de caching!
        // Poderia usar InMemoryRepositoryAdapter ou CachedRepositoryAdapter
        // Resultado idêntico!
    }

    [Fact]
    public async Task ExistsAsync_Works()
    {
        // Arrange
        var innerAdapter = new SimpleInMemoryAdapter();
        var cache = new MemoryCache(new MemoryCacheOptions());
        var cachedAdapter = new CachedRepositoryAdapter(innerAdapter, cache);

        var item = new Item { Id = "ITEM-001", Name = "Test", Status = "Active" };
        await cachedAdapter.SaveAsync(item);

        // Act
        var existsTrue = await cachedAdapter.ExistsAsync("ITEM-001");
        var existsFalse = await cachedAdapter.ExistsAsync("ITEM-999");

        // Assert
        Assert.True(existsTrue);
        Assert.False(existsFalse);
    }

    [Fact]
    public async Task SaveAsync_WithNullItem_ThrowsException()
    {
        // Arrange
        var innerAdapter = new SimpleInMemoryAdapter();
        var cache = new MemoryCache(new MemoryCacheOptions());
        var cachedAdapter = new CachedRepositoryAdapter(innerAdapter, cache);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => cachedAdapter.SaveAsync(null!)
        );
    }

    [Fact]
    public async Task GetByIdAsync_WithEmptyId_ThrowsException()
    {
        // Arrange
        var innerAdapter = new SimpleInMemoryAdapter();
        var cache = new MemoryCache(new MemoryCacheOptions());
        var cachedAdapter = new CachedRepositoryAdapter(innerAdapter, cache);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => cachedAdapter.GetByIdAsync(string.Empty)
        );
    }
}
