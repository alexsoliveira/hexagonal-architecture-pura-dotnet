namespace HexagonalLab.Core.Ports;

using HexagonalLab.Core.Models;

/// <summary>
/// Output Port: Repository abstraction for data persistence.
///
/// CRITICAL CHARACTERISTICS (Hexagonal Architecture):
/// ✅ Defined in Core (inside)
/// ✅ Implemented by Adapters (outside)
/// ✅ Core doesn't know HOW data is stored (DB, cache, file, API, etc.)
/// ✅ Pure interface - no implementation details
/// ✅ Contract, not concrete class
///
/// REFERENCE: Alistair Cockburn - "Ports are contracts between inside and outside"
/// 
/// This port allows:
/// - Keeping Core free of database/infrastructure knowledge
/// - Easily swapping storage implementations (In-Memory for tests, EF Core for prod, etc.)
/// - Testing Core in isolation without hitting database
/// </summary>
public interface IItemRepositoryPort
{
    /// <summary>
    /// Persists an item (CREATE or UPDATE).
    /// 
    /// Core doesn't care if:
    /// - It's SQL Server, PostgreSQL, MongoDB, etc.
    /// - It's EF Core, Dapper, raw SQL, etc.
    /// - It's a file, cache, HTTP API, etc.
    /// </summary>
    /// <param name="item">The item to persist</param>
    /// <returns>Completed task</returns>
    Task SaveAsync(Item item);

    /// <summary>
    /// Retrieves an item by ID.
    /// 
    /// Returns null if not found (like a database).
    /// </summary>
    /// <param name="itemId">The unique item identifier</param>
    /// <returns>The item or null if not found</returns>
    Task<Item?> GetByIdAsync(string itemId);

    /// <summary>
    /// Retrieves all items.
    /// </summary>
    /// <returns>Collection of all items</returns>
    Task<IEnumerable<Item>> GetAllAsync();

    /// <summary>
    /// Deletes an item by ID.
    /// Idempotent - doesn't fail if item doesn't exist.
    /// </summary>
    /// <param name="itemId">The unique item identifier</param>
    /// <returns>Completed task</returns>
    Task DeleteAsync(string itemId);

    /// <summary>
    /// Checks if an item exists.
    /// </summary>
    /// <param name="itemId">The unique item identifier</param>
    /// <returns>True if exists, false otherwise</returns>
    Task<bool> ExistsAsync(string itemId);
}
