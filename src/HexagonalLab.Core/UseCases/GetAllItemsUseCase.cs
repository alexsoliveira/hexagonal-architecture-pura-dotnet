namespace HexagonalLab.Core.UseCases;

using HexagonalLab.Core.Models;
using HexagonalLab.Core.Ports;

/// <summary>
/// UseCase: Retrieve all items from repository.
/// 
/// HEXAGONAL PATTERN IN ACTION:
/// ✅ Implements Input Port (IGetAllItemsInputPort)
/// ✅ Depends on Output Port (IItemRepositoryPort)
/// ✅ Core is agnostic of repository implementation
/// ✅ Can be used by any Input Adapter (API, Worker, CLI, gRPC, etc.)
/// 
/// EXAMPLE DAG:
/// ```
/// API Endpoint
///     ↓ uses
/// GetAllItemsUseCase (Input Port implementation)
///     ↓ depends on
/// IItemRepositoryPort (interface)
///     ↑ implemented by
/// EfCoreRepositoryAdapter (real database)
/// InMemoryRepositoryAdapter (tests)
/// ```
/// </summary>
public class GetAllItemsUseCase : IGetAllItemsInputPort
{
    private readonly IItemRepositoryPort _repository;

    /// <summary>
    /// Constructor: Injects the Output Port dependency.
    /// 
    /// The repository is an interface, so it can be:
    /// - EfCoreRepositoryAdapter (production)
    /// - InMemoryRepositoryAdapter (tests)
    /// - CachedRepositoryAdapter (caching layer)
    /// </summary>
    public GetAllItemsUseCase(IItemRepositoryPort repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>
    /// Retrieves all items from the repository.
    /// 
    /// PURE BUSINESS LOGIC:
    /// - Just delegates to the repository (Output Port)
    /// - No HTTP, no EF Core, no SQL - all hidden by the port
    /// </summary>
    public async Task<IEnumerable<Item>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }
}
