# 📌 PHASE 2: Design Output Port Pattern (Dia 2)

**Story ID:** 273  
**Azure DevOps Link:** [Story 273](https://dev.azure.com/alexestudocertificacoes/e699c50b-3ca9-45b5-ba7c-29591ee19071/web/wi.aspx?pcguid=&id=273)  
**Feature:** 271 - Output Adapters  
**Epic:** 268 - Hexagonal Architecture Lab  

---

## 🎯 Objetivo

Implementar o **padrão de Output Port** criando:
- **Output Port Interface** (contrato de saída)
- **In-Memory Adapter** (implementação fake)
- **UseCase que usa Output Port** (sem conhecer implementação)

### Resultado Esperado
- ✅ Output Port definido (interface pura)
- ✅ In-Memory Adapter implementado
- ✅ UseCase injeta Port via constructor
- ✅ Core **ainda sem dependências**
- ✅ Testes mockam Output Port facilmente

---

## 📋 Tarefas Técnicas

| # | Descrição | Tipo | Duração Est. | Dependência |
|---|-----------|------|-------------|-------------|
| T1 | Design Output Port Interface | Core | 15 min | Phase 1 ✅ |
| T2 | Create In-Memory Adapter | Test | 20 min | T1 |
| T3 | Refactor UseCase to use Port | Core | 20 min | T1 |
| T4 | Create Tests with Mock | Test | 25 min | T2-T3 |
| T5 | Validate Core ZERO deps | Core | 10 min | T1-T4 |

**Total Estimado:** ~1.5 horas  
**Bloqueador Anterior:** ✅ Phase 1 (DONE)

---

## 🏗️ Estrutura de Pastas Esperada

```
HexagonalLab.NET10/
│
├── src/
│   └── HexagonalLab.Core/
│       ├── Ports/
│       │   ├── IItemInputPort.cs           ← (já existe - Phase 1)
│       │   └── IItemRepositoryPort.cs      ← OUTPUT PORT (novo!)
│       │
│       ├── UseCases/
│       │   ├── ProcessItemUseCase.cs       ← (refatorado)
│       │   └── GetItemUseCase.cs           ← (novo - usa Output Port)
│       │
│       └── Models/
│           ├── ItemRequest.cs
│           ├── ItemResponse.cs
│           └── Item.cs                     ← Domain model
│
└── tests/
    └── HexagonalLab.Core.Tests/
        ├── Adapters/
        │   └── InMemoryRepositoryAdapter.cs ← FAKE ADAPTER (novo!)
        │
        └── UseCases/
            ├── ProcessItemUseCaseTests.cs
            └── GetItemUseCaseTests.cs      ← (novo - com mock)
```

---

## 💻 Passo a Passo: Implementação

### T1: Design Output Port Interface

#### **Arquivo: src/HexagonalLab.Core/Ports/IItemRepositoryPort.cs**

```csharp
namespace HexagonalLab.Core.Ports;

using HexagonalLab.Core.Models;

/// <summary>
/// Output Port: Contrato para acesso a dados.
/// 
/// CRÍTICO: Esta interface:
/// ✅ Define o que o Core PRECISA (lê de fora)
/// ✅ Não sabe HOW (banco, cache, arquivo, etc.)
/// ✅ É pura abstração no Core
/// ✅ Adapters implementam concretamente
/// 
/// Referência: Alistair Cockburn - "Ports are contracts"
/// </summary>
public interface IItemRepositoryPort
{
    /// <summary>
    /// Persiste um item (CREATE/UPDATE).
    /// O Core não sabe se é EF Core, Dapper, SQL direto, arquivo, etc.
    /// </summary>
    Task SaveAsync(Item item);

    /// <summary>
    /// Recupera um item por ID.
    /// O Core não sabe de qual fonte (DB, cache, etc).
    /// </summary>
    Task<Item?> GetByIdAsync(string itemId);

    /// <summary>
    /// Retorna todos os itens.
    /// </summary>
    Task<IEnumerable<Item>> GetAllAsync();

    /// <summary>
    /// Deleta um item.
    /// </summary>
    Task DeleteAsync(string itemId);

    /// <summary>
    /// Verifica se item existe.
    /// </summary>
    Task<bool> ExistsAsync(string itemId);
}
```

### T2: Create In-Memory Adapter (Fake)

#### **Arquivo: tests/HexagonalLab.Core.Tests/Adapters/InMemoryRepositoryAdapter.cs**

```csharp
namespace HexagonalLab.Core.Tests.Adapters;

using HexagonalLab.Core.Models;
using HexagonalLab.Core.Ports;

/// <summary>
/// In-Memory Repository Adapter (Fake/Test Adapter)
/// 
/// CARACTERÍSTICAS:
/// ✅ Implementa IItemRepositoryPort (Output Port)
/// ✅ Armazena dados em memória (Dictionary)
/// ✅ Simula comportamento de repositório real
/// ✅ Sem dependências externas (perfeito para testes)
/// ✅ Prova que Output Port é agnóstico de implementação
/// 
/// QUANDO USAR: Testes unitários do Core, Phase 2-3
/// QUANDO NÃO USAR: Produção
/// </summary>
public class InMemoryRepositoryAdapter : IItemRepositoryPort
{
    private readonly Dictionary<string, Item> _store = new();

    /// <summary>
    /// Persiste um item em memória.
    /// Simula transaction commit.
    /// </summary>
    public Task SaveAsync(Item item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        _store[item.Id] = item;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Recupera item por ID.
    /// Retorna null se não encontrado (como DB).
    /// </summary>
    public Task<Item?> GetByIdAsync(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
            throw new ArgumentException("Item ID cannot be empty", nameof(itemId));

        var item = _store.TryGetValue(itemId, out var value) ? value : null;
        return Task.FromResult(item);
    }

    /// <summary>
    /// Retorna todos os itens.
    /// </summary>
    public Task<IEnumerable<Item>> GetAllAsync()
    {
        var items = _store.Values.AsEnumerable();
        return Task.FromResult(items);
    }

    /// <summary>
    /// Deleta um item.
    /// Não falha se não existir (idempotent).
    /// </summary>
    public Task DeleteAsync(string itemId)
    {
        _store.Remove(itemId);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Verifica existência.
    /// </summary>
    public Task<bool> ExistsAsync(string itemId)
    {
        var exists = _store.ContainsKey(itemId);
        return Task.FromResult(exists);
    }

    /// <summary>
    /// Helper para testes: inserir dados iniciais.
    /// </summary>
    public async Task SeedAsync(params Item[] items)
    {
        foreach (var item in items)
            await SaveAsync(item);
    }

    /// <summary>
    /// Helper para testes: limpar dados.
    /// </summary>
    public void Clear()
    {
        _store.Clear();
    }
}
```

### T3: Refactor UseCase to Use Output Port

#### **Arquivo: src/HexagonalLab.Core/Models/Item.cs** (novo)

```csharp
namespace HexagonalLab.Core.Models;

/// <summary>
/// Item domain model (DTO simples - sem Entity Framework).
/// Apenas dados, sem lógica complexa.
/// </summary>
public class Item
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
}
```

#### **Arquivo: src/HexagonalLab.Core/UseCases/GetItemUseCase.cs** (novo)

```csharp
namespace HexagonalLab.Core.UseCases;

using HexagonalLab.Core.Models;
using HexagonalLab.Core.Ports;

/// <summary>
/// UseCase: Recupera um item.
/// 
/// PADRÃO CRÍTICO:
/// ✅ Implementa Input Port
/// ✅ Usa Output Port (rejeitado via constructor)
/// ✅ Não sabe COMO dados são recuperados
/// ✅ Apenas: pede ao Port, recebe resposta, retorna
/// 
/// OUTPUT PORT INJECTION:
/// ```csharp
/// public GetItemUseCase(IItemRepositoryPort repository)
/// {
///     _repository = repository; // Pode ser In-Memory, EF, Dapper, HTTP, etc.
/// }
/// ```
/// </summary>
public class GetItemUseCase : IItemInputPort
{
    private readonly IItemRepositoryPort _repository;

    /// <summary>
    /// Constructor: Recebe Output Port via Dependency Injection.
    /// CRÍTICO: é uma interface, não implementação concreta!
    /// </summary>
    public GetItemUseCase(IItemRepositoryPort repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>
    /// Implementa Input Port.
    /// Orquestra: "Peguei o ID → Pedi ao Port → Retornei resposta"
    /// </summary>
    public async Task<ItemResponse> ProcessAsync(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
            throw new ArgumentException("Item ID cannot be empty", nameof(itemId));

        // ✅ CRÍTICO: usa Port (interface)
        // ❌ Não sabe se é EF Core, Dapper, arquivo, HTTP, etc.
        var item = await _repository.GetByIdAsync(itemId);

        if (item == null)
            return new ItemResponse
            {
                ItemId = itemId,
                Status = "NotFound",
                Message = $"Item {itemId} not found"
            };

        return new ItemResponse
        {
            ItemId = item.Id,
            Status = item.Status,
            ProcessedAt = item.ProcessedAt ?? DateTime.UtcNow,
            Message = $"Item {itemId} retrieved successfully"
        };
    }
}
```

#### **Arquivo: src/HexagonalLab.Core/Ports/IItemInputPort.cs** (atualizado)

```csharp
namespace HexagonalLab.Core.Ports;

using HexagonalLab.Core.Models;

/// <summary>
/// Input Port - Contrato de entrada. (atualizado para retornar ItemResponse)
/// </summary>
public interface IItemInputPort
{
    Task<ItemResponse> ProcessAsync(string itemId);
}
```

### T4: Create Tests with Mocks

#### **Arquivo: tests/HexagonalLab.Core.Tests/UseCases/GetItemUseCaseTests.cs** (novo)

```csharp
namespace HexagonalLab.Core.Tests.UseCases;

using HexagonalLab.Core.Models;
using HexagonalLab.Core.Tests.Adapters;
using HexagonalLab.Core.UseCases;
using Xunit;

/// <summary>
/// Unit tests: GetItemUseCase
/// 
/// PONTO CRÍTICO: UseCase é testado com In-Memory Adapter.
/// Prova que:
/// ✅ Core não depende de banco real
/// ✅ Output Port funciona com múltiplos adapters
/// ✅ Tests rodam rápido (sem I/O)
/// </summary>
public class GetItemUseCaseTests
{
    [Fact]
    public async Task ProcessAsync_WithExistingItem_ReturnsItemResponse()
    {
        // Arrange
        var repository = new InMemoryRepositoryAdapter();
        var item = new Item { Id = "ITEM-001", Name = "Test Item", Status = "Active" };
        await repository.SaveAsync(item);

        var useCase = new GetItemUseCase(repository);

        // Act
        var result = await useCase.ProcessAsync("ITEM-001");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("ITEM-001", result.ItemId);
        Assert.Equal("Active", result.Status);
        Assert.Contains("retrieved successfully", result.Message);
    }

    [Fact]
    public async Task ProcessAsync_WithNonExistentItem_ReturnsNotFoundStatus()
    {
        // Arrange
        var repository = new InMemoryRepositoryAdapter();
        var useCase = new GetItemUseCase(repository);

        // Act
        var result = await useCase.ProcessAsync("ITEM-999");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("ITEM-999", result.ItemId);
        Assert.Equal("NotFound", result.Status);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task ProcessAsync_WithInvalidItemId_ThrowsArgumentException(string itemId)
    {
        // Arrange
        var repository = new InMemoryRepositoryAdapter();
        var useCase = new GetItemUseCase(repository);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => useCase.ProcessAsync(itemId));
    }

    [Fact]
    public async Task ProcessAsync_WithMultipleItems_ReturnsCorrectOne()
    {
        // Arrange
        var repository = new InMemoryRepositoryAdapter();
        var items = new[]
        {
            new Item { Id = "ITEM-001", Name = "Item 1", Status = "Active" },
            new Item { Id = "ITEM-002", Name = "Item 2", Status = "Inactive" },
            new Item { Id = "ITEM-003", Name = "Item 3", Status = "Pending" }
        };
        foreach (var item in items)
            await repository.SaveAsync(item);

        var useCase = new GetItemUseCase(repository);

        // Act
        var result = await useCase.ProcessAsync("ITEM-002");

        // Assert
        Assert.Equal("ITEM-002", result.ItemId);
        Assert.Equal("Inactive", result.Status);
    }

    [Fact]
    public void GetItemUseCase_NullRepository_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new GetItemUseCase(null!) // Força null
        );
    }
}
```

### Build and Run Tests

```bash
dotnet build HexagonalLab.NET10.sln

