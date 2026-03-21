namespace HexagonalLab.API.Tests.Endpoints;

using HexagonalLab.API.Tests.Fixtures;
using HexagonalLab.Core.Models;
using HexagonalLab.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text.Json;
using Xunit;

/// <summary>
/// E2E Integration Tests: Item Endpoints
/// 
/// FASE 4:
/// ✅ Testa API contra EF Core real (SQLite em memória)
/// ✅ Prova end-to-end HTTP flow com banco de dados
/// ✅ Valida serialization/deserialization
/// ✅ Core nunca foi alterado
/// </summary>
public class ItemEndpointsTests : IAsyncLifetime
{
    private readonly ItemApiWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ItemEndpointsTests()
    {
        _factory = new ItemApiWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        // Initialize in-memory database before each test
        await _factory.InitializeDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        // Cleanup
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task GetItem_WithExistingItem_ReturnsOk()
    {
        // Arrange: Insert item directly to database
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var item = new Item { Id = "ITEM-001", Name = "Test Item", Status = "Active" };
        context.Items.Add(item);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/items/ITEM-001");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        var content = JsonSerializer.Deserialize<ItemResponse>(json);
        Assert.NotNull(content);
        Assert.Equal("ITEM-001", content!.ItemId);
    }

    [Fact]
    public async Task GetItem_WithNonExistentItem_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/items/ITEM-999");

        // Assert
        // Item não encontrado, mas endpoint retorna 200 com NotFound
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ProcessItem_WithValidId_ReturnsOk()
    {
        // Arrange: Insert item to database
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var item = new Item { Id = "ITEM-001", Name = "Test", Status = "Pending" };
        context.Items.Add(item);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.PostAsync("/api/items/ITEM-001/process", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAllItems_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/items/");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
