namespace HexagonalLab.Core.Models;

/// <summary>
/// Item Domain Model.
///
/// Simple DTO without Entity Framework attributes or complex logic.
/// Represents the business concept of an "Item" that can be processed.
///
/// DESIGN PRINCIPLES:
/// ✅ No EF Core annotations (keeps Core framework-agnostic)
/// ✅ No validation logic (that's for UseCase layer)
/// ✅ Simple data container
/// ✅ Works with any storage adapter
/// </summary>
public class Item
{
    /// <summary>
    /// Unique identifier for the item.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Item name or description.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Current processing status.
    /// Examples: "Pending", "Processing", "Completed", "Failed"
    /// </summary>
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// When was the item created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When was the item last processed.
    /// Null if never processed.
    /// </summary>
    public DateTime? ProcessedAt { get; set; }
}