dotnet test tests/HexagonalLab.Core.Tests/ --verbosity detailed

# Output esperado:
# ✅ GetItemUseCaseTests.ProcessAsync_WithExistingItem_ReturnsItemResponse PASSED
# ✅ GetItemUseCaseTests.ProcessAsync_WithNonExistentItem_ReturnsNotFoundStatus PASSED
# ✅ GetItemUseCaseTests.ProcessAsync_WithInvalidItemId_ThrowsArgumentException PASSED (3x)
# ✅ GetItemUseCaseTests.ProcessAsync_WithMultipleItems_ReturnsCorrectOne PASSED
# ✅ GetItemUseCaseTests_NullRepository_ThrowsArgumentNullException PASSED
```

---

## ✅ Critério de Aceitação

Completar Phase 2 quando:

- [x] **Output Port interface definida**
  - Métodos: Save, Get, GetAll, Delete, Exists
  - Sem implementação concreta

- [x] **In-Memory Adapter implementado**
  - Implementa IItemRepositoryPort
  - Armazena em Dictionary
  - Sem dependências externas

- [x] **UseCase refatorado**
  - Recebe Port via constructor
  - Usa apenas Port (não conhece implementação)
  - Não faz queries diretas

- [x] **Testes com Mocks passam**
  ```
  dotnet test
  ✅ 8+ passed
  ```

- [x] **Core AINDA com ZERO dependências**
  ```xml
  <!-- HexagonalLab.Core.csproj: sem <PackageReference> -->
  ```

- [x] **Contrato validado**
  - Port é agnóstico
  - Adapter intercambiável
  - Tests usam fake facilmente

---

## 🏛️ Decisões Arquiteturais

### Pattern: Dependency Inversion

```csharp
❌ BAD (Tight Coupling):
class GetItemUseCase {
    var item = database.Query("SELECT ... WHERE id = ?");  // ← Conhece implementação
}

