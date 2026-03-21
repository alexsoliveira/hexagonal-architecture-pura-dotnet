namespace HexagonalLab.Core.UseCases;

using HexagonalLab.Core.Models;
using HexagonalLab.Core.Ports;

/// <summary>
/// New UseCase: Get an item by ID.
///
/// CRITICAL PATTERN (Output Port Usage):
/// ✅ Implements Input Port (receiving contract)
/// ✅ Depends on Output Port (sending contract) via constructor injection
/// ✅ Core doesn't know WHERE data comes from (DB, cache, etc.)
/// ✅ Only knows WHAT it needs (IItemRepositoryPort interface)
/// ✅ Proves Hexagonal pattern works (inside agnostic of outside)
///
/// ARCHITECTURE:
/// ```
/// GetItemUseCase (inside)
///     ↓ depends on
/// IItemRepositoryPort (interface, inside, boundary)
///     ↑ implemented by
/// InMemoryRepositoryAdapter (outside for tests)
/// EFRepositoryAdapter (outside for production)
/// ```
///
/// BENEFIT: Can swap adapters without touching UseCase code!
/// </summary>
public class GetItemUseCase : IItemInputPort
{
    private readonly IItemRepositoryPort _repository;

    /// <summary>
    /// Constructor: Injects Output Port (dependency).
    /// 
    /// The magic: _repository is an INTERFACe, not a concrete class.
    /// It can be InMemoryRepositoryAdapter, EF Core adapter, etc.
    /// Core doesn't care!
    /// </summary>
    public GetItemUseCase(IItemRepositoryPort repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>
    /// Implements Input Port: ProcessAsync.
    ///
    /// Flow:
    /// 1. Validate input
    /// 2. Ask repository (via Output Port) to GET item
    /// 3. Return response
    ///
    /// CRITICAL: Uses Output Port (doesn't know implementation)
    /// </summary>
    public async Task<ItemResponse> ProcessAsync(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
            throw new ArgumentException("Item ID cannot be empty", nameof(itemId));

        // ✅ CRUCIAL: Use Output Port (interface)
        // Repository could be:
        //  - InMemoryRepositoryAdapter (tests)
        //  - EFRepositoryAdapter (production)
        //  - HttpRepositoryAdapter (external API)
        //  - CacheRepositoryAdapter (caching layer)
        // Core DOESN'T KNOW AND DOESN'T CARE!
        var item = await _repository.GetByIdAsync(itemId);

        if (item == null)
        {
            return new ItemResponse
            {
                ItemId = itemId,
                Status = "Not Found",
                ProcessedAt = DateTime.UtcNow,
                Message = $"Item {itemId} not found in repository"
            };
        }

        return new ItemResponse
        {
            ItemId = item.Id,
            Status = item.Status,
            ProcessedAt = DateTime.UtcNow,
            Message = $"Item {item.Id} retrieved successfully from repository. Current status: {item.Status}"
        };
    }
}
