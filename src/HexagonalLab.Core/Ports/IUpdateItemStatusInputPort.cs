namespace HexagonalLab.Core.Ports;

using HexagonalLab.Core.Models;

/// <summary>
/// Input Port: Update Item Status
/// 
/// Representa a capacidade de ATUALIZAR o status de um item.
/// 
/// CRÍTICO PARA HEXAGONAL:
/// ✅ Define o contrato (interface)
/// ✅ Zero conhecimento de implementação (DB, cache, etc.)
/// ✅ Adapters podem variar (SQL Server, SQLite, MongoDB, etc.)
/// ✅ Core NUNCA muda quando adapter muda
/// </summary>
public interface IUpdateItemStatusInputPort
{
    /// <summary>
    /// Atualiza o status de um item e retorna resposta.
    /// </summary>
    /// <param name="itemId">ID do item a atualizar</param>
    /// <param name="newStatus">Novo status (ex: "Processed", "Failed", etc.)</param>
    /// <returns>ItemResponse com status atualizado</returns>
    Task<ItemResponse> UpdateStatusAsync(string itemId, string newStatus);
}
