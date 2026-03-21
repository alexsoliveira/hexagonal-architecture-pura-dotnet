namespace HexagonalLab.Core.UseCases;

using HexagonalLab.Core.Models;
using HexagonalLab.Core.Ports;

/// <summary>
/// UseCase: Processa um item.
/// 
/// CARACTERÍSTICAS CRÍTICAS:
/// ✅ Zero dependências de framework
/// ✅ Recebe Port via constructor (Dependency Injection)
/// ✅ Pode ser testado em memória (sem DB)
/// ✅ Implementa Input Port (interface)
/// ✅ Isola lógica de negócio
/// </summary>
public class ProcessItemUseCase : IItemInputPort
{
    /// <summary>
    /// Constructor
    /// </summary>
    public ProcessItemUseCase()
    {
    }

    /// <summary>
    /// Implementa Input Port.
    /// Lógica de negócio: "Processar um item".
    /// </summary>
    public Task<ItemResponse> ProcessAsync(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
            throw new ArgumentException("Item ID cannot be empty", nameof(itemId));

        var response = new ItemResponse
        {
            ItemId = itemId,
            Status = "Processed",           // ← Lógica simples de negócio
            ProcessedAt = DateTime.UtcNow,
            Message = $"Item {itemId} processed successfully"
        };

        return Task.FromResult(response);
    }
}
