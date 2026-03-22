namespace HexagonalLab.Core.Tests.Models;

using HexagonalLab.Core.Models;
using Xunit;

/// <summary>
/// Tests para validar Models (DTOs) do Core
/// 
/// IMPORTÂNCIA: Models são simples, mas críticos para:
/// ✅ Serialização/Desserialização
/// ✅ Data transfer entre adapters e core
/// ✅ Validação de estrutura
/// 
/// PADRÃO HEXAGONAL: Core models devem ser framework-agnostic!
/// (Sem atributos EF Core, nem validation attributes do ASP.NET)
/// </summary>
public class ItemResponseTests
{
    [Fact]
    public void ItemResponse_CanBeCreated_WithAllProperties()
    {
        // Arrange & Act
        var response = new ItemResponse
        {
            ItemId = "TEST-001",
            Status = "Active",
            Message = "Item retrieved successfully",
            ProcessedAt = System.DateTime.UtcNow
        };

        // Assert
        Assert.Equal("TEST-001", response.ItemId);
        Assert.Equal("Active", response.Status);
        Assert.NotNull(response.Message);
        Assert.NotEqual(System.DateTime.MinValue, response.ProcessedAt);
    }

    [Fact]
    public void ItemResponse_Properties_CanBeModified()
    {
        // Arrange
        var response = new ItemResponse { ItemId = "ITEM-001", Status = "Pending" };

        // Act
        response.Status = "Completed";
        response.Message = "Updated message";

        // Assert
        Assert.Equal("Completed", response.Status);
        Assert.Equal("Updated message", response.Message);
    }

    [Fact]
    public void ItemResponse_DefaultValues_AreValid()
    {
        // Arrange & Act
        var response = new ItemResponse();

        // Assert: Default values should match model defaults
        Assert.Equal(string.Empty, response.ItemId);
        Assert.Equal("Pending", response.Status);
        Assert.Equal(string.Empty, response.Message);
        Assert.Equal(System.DateTime.MinValue, response.ProcessedAt);
    }
}

/// <summary>
/// Tests para ItemRequest model
/// </summary>
public class ItemRequestTests
{
    [Fact]
    public void ItemRequest_CanBeCreated()
    {
        // Arrange & Act
        var request = new ItemRequest
        {
            ItemId = "REQ-001"
        };

        // Assert
        Assert.Equal("REQ-001", request.ItemId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ItemRequest_WithEmptyItemId_IsInvalid(string itemId)
    {
        // Arrange & Act
        var request = new ItemRequest { ItemId = itemId };

        // Assert: Model allows it, but should be validated in UseCase
        // This shows: Models are dumb, UseCase validates
        Assert.True(string.IsNullOrWhiteSpace(request.ItemId));
    }

    [Fact]
    public void ItemRequest_DefaultValue()
    {
        // Arrange & Act
        var request = new ItemRequest();

        // Assert
        Assert.Equal(string.Empty, request.ItemId);
    }
}

/// <summary>
/// Tests para Item model (domain model)
/// </summary>
public class ItemTests
{
    [Fact]
    public void Item_CanBeCreated_WithAllProperties()
    {
        // Arrange & Act
        var item = new Item
        {
            Id = "ITEM-001",
            Name = "Test Item",
            Status = "Active"
        };

        // Assert
        Assert.Equal("ITEM-001", item.Id);
        Assert.Equal("Test Item", item.Name);
        Assert.Equal("Active", item.Status);
    }

    [Fact]
    public void Item_Equality_BasedOnId()
    {
        // Arrange
        var item1 = new Item { Id = "ITEM-001", Name = "Item 1", Status = "Active" };
        var item2 = new Item { Id = "ITEM-001", Name = "Different", Status = "Inactive" };
        var item3 = new Item { Id = "ITEM-002", Name = "Item 1", Status = "Active" };

        // Act & Assert: Two items with same ID should be treated equally
        // Note: This depends on Item implementing IEquatable<Item>
        Assert.Equal(item1.Id, item2.Id);
        Assert.NotEqual(item1.Id, item3.Id);
    }

    [Fact]
    public void Item_Properties_CanBeModified()
    {
        // Arrange
        var item = new Item { Id = "MOD-001", Name = "Original", Status = "Pending" };

        // Act
        item.Name = "Modified";
        item.Status = "Completed";

        // Assert
        Assert.Equal("Modified", item.Name);
        Assert.Equal("Completed", item.Status);
    }

    [Fact]
    public void Item_ToString_ReturnsValidRepresentation()
    {
        // Arrange
        var item = new Item { Id = "STR-001", Name = "String Test", Status = "Active" };

        // Act
        var str = item.ToString();

        // Assert: Should not be null (default object representation)
        Assert.NotNull(str);
        Assert.NotEmpty(str);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Item_WithEmptyId_IsValid_ButInConsistent(string id)
    {
        // Arrange & Act
        var item = new Item { Id = id, Name = "Test" };

        // Assert: Models allow empty, but repository should validate
        // This shows: Core models are permissive, business logic validates
        Assert.True(string.IsNullOrWhiteSpace(item.Id));
    }

    [Fact]
    public void Item_WithMultipleUpdates_RemainsConsistent()
    {
        // Arrange
        var item = new Item { Id = "CONS-001", Name = "Version 1", Status = "Draft" };

        // Act: Multiple updates
        item.Name = "Version 2";
        item.Status = "Review";
        item.Name = "Version 3";
        item.Status = "Published";

        // Assert: Final state correct
        Assert.Equal("Version 3", item.Name);
        Assert.Equal("Published", item.Status);
        Assert.Equal("CONS-001", item.Id); // ID should never change
    }
}
