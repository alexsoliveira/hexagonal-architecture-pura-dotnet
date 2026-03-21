namespace HexagonalLab.Core.Tests.Adapters;

using HexagonalLab.Core.Models;
using HexagonalLab.Core.Ports;

/// <summary>
/// In-Memory Repository Adapter (Fake/Test Adapter).
///
/// PURPOSE:
/// ✅ Implements IItemRepositoryPort interface
/// ✅ Stores data in memory (no database)
/// ✅ Simulates real repository behavior
/// ✅ Perfect for unit tests - no external dependencies
/// ✅ Proves Output Port pattern works
///
/// CHARACTERISTICS:
/// - Stores items in Dictionary (RAM)
/// - Simulates async operations (returns Task)
/// - Handles null/not-found scenarios like real DB
/// - Can be reset/seeded for tests
///
/// USAGE: Only in tests. For production, implement with EF Core.
///
/// REFERENCE: Alistair Cockburn - "Fake Adapter for testing"
/// </summary>
public class InMemoryRepositoryAdapter : IItemRepositoryPort
{
    private readonly Dictionary<string, Item> _store = new();

    /// <summary>
    /// Persists an item to memory.
    /// Simulates database INSERT or UPDATE.
    /// </summary>
    public Task SaveAsync(Item item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        if (string.IsNullOrWhiteSpace(item.Id))
            throw new ArgumentException("Item.Id cannot be empty", nameof(item.Id));

        _store[item.Id] = item;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Retrieves item by ID from memory.
    /// Returns null if not found (like SELECT * WHERE id = X that returns no rows).
    /// </summary>
    public Task<Item?> GetByIdAsync(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
            throw new ArgumentException("Item ID cannot be empty", nameof(itemId));

        var item = _store.TryGetValue(itemId, out var value) ? value : null;
        return Task.FromResult(item);
    }

    /// <summary>
    /// Retrieves all items from memory.
    /// Simulates SELECT * FROM items.
    /// </summary>
    public Task<IEnumerable<Item>> GetAllAsync()
    {
        var items = _store.Values.AsEnumerable();
        return Task.FromResult(items);
    }

    /// <summary>
    /// Deletes item from memory.
    /// Idempotent - doesn't fail if item doesn't exist.
    /// </summary>
    public Task DeleteAsync(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
            throw new ArgumentException("Item ID cannot be empty", nameof(itemId));

        _store.Remove(itemId);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Checks if item exists in memory.
    /// Simulates COUNT(*) or EXISTS query.
    /// </summary>
    public Task<bool> ExistsAsync(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
            throw new ArgumentException("Item ID cannot be empty", nameof(itemId));

        var exists = _store.ContainsKey(itemId);
        return Task.FromResult(exists);
    }

    /// <summary>
    /// TEST HELPER: Seed initial data for testing.
    /// </summary>
    public async Task SeedAsync(params Item[] items)
    {
        foreach (var item in items)
            await SaveAsync(item);
    }

    /// <summary>
    /// TEST HELPER: Clear all data between tests.
    /// </summary>
    public void Clear()
    {
        _store.Clear();
    }

    /// <summary>
    /// TEST HELPER: Get internal store count (for assertions).
    /// </summary>
    public int Count => _store.Count;
}
