namespace HexagonalLab.Infrastructure.Repositories;

using HexagonalLab.Core.Models;
using HexagonalLab.Core.Ports;
using Microsoft.Extensions.Caching.Memory;

/// <summary>
/// Cached Repository Adapter (Decorator Pattern)
/// 
/// PADRÃO CRÍTICO - Decorator:
/// ✅ Implementa IItemRepositoryPort (MESMA interface)
/// ✅ Wraps outro adapter (EF Core)
/// ✅ Adiciona caching transparentemente
/// ✅ Zero mudanças em Core ou UseCase
/// ✅ DI apenas muda: qual adapter registra
/// 
/// BENEFÍCIO:
/// Antes (sem cache):
/// services.AddScoped&lt;IItemRepositoryPort, EfCoreRepositoryAdapter&gt;();
/// 
/// Depois (com cache):
/// services.AddScoped&lt;IItemRepositoryPort&gt;(sp =&gt;
///     new CachedRepositoryAdapter(
///         sp.GetRequiredService&lt;EfCoreRepositoryAdapter&gt;(),
///         sp.GetRequiredService&lt;IMemoryCache&gt;()
///     ));
/// 
/// Core NÃO mudou! Apenas bootstrap!
/// </summary>
public class CachedRepositoryAdapter : IItemRepositoryPort
{
    private readonly IItemRepositoryPort _innerAdapter;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheDuration;

    private const string CacheKeyPrefix = "item_";
    private const string CacheKeyAll = "items_all";

    public CachedRepositoryAdapter(
        IItemRepositoryPort innerAdapter,
        IMemoryCache cache,
        TimeSpan? cacheDuration = null)
    {
        _innerAdapter = innerAdapter ?? throw new ArgumentNullException(nameof(innerAdapter));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _cacheDuration = cacheDuration ?? TimeSpan.FromMinutes(5);
    }

    /// <summary>
    /// Persiste item.
    /// Invalida cache ao salvar.
    /// </summary>
    public async Task SaveAsync(Item item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        // Salva no adapter real
        await _innerAdapter.SaveAsync(item);

        // Invalida cache
        _cache.Remove($"{CacheKeyPrefix}{item.Id}");
        _cache.Remove(CacheKeyAll);
    }

    /// <summary>
    /// Recupera item COM CACHE.
    /// Se estiver em cache, retorna é imediato.
    /// Se não estiver, busca do adapter real e cacheia.
    /// </summary>
    public async Task<Item?> GetByIdAsync(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
            throw new ArgumentException("Item ID cannot be empty", nameof(itemId));

        var cacheKey = $"{CacheKeyPrefix}{itemId}";

        // Tentar cache primeiro
        if (_cache.TryGetValue(cacheKey, out Item? cachedItem))
            return cachedItem;

        // Se não estiver em cache, buscar do adapter real
        var item = await _innerAdapter.GetByIdAsync(itemId);

        // Cachear o resultado (mesmo se for null!)
        if (item != null)
            _cache.Set(cacheKey, item, _cacheDuration);

        return item;
    }

    /// <summary>
    /// Recupera todos COM CACHE.
    /// </summary>
    public async Task<IEnumerable<Item>> GetAllAsync()
    {
        if (_cache.TryGetValue(CacheKeyAll, out IEnumerable<Item>? cachedItems))
            return cachedItems!;

        var items = (await _innerAdapter.GetAllAsync()).ToList();
        _cache.Set(CacheKeyAll, items, _cacheDuration);

        return items;
    }

    /// <summary>
    /// Deleta item.
    /// Invalida cache.
    /// </summary>
    public async Task DeleteAsync(string itemId)
    {
        await _innerAdapter.DeleteAsync(itemId);

        // Invalidar cache
        _cache.Remove($"{CacheKeyPrefix}{itemId}");
        _cache.Remove(CacheKeyAll);
    }

    /// <summary>
    /// Verifica existência.
    /// Pode usar cache de Get se disponível.
    /// </summary>
    public async Task<bool> ExistsAsync(string itemId)
    {
        var item = await GetByIdAsync(itemId);
        return item != null;
    }

    /// <summary>
    /// Limpar cache (útil para testes).
    /// </summary>
    public void ClearCache()
    {
        _cache.Remove(CacheKeyAll);
        // Nota: Não podemos limpar individual keys sem tracking
    }
}
