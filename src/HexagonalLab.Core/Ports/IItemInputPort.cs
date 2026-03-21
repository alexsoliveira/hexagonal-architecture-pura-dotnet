namespace HexagonalLab.Core.Ports;

using HexagonalLab.Core.Models;

/// <summary>
/// Input Port - Define o contrato de entrada para o UseCase.
/// Não depende de nada externo (sem HTTP, sem banco, sem frameworks).
/// </summary>
public interface IItemInputPort
{
    /// <summary>
    /// Processa um item identificado pelo ID.
    /// </summary>
    /// <param name="itemId">ID do item a ser processado</param>
    /// <returns>Resultado do processamento</returns>
    Task<ItemResponse> ProcessAsync(string itemId);
}
