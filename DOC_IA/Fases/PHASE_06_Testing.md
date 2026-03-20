# 📌 PHASE 6: Unit Testing Strategy (Dia 6)

**Story ID:** 279  
**Azure DevOps Link:** [Story 279](https://dev.azure.com/alexestudocertificacoes/e699c50b-3ca9-45b5-ba7c-29591ee19071/web/wi.aspx?pcguid=&id=279)  
**Feature:** 272 - Testing Strategy  
**Epic:** 268 - Hexagonal Architecture Lab  

---

## 🎯 Objetivo

Estabelecer **estratégia de testes robusta** que valida:
- Core testável em isolamento
- Ports são contratos que podem ser mockados
- Arquitetura permite testes em múltiplas camadas

### Resultado Esperado
- ✅ Unit tests do Core (sem DB/API)
- ✅ Integration tests com adaptadores reais
- ✅ E2E tests (API + Worker + DB)
- ✅ 80%+ code coverage
- ✅ Testes documento comportamento

---

## 📋 Tarefas Técnicas

| # | Descrição | Tipo | Duração Est. | Dependência |
|---|-----------|------|-------------|-------------|
| T1 | Comprehensive Unit Tests (Core) | Test | 30 min | Phase 5 ✅ |
| T2 | Integration Tests (Adapters) | Test | 25 min | T1 |
| T3 | E2E Tests (Full Flow) | Test | 25 min | T2 |
| T4 | Code Coverage Analysis | Test | 15 min | T3 |
| T5 | Document Test Pyramid | Doc | 15 min | T4 |

**Total Estimado:** ~2 horas  
**Bloqueador Anterior:** ✅ Phase 5 (DONE)

---

## 🏗️ Test Pyramid Structure

```
PROJECT STRUCTURE:
tests/
├── HexagonalLab.Core.Tests/              ← Unit Tests (Rápido)
│   ├── UseCases/
│   │   ├── GetItemUseCaseTests.cs
│   │   └── ProcessItemUseCaseTests.cs
│   ├── Models/
│   │   └── ItemTests.cs
│   └── Ports/
│       └── PortContractTests.cs
│
├── HexagonalLab.Infrastructure.Tests/    ← Integration Tests (Médio)
│   └── Repositories/
│       └── EfCoreRepositoryAdapterTests.cs
│
├── HexagonalLab.API.Tests/               ← E2E Tests (Lento)
│   └── Endpoints/
│       └── ItemEndpointsTests.cs
│
└── HexagonalLab.Worker.Tests/            ← E2E Tests (Worker)
    └── Services/
        └── ItemProcessingWorkerTests.cs

TEST PYRAMID:
        /\
       /  \         ← E2E (5% - 2-3 testes)
      /────\        ← Integration (15% - 10-15 testes)
     /      \       ← Unit (80% - 50+ testes)
    /________\
```

---

## 💻 Passo a Passo: Implementação

### T1: Comprehensive Unit Tests (Core)

#### **Arquivo: tests/HexagonalLab.Core.Tests/UseCases/ProcessItemUseCaseTests.cs** (expandido)

```csharp
namespace HexagonalLab.Core.Tests.UseCases;

using HexagonalLab.Core.Models;
using HexagonalLab.Core.Ports;
using HexagonalLab.Core.Tests.Adapters;
using HexagonalLab.Core.UseCases;
using Xunit;

/// <summary>
/// Unit Tests: ProcessItemUseCase (obtido em Phase 1)
/// 
/// ESTRUTURA:
/// ✅ Testa APENAS o UseCase
/// ✅ Usa In-Memory adapter (nenhuma dependência externa)
/// ✅ Testes verdadeiramente unitários
/// ✅ Rápidos (<100ms cada)
/// </summary>
public class ProcessItemUseCaseTests
{
    [Fact]
    public async Task ProcessAsync_WithValidItemId_ReturnsProcessedResponse()
    {
        // Arrange
        var useCase = new ProcessItemUseCase();
        var itemId = "ITEM-001";

        // Act
        var result = await useCase.ProcessAsync(itemId);

        // Assert - Comportamento documentado
        Assert.NotNull(result);
        Assert.Equal(itemId, result.ItemId);
        Assert.Equal("Processed", result.Status);
        Assert.True(result.ProcessedAt > DateTime.MinValue);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ProcessAsync_WithInvalidItemId_ThrowsArgumentException(string itemId)
    {
        // Arrange
        var useCase = new ProcessItemUseCase();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => useCase.ProcessAsync(itemId));
        Assert.Contains("cannot be empty", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProcessAsync_MultipleItems_ReturnsIndependentResults()
    {
        // Arrange
        var useCase = new ProcessItemUseCase();
        var itemIds = new[] { "ITEM-001", "ITEM-002", "ITEM-003" };

        // Act
        var results = await Task.WhenAll(
            itemIds.Select(id => useCase.ProcessAsync(id)));

        // Assert
        Assert.Equal(3, results.Length);
        Assert.All(results, r => Assert.Equal("Processed", r.Status));
    }
}
```

#### **Arquivo: tests/HexagonalLab.Core.Tests/UseCases/GetItemUseCaseTests.cs** (expandido)

```csharp
namespace HexagonalLab.Core.Tests.UseCases;

using HexagonalLab.Core.Models;
using HexagonalLab.Core.Tests.Adapters;
using HexagonalLab.Core.UseCases;
using Xunit;

/// <summary>
/// Unit Tests: GetItemUseCase (com mock adapter)
/// 
/// PONTO CRÍTICO:
/// ✅ UseCase usa Output Port (interface)
/// ✅ Port mockado com In-Memory adapter
/// ✅ Zero dependências externas
/// ✅ Testa: buscar item que existe, não existe, erro, etc.
/// </summary>
public class GetItemUseCaseTests
{
    [Fact]
    public async Task ProcessAsync_WithExistingItem_ReturnsItem()
    {
        // Arrange
        var repository = new InMemoryRepositoryAdapter();
        var item = new Item { Id = "ITEM-001", Name = "Test", Status = "Active" };
        await repository.SaveAsync(item);

        var useCase = new GetItemUseCase(repository);

        // Act
        var result = await useCase.ProcessAsync("ITEM-001");

        // Assert
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
        Assert.Equal("NotFound", result.Status);
        Assert.Equal("ITEM-999", result.ItemId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ProcessAsync_WithInvalidItemId_ThrowsArgumentException(string itemId)
    {
        // Arrange
        var repository = new InMemoryRepositoryAdapter();
        var useCase = new GetItemUseCase(repository);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => useCase.ProcessAsync(itemId));
    }

    [Fact]
    public void Constructor_WithNullRepository_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new GetItemUseCase(null!));
    }

    [Fact]
    public async Task ProcessAsync_WithMultipleItemsInRepository_ReturnsCorrectOne()
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
        Assert.Equal("Inactive", result.Status);
        Assert.Equal("Item 2", result.ItemId);  // Validar que é o correto
    }
}
```

#### **Arquivo: tests/HexagonalLab.Core.Tests/Ports/PortContractTests.cs** (novo)

```csharp
namespace HexagonalLab.Core.Tests.Ports;

using HexagonalLab.Core.Models;
using HexagonalLab.Core.Ports;
using HexagonalLab.Core.Tests.Adapters;
using Xunit;

/// <summary>
/// Port Contract Validation Tests
/// 
/// OBJETIVO: Validar que o contrato (interface) é implementado corretamente.
/// Se trocar adaptador no futuro, estes testes garantem que
/// o novo adapter segue o contrato.
/// </summary>
public class PortContractTests
{
    [Fact]
    public async Task IItemRepositoryPort_SaveAsync_PersistsItem()
    {
        // Arrange
        IItemRepositoryPort adapter = new InMemoryRepositoryAdapter();
        var item = new Item { Id = "TEST-001", Name = "Test", Status = "Active" };

        // Act
        await adapter.SaveAsync(item);

        // Assert
        var retrieved = await adapter.GetByIdAsync("TEST-001");
        Assert.NotNull(retrieved);
        Assert.Equal("TEST-001", retrieved.Id);
    }

    [Fact]
    public async Task IItemRepositoryPort_GetByIdAsync_ReturnsNullIfNotFound()
    {
        // Arrange
        IItemRepositoryPort adapter = new InMemoryRepositoryAdapter();

        // Act
        var result = await adapter.GetByIdAsync("ITEM-NOT-FOUND");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task IItemRepositoryPort_DeleteAsync_RemovesItem()
    {
        // Arrange
        IItemRepositoryPort adapter = new InMemoryRepositoryAdapter();
        var item = new Item { Id = "TEST-001", Name = "Test", Status = "Active" };
        await adapter.SaveAsync(item);

        // Act
        await adapter.DeleteAsync("TEST-001");

        // Assert
        var result = await adapter.GetByIdAsync("TEST-001");
        Assert.Null(result);
    }

    [Fact]
    public async Task IItemRepositoryPort_ExistsAsync_ValidatesExistence()
    {
        // Arrange
        IItemRepositoryPort adapter = new InMemoryRepositoryAdapter();
        var item = new Item { Id = "TEST-001", Name = "Test", Status = "Active" };

        // Act
        var existsBefore = await adapter.ExistsAsync("TEST-001");
        await adapter.SaveAsync(item);
        var existsAfter = await adapter.ExistsAsync("TEST-001");

        // Assert
        Assert.False(existsBefore);
        Assert.True(existsAfter);
    }
}
```

### T2: Integration Tests (Adapters)

#### **Arquivo: tests/HexagonalLab.Infrastructure.Tests/HexagonalLab.Infrastructure.Tests.csproj** (novo)

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="xunit" Version="2.6.0" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.0" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="8.0.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="../../src/HexagonalLab.Core/HexagonalLab.Core.csproj" />
    <ProjectReference Include="../../src/HexagonalLab.Infrastructure/HexagonalLab.Infrastructure.csproj" />
  </ItemGroup>

</Project>
```

#### **Arquivo: tests/HexagonalLab.Infrastructure.Tests/Repositories/EfCoreRepositoryAdapterTests.cs** (novo)

```csharp
namespace HexagonalLab.Infrastructure.Tests.Repositories;

using HexagonalLab.Core.Models;
using HexagonalLab.Infrastructure.Data;
using HexagonalLab.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

/// <summary>
/// Integration Tests: EF Core Repository Adapter
/// 
/// PONTO CRÍTICO:
/// ✅ Testa adapter com BANCO REAL (ou In-Memory EF)
/// ✅ Valida: queries, updates, deletes funcionam
/// ✅ Testa interactions com EF Core
/// ✅ MAS Core não é testado (apenas adapter)
/// </summary>
public class EfCoreRepositoryAdapterTests
{
    private AppDbContext CreateTestContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task SaveAsync_WithNewItem_InsertsIntoDatabase()
    {
        // Arrange
        using var context = CreateTestContext();
        var adapter = new EfCoreRepositoryAdapter(context);
        var item = new Item { Id = "INT-001", Name = "Integration Test", Status = "Active" };

        // Act
        await adapter.SaveAsync(item);

        // Assert
        var retrieved = await adapter.GetByIdAsync("INT-001");
        Assert.NotNull(retrieved);
        Assert.Equal("Integration Test", retrieved.Name);
    }

    [Fact]
    public async Task SaveAsync_WithExistingItem_Updates()
    {
        // Arrange
        using var context = CreateTestContext();
        var adapter = new EfCoreRepositoryAdapter(context);
        var item = new Item { Id = "INT-001", Name = "Original", Status = "Active" };

        // Act
        await adapter.SaveAsync(item);
        item.Name = "Updated";
        await adapter.SaveAsync(item);

        // Assert
        var retrieved = await adapter.GetByIdAsync("INT-001");
        Assert.Equal("Updated", retrieved!.Name);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllItems()
    {
        // Arrange
        using var context = CreateTestContext();
        var adapter = new EfCoreRepositoryAdapter(context);

        var items = new[]
        {
            new Item { Id = "INT-001", Name = "Item 1", Status = "Active" },
            new Item { Id = "INT-002", Name = "Item 2", Status = "Inactive" }
        };

        foreach (var item in items)
            await adapter.SaveAsync(item);

        // Act
        var retrieved = await adapter.GetAllAsync();

        // Assert
        Assert.Equal(2, retrieved.Count());
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        // Arrange
        using var context = CreateTestContext();
        var adapter = new EfCoreRepositoryAdapter(context);
        var item = new Item { Id = "INT-001", Name = "To Delete", Status = "Active" };

        // Act
        await adapter.SaveAsync(item);
        await adapter.DeleteAsync("INT-001");

        // Assert
        var result = await adapter.GetByIdAsync("INT-001");
        Assert.Null(result);
    }
}
```

### T3-T4: E2E + Coverage

```bash
# Run all tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# View results
opencover browse coverage.xml  # ou usar ferramenta de visualização
```

### T5: Document Test Pyramid

#### **Arquivo: DOC_IA/TEST_STRATEGY.md** (novo)

```markdown
# Test Strategy - HexagonalLab.NET10

## Test Pyramid

```
        /\
       /  \         E2E (5%)
      /────\        Integration (15%)
     /      \       Unit (80%)
    /________\
   /          \
```

## Detalhamento

### Unit Tests (80% - Rápido, isolado)
- **Projeto:** HexagonalLab.Core.Tests
- **Propósito:** Validar lógica de negócio isolada
- **Adapters:** In-Memory (fake)
- **Tempo:** ~1s para todo suite
- **Cobertura:** Core + Ports (contratos)

### Integration Tests (15% - Médio, com EF)
- **Projeto:** HexagonalLab.Infrastructure.Tests
- **Propósito:** Validar adapter implementação
- **Adapters:** EF Core In-Memory
- **Tempo:** ~3s para todo suite
- **Cobertura:** Repository adapter

### E2E Tests (5% - Lento, full flow)
- **Projetos:** HexagonalLab.API.Tests, HexagonalLab.Worker.Tests
- **Propósito:** Validar end-to-end flow
- **Adapters:** Real (ou simulado)
- **Tempo:** ~10s para todo suite
- **Cobertura:** HTTP + Worker + DB

## Coverage Goals

| Layer | Target | Atual |
|-------|--------|-------|
| Core | 90%+ | TBD |
| Adapters | 80%+ | TBD |
| API | 70%+ | TBD |
| Overall | 80%+ | TBD |

## Test Naming Convention

```
[MethodName]_[Condition]_[ExpectedResult]

Exemplo:
ProcessAsync_WithValidItemId_ReturnsProcessedStatus
GetByIdAsync_WithNonExistentItem_ReturnsNull
SaveAsync_WithNullItem_ThrowsArgumentNullException
```

## Test Organization

```
tests/
├── HexagonalLab.Core.Tests/
│   ├── UseCases/        ← Business logic tests
│   ├── Models/          ← DTO validation tests
│   └── Ports/           ← Contract validation tests
├── HexagonalLab.Infrastructure.Tests/
│   └── Repositories/    ← Adapter implementation tests
├── HexagonalLab.API.Tests/
│   └── Endpoints/       ← HTTP flow tests
└── HexagonalLab.Worker.Tests/
    └── Services/        ← Worker flow tests
```

## Running Tests

```bash
# Todos os testes
dotnet test

# Apenas unit tests (rápido)
dotnet test tests/HexagonalLab.Core.Tests/

# Com coverage
dotnet test /p:CollectCoverage=true

# Verbose
dotnet test --verbosity detailed
```
```

---

## ✅ Critério de Aceitação

Completar Phase 6 quando:

- [x] **Unit tests implementados**
  - ProcessItemUseCase tests
  - GetItemUseCase tests
  - Port contract tests
  - +10 testes no mínimo

- [x] **Integration tests implementados**
  - EF Core adapter tests
  - Database operations validadas

- [x] **E2E tests funcionando**
  - API endpoints tested
  - Worker flow tested

- [x] **Tests all passing**
  ```
  dotnet test
  ✅ 30+ tests passed
  ```

- [x] **Code coverage 80%+**
  - Core: 90%+
  - Adapters: 80%+

- [x] **Test strategy documentada**
  - Pyramid explained
  - Organization documented
  - Running instructions clear

---

## 🏛️ Decisões Arquiteturais

### Por que essa estrutura de tests?

```
UNIT (Core):
✅ Rápido (~1ms cada)
✅ Isolado (sem dependências)
✅ Executa em paralelo
✅ Valida lógica pura
✅ SEMPRE deve passar

INTEGRATION:
⚠️ Médio (~100ms cada)
⚠️ Depende de EF Core
⚠️ Válida adapter

E2E:
🐢 Lento (~1s cada)
🐢 Full stack
🐢 Poucas (não multiplicar)
🐢 Validam happy paths

REGRA DE OURO:
Para cada bug findado em production:
- 1 E2E test
- 5 integration tests
- 10 unit tests
```

---

## 📌 Próximos Passos (Phase 7)

Quando Phase 6 estiver ✅ **DONE**:

1. Atualizar Story 279 status → `Done`
2. Iniciar Story 275 (Phase 7 - Evolution)
3. Add novo adapter (cache, messaging)
4. Validar contra paper de Cockburn

---

**Status:** Ready for Implementation  
**Last Updated:** 2026-03-20  
**Author:** Architecture Team
