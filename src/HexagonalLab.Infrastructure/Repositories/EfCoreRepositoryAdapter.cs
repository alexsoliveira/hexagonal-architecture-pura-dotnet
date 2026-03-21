namespace HexagonalLab.Infrastructure.Repositories;

using HexagonalLab.Core.Models;
using HexagonalLab.Core.Ports;
using HexagonalLab.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// EF Core Repository Adapter: Implementação REAL de IItemRepositoryPort.
/// 
/// PADRÃO CRÍTICO:
/// ✅ Implementa exatamente o que Core espera (IItemRepositoryPort)
/// ✅ Encapsula TODOS os detalhes de EF Core
/// ✅ Core nunca sabe que EF Core existe!
/// ✅ Pode trocar por Dapper, ADO.NET direto, etc = zero impact
/// 
/// DIFERENÇA com InMemoryRepository:
/// - InMemoryRepository: Dados em Dictionary (testes)
/// - EfCoreRepositoryAdapter: Dados em SQL Server (produção)
/// </summary>
public class EfCoreRepositoryAdapter : IItemRepositoryPort
{
    private readonly AppDbContext _context;

    public EfCoreRepositoryAdapter(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Persiste um item (INSERT ou UPDATE).
    /// </summary>
    public async Task SaveAsync(Item item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        var existingItem = await _context.Items.FindAsync(item.Id);

        if (existingItem == null)
        {
            _context.Items.Add(item);
        }
        else
        {
            existingItem.Name = item.Name;
            existingItem.Status = item.Status;
            existingItem.ProcessedAt = item.ProcessedAt;
            _context.Items.Update(existingItem);
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Recupera um item por ID.
    /// </summary>
    public async Task<Item?> GetByIdAsync(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
            throw new ArgumentException("Item ID cannot be empty", nameof(itemId));

        return await _context.Items
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == itemId);
    }

    /// <summary>
    /// Retorna todos os itens.
    /// </summary>
    public async Task<IEnumerable<Item>> GetAllAsync()
    {
        return await _context.Items
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// Deleta um item.
    /// </summary>
    public async Task DeleteAsync(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
            throw new ArgumentException("Item ID cannot be empty", nameof(itemId));

        var item = await _context.Items.FindAsync(itemId);
        if (item != null)
        {
            _context.Items.Remove(item);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Verifica se item existe.
    /// </summary>
    public async Task<bool> ExistsAsync(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
            throw new ArgumentException("Item ID cannot be empty", nameof(itemId));

        return await _context.Items
            .AnyAsync(x => x.Id == itemId);
    }
}
