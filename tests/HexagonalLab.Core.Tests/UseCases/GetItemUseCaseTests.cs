namespace HexagonalLab.Core.Tests.UseCases;

using HexagonalLab.Core.Models;
using HexagonalLab.Core.Tests.Adapters;
using HexagonalLab.Core.UseCases;
using Xunit;

/// <summary>
/// Tests for GetItemUseCase using Fake Adapter.
///
/// CRITICAL: These tests prove Output Port pattern works!
///
/// WHY THIS IS IMPORTANT:
/// ✅ Tests run WITHOUT database
/// ✅ Tests run WITHOUT any framework
/// ✅ UseCase works with injected Output Port
/// ✅ Swappable implementation (today: InMemory, tomorrow: EF Core)
/// ✅ 100% testable, 0% coupling to infrastructure
///
/// PATTERN:
/// 1. Arrange: Create fake adapter and seed data
/// 2. Act: Inject fake adapter into UseCase
/// 3. Assert: UseCase works without knowing it's fake!
///
/// This is Hexagonal Architecture in action.
/// </summary>
public class GetItemUseCaseTests
{
    /// <summary>
    /// Given: Item exists in repository
    /// When: GetItemUseCase calls ProcessAsync
    /// Then: Returns ItemResponse with item data
    ///
    /// KEY POINT: Repository is FAKE (in-memory), not real database!
    /// UseCase has ZERO idea it's fake.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_WithExistingItem_ReturnsItemData()
    {
        // Arrange: Setup fake repository with test data
        var fakeRepository = new InMemoryRepositoryAdapter();
        var testItem = new Item { Id = "ITEM-001", Name = "Test Item", Status = "Pending" };
        await fakeRepository.SaveAsync(testItem);

        // Create UseCase injecting FAKE repository
        var useCase = new GetItemUseCase(fakeRepository);

        // Act: Call ProcessAsync (UseCase doesn't know repository is fake!)
        var result = await useCase.ProcessAsync("ITEM-001");

        // Assert: Verify correct data returned
        Assert.NotNull(result);
        Assert.Equal("ITEM-001", result.ItemId);
        Assert.Equal("Pending", result.Status);
        Assert.Contains("retrieved successfully", result.Message);
    }

    /// <summary>
    /// Given: Item doesn't exist in repository
    /// When: GetItemUseCase calls ProcessAsync
    /// Then: Returns "Not Found" response
    /// </summary>
    [Fact]
    public async Task ProcessAsync_WithNonExistentItem_ReturnsNotFound()
    {
        // Arrange
        var fakeRepository = new InMemoryRepositoryAdapter();
        var useCase = new GetItemUseCase(fakeRepository);

        // Act
        var result = await useCase.ProcessAsync("NON-EXISTENT");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("NON-EXISTENT", result.ItemId);
        Assert.Equal("Not Found", result.Status);
        Assert.Contains("not found", result.Message);
    }

    /// <summary>
    /// Given: Multiple items in repository
    /// When: GetItemUseCase retrieves different items
    /// Then: Returns correct item each time
    ///
    /// Tests that fake repository maintains separate state per item.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_WithMultipleItems_ReturnsCorrectItem()
    {
        // Arrange: Seed repository with multiple items
        var fakeRepository = new InMemoryRepositoryAdapter();
        var items = new[]
        {
            new Item { Id = "ITEM-001", Name = "Item 1", Status = "Completed" },
            new Item { Id = "ITEM-002", Name = "Item 2", Status = "Processing" },
            new Item { Id = "ITEM-003", Name = "Item 3", Status = "Pending" }
        };

        foreach (var item in items)
            await fakeRepository.SaveAsync(item);

        var useCase = new GetItemUseCase(fakeRepository);

        // Act: Retrieve each item
        var result1 = await useCase.ProcessAsync("ITEM-001");
        var result2 = await useCase.ProcessAsync("ITEM-002");
        var result3 = await useCase.ProcessAsync("ITEM-003");

        // Assert: Each returns correct item
        Assert.Equal("Completed", result1.Status);
        Assert.Equal("Processing", result2.Status);
        Assert.Equal("Pending", result3.Status);
    }

    /// <summary>
    /// Given: Invalid ItemId
    /// When: GetItemUseCase calls ProcessAsync
    /// Then: Throws ArgumentException
    ///
    /// Shows that validation happens in Core (UseCase), not in repository.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ProcessAsync_WithInvalidItemId_ThrowsArgumentException(string itemId)
    {
        // Arrange
        var fakeRepository = new InMemoryRepositoryAdapter();
        var useCase = new GetItemUseCase(fakeRepository);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => useCase.ProcessAsync(itemId));
    }

    /// <summary>
    /// Given: Repository reference is null
    /// When: Creating GetItemUseCase
    /// Then: Throws ArgumentNullException
    ///
    /// Shows that Output Port is REQUIRED (DIP - Dependency Inversion Principle).
    /// </summary>
    [Fact]
    public void Constructor_WithNullRepository_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new GetItemUseCase(null!));
    }

    /// <summary>
    /// TEST INTEGRATION DEMO:
    /// Shows that InMemoryRepositoryAdapter truly simulates persistence.
    ///
    /// This is the PROOF that Output Port pattern works!
    /// UseCase treats fake adapter same as real database.
    /// </summary>
    [Fact]
    public async Task FullFlow_SaveAndRetrieve_DemonstratesPersistence()
    {
        // Arrange: Setup fake repository
        var fakeRepository = new InMemoryRepositoryAdapter();

        // Create another UseCase that will SAVE to repository
        var saveUseCase = new ProcessItemUseCase();

        // Create GetItemUseCase that will RETRIEVE
        var getUseCase = new GetItemUseCase(fakeRepository);

        // Act: Process item (in-memory, no persistence yet)
        var processResult = await saveUseCase.ProcessAsync("ITEM-NEW");

        // Now save to fake repository
        var itemToSave = new Item
        {
            Id = "ITEM-NEW",
            Name = processResult.Message,
            Status = processResult.Status,
            ProcessedAt = processResult.ProcessedAt
        };
        await fakeRepository.SaveAsync(itemToSave);

        // Retrieve from fake repository
        var retrieveResult = await getUseCase.ProcessAsync("ITEM-NEW");

        // Assert: Complete round-trip works
        Assert.NotNull(retrieveResult);
        Assert.Equal("ITEM-NEW", retrieveResult.ItemId);
        Assert.Equal("Processed", retrieveResult.Status);
    }
}
