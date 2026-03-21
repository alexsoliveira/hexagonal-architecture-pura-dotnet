namespace HexagonalLab.Core.Tests.UseCases;

using HexagonalLab.Core.UseCases;
using Xunit;

/// <summary>
/// Unit tests para o primeiro UseCase.
/// 
/// PONTO CRÍTICO: Estes testes rodam SEM BANCO DE DADOS!
/// Prova que o Core é isolado de infraestrutura.
/// </summary>
public class ProcessItemUseCaseTests
{
    /// <summary>
    /// Given: ID válido
    /// When: ProcessAsync é chamado
    /// Then: Retorna ItemResponse com status "Processed"
    /// </summary>
    [Fact]
    public async Task ProcessAsync_WithValidItemId_ReturnsProcessedStatus()
    {
        // Arrange
        var useCase = new ProcessItemUseCase();
        var itemId = "ITEM-001";

        // Act - NO DATABASE, NO FRAMEWORKS, PURE MEMORY
        var result = await useCase.ProcessAsync(itemId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(itemId, result.ItemId);
        Assert.Equal("Processed", result.Status);
        Assert.NotEqual(default, result.ProcessedAt);
    }

    /// <summary>
    /// Given: ID vazio
    /// When: ProcessAsync é chamado
    /// Then: Lança ArgumentException
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ProcessAsync_WithInvalidItemId_ThrowsArgumentException(string itemId)
    {
        // Arrange
        var useCase = new ProcessItemUseCase();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => useCase.ProcessAsync(itemId));
    }

    /// <summary>
    /// Given: Multiple items
    /// When: ProcessAsync é chamado múltiplas vezes
    /// Then: Cada um retorna resultado independente
    /// </summary>
    [Fact]
    public async Task ProcessAsync_WithMultipleItems_ReturnsIndependentResults()
    {
        // Arrange
        var useCase = new ProcessItemUseCase();
        var itemIds = new[] { "ITEM-001", "ITEM-002", "ITEM-003" };

        // Act
        var results = await Task.WhenAll(
            itemIds.Select(id => useCase.ProcessAsync(id))
        );

        // Assert
        Assert.Equal(3, results.Length);
        Assert.All(results, r =>
        {
            Assert.Equal("Processed", r.Status);
            Assert.True(r.ProcessedAt > DateTime.MinValue);
        });
    }
}
