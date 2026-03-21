namespace HexagonalLab.API.Endpoints;

using HexagonalLab.Core.Models;
using HexagonalLab.Core.Ports;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

/// <summary>
/// Minimal API Endpoints para Items.
/// 
/// PADRÃO CRÍTICO (Input Adapter):
/// ✅ Traduz HTTP Request → Input Port
/// ✅ Recebe Port via DI
/// ✅ Não chama UseCase direto
/// ✅ Não sabe detalhes de implementação
/// 
/// EXEMPLO:
/// GET /api/items/{id}  →  IItemInputPort.ProcessAsync(id)
/// 
/// A MAGIA: Trocar Input Adapter (API → Worker) sem alterar Core!
/// </summary>
public static class ItemEndpoints
{
    /// <summary>
    /// Registra todos os endpoints de Items.
    /// Chamado em Program.cs durante Bootstrap.
    /// </summary>
    public static void MapItemEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/items")
            .WithName("Items");

        group.MapGet("/{id}", GetItem)
            .WithName("GetItem");

        group.MapPost("/{id}/process", ProcessItem)
            .WithName("ProcessItem");

        group.MapGet("/", GetAllItems)
            .WithName("GetAllItems");
    }

    /// <summary>
    /// GET /api/items/{id}
    /// Recupera um item.
    /// </summary>
    private static async Task<IResult> GetItem(
        string id,
        IItemInputPort useCase)  // ← DI: Recebe UseCase (Input Port)
    {
        try
        {
            var result = await useCase.ProcessAsync(id);
            return Results.Ok(result);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// POST /api/items/{id}/process
    /// Processa um item.
    /// </summary>
    private static async Task<IResult> ProcessItem(
        string id,
        IItemInputPort useCase)  // ← DI: Recebe UseCase (Input Port)
    {
        try
        {
            var result = await useCase.ProcessAsync(id);
            return Results.Ok(result);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message, statusCode: 500);
        }
    }

    /// <summary>
    /// GET /api/items
    /// Lista todos os itens (placeholder).
    /// </summary>
    private static Task<IResult> GetAllItems()
    {
        return Task.FromResult(Results.Ok(new[] { "item1", "item2" }));
    }
}
