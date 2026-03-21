namespace HexagonalLab.Core.Models;

/// <summary>
/// DTO para resposta do UseCase.
/// Contém resultado e metadados do processamento.
/// </summary>
public class ItemResponse
{
    public string ItemId { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public DateTime ProcessedAt { get; set; }
    public string Message { get; set; } = string.Empty;
}
