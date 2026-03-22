namespace HexagonalLab.Core.UseCases;

using HexagonalLab.Core.Models;
using HexagonalLab.Core.Ports;

/// <summary>
/// UseCase: Update Item Status
/// 
/// RESPONSABILIDADE:
/// - Recebe um Item ID e novo status
/// - Delega para Output Port para PERSISTIR
/// - Retorna resposta com novo status
/// 
/// CRÍTICO PARA HEXAGONAL:
/// ✅ Zero conhecimento de HOW storage works
/// ✅ Apenas usa Output Port (interface)
/// ✅ 100% testável sem banco de dados
/// ✅ StorageAdapter pode mudar, UseCase NÃO muda
/// 
/// PATTERN: Input Port → UseCase → Output Port
/// </summary>
public class UpdateItemStatusUseCase : IUpdateItemStatusInputPort
{
    private readonly IItemRepositoryPort _repository;

    public UpdateItemStatusUseCase(IItemRepositoryPort repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<ItemResponse> UpdateStatusAsync(string itemId, string newStatus)
    {
        if (string.IsNullOrWhiteSpace(itemId))
            throw new ArgumentException("Item ID cannot be empty", nameof(itemId));

        if (string.IsNullOrWhiteSpace(newStatus))
            throw new ArgumentException("Status cannot be empty", nameof(newStatus));

        // ✅ Usa Output Port (não sabe se é DB, cache, file, etc.)
        // Repository implementa a persistência real
        var item = await _repository.GetByIdAsync(itemId);
        
        if (item == null)
            throw new InvalidOperationException($"Item {itemId} not found");

        // ✅ Atualiza in-memory (transação conceitual)
        item.Status = newStatus;
        item.ProcessedAt = DateTime.UtcNow;

        // ✅ Persiste via Output Port (SaveAsync para CREATE ou UPDATE)
        await _repository.SaveAsync(item);

        // ✅ Retorna resposta com status atualizado
        return new ItemResponse
        {
            ItemId = itemId,
            Status = newStatus,
            ProcessedAt = DateTime.UtcNow,
            Message = $"Item {itemId} status updated to {newStatus} successfully"
        };
    }
}