✅ GOOD (Loose Coupling):
class GetItemUseCase {
    var item = _repository.GetByIdAsync(itemId);  // ← Conhece apenas o contrato
}
```

### Por que In-Memory Adapter em Tests?

| Aspecto | In-Memory | Real DB |
|---------|-----------|---------|
| **Velocidade** | ~1ms | ~100ms+ |
| **Setup** | Trivial | Complex |
| **Isolation** | Total | Shared |
| **Uso em Phase 2** | ✅ Perfeito | ❌ Não (Phase 4) |

### Referência: Cockburn Paper

> "The application should not depend on the details of how data is persisted. Instead, it should depend on an abstraction."

---

## 🚨 Possíveis Problemas

### Problema 1: "In-Memory Adapter não é thread-safe"
**Resposta:** Correto para Phase 2! Tests podem ser single-threaded. Thread-safety vem com real adapter (Phase 4).

### Problema 2: "UseCase não valida dados"
**Resposta:** Data validation pode vir depois. Phase 2 foca em Port pattern, não validação.

### Problema 3: "Output Port tem muitos métodos"
**Resposta:** Correto! Port define contrato completo. Adapters usam todos, mas UseCase usa apenas o necessário.

---

## 📌 Próximos Passos (Phase 3)

Quando Phase 2 estiver ✅ **DONE**:

1. Atualizar Story 273 status → `Done`
2. Iniciar Story 277 (Phase 3 - Connect API Adapter)
3. Criar projeto Web API
4. Conectar API endpoints ao Input Port

---

## 🔗 Referências

- [Dependency Inversion Principle](https://en.wikipedia.org/wiki/Dependency_inversion_principle)
- [Mockito/Mocking Pattern](https://martinfowler.com/articles/mocksArentStubs.html)
- [Alistair Cockburn - Ports & Adapters](https://alistair.cockburn.us/hexagonal-architecture/)

---

**Status:** Ready for Implementation  
**Last Updated:** 2026-03-20  
**Author:** Architecture Team
