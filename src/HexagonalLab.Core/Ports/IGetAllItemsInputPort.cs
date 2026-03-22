namespace HexagonalLab.Core.Ports;

using HexagonalLab.Core.Models;

/// <summary>
/// Input Port: Retrieve all items.
/// 
/// HEXAGONAL PATTERN:
/// ✅ Defined in Core (boundary)
/// ✅ Implemented by UseCase (GetAllItemsUseCase)
/// ✅ Used by Input Adapters (API, Worker, CLI, etc.)
/// 
/// Separate from IItemInputPort to maintain Single Responsibility:
/// - IItemInputPort: Process a single item
/// - IGetAllItemsInputPort: Retrieve all items
/// </summary>
public interface IGetAllItemsInputPort
{
    /// <summary>
    /// Retrieves all items.
    /// </summary>
    /// <returns>Collection of all items with their details</returns>
    Task<IEnumerable<Item>> GetAllAsync();
}
