namespace HexagonalLab.Core.Models;

/// <summary>
/// DTO simples para requisição de processamento.
/// Sem validações complexas, sem frameworks - apenas dados.
/// </summary>
public class ItemRequest
{
    public string ItemId { get; set; } = string.Empty;
}
