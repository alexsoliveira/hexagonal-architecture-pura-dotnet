# 📌 PHASE 1: Understand Inside vs Outside (Dia 1)

**Story ID:** 276  
**Azure DevOps Link:** [Story 276](https://dev.azure.com/alexestudocertificacoes/e699c50b-3ca9-45b5-ba7c-29591ee19071/web/wi.aspx?pcguid=&id=276)  
**Feature:** 269 - Core Application  
**Epic:** 268 - Hexagonal Architecture Lab  

---

## 🎯 Objetivo

Criar a **solução base** com a **estrutura fundamental do Core** e implementar o **primeiro UseCase + Input Port** sem dependências externas.

### Resultado Esperado
- ✅ Solução .NET 10 compilando
- ✅ Primeiro UseCase rodando isolado
- ✅ Input Port definido
- ✅ Testes unitários passando **sem banco de dados**
- ✅ **Zero dependências** de frameworks no Core

---

## 📋 Tarefas Técnicas

| # | Descrição | Tipo | Duração Est. | Dependência |
|---|-----------|------|-------------|-------------|
| T1 | Create .NET 10 Solution + Projects | Core | 15 min | — |
| T2 | Create Core Project Structure | Core | 20 min | T1 |
| T3 | Implement First UseCase | Core | 25 min | T2 |
| T4 | Define Input Port Interface | Core | 15 min | T2 |
| T5 | Create Domain Models (DTOs) | Core | 20 min | T2 |
| T6 | Create Unit Test Project (xUnit) | Test | 15 min | T1 |
| T7 | Validate Core runs without DB/API | Test | 20 min | T3-T5 |

**Total Estimado:** ~2 horas  
**Bloqueador Anterior:** ❌ Nenhum (é a primeira fase)

---

## 🏗️ Estrutura de Pastas Esperada

```
HexagonalLab.NET10/
├── HexagonalLab.NET10.sln
├── global.json (SDK version 10.0+)
│
├── src/
│   └── HexagonalLab.Core/
│       ├── HexagonalLab.Core.csproj
│       │   ├── <PackageReference> ❌ NONE (Zero dependencies!)
│       │
│       ├── Ports/
│       │   └── IItemInputPort.cs        ← Input Port (interface)
│       │
│       ├── UseCases/
│       │   └── ProcessItemUseCase.cs    ← First UseCase
│       │
│       └── Models/
│           ├── ItemRequest.cs           ← Input DTO
│           └── ItemResponse.cs          ← Output DTO
│
└── tests/
    └── HexagonalLab.Core.Tests/
        ├── HexagonalLab.Core.Tests.csproj
        │   ├── <PackageReference>xunit</PackageReference>
        │   ├── <PackageReference>xunit.runner.visualstudio</PackageReference>
        │   └── <ProjectReference>../src/HexagonalLab.Core/...</ProjectReference>
        │
        └── UseCases/
            └── ProcessItemUseCaseTests.cs ← Unit tests
```

---

## 💻 Passo a Passo: Implementação

### T1: Create Solution

```bash
# Navegar para o repositório
cd "e:\Documents\Repositorios_GitHub\Hexagonal Architecture .NET Pura"

# Criar global.json para especificar SDK
dotnet new globaljson --sdk-version 10.0.0 --roll-forward latestMinor --force

# Criar solução
dotnet new sln -n HexagonalLab.NET10
```

### T2: Create Core Project

```bash
# Criar Core project (Class Library - NO dependencies!)
dotnet new classlib -n HexagonalLab.Core -o src/HexagonalLab.Core --force
cd src/HexagonalLab.Core

# ⚠️ IMPORTANTE: Verificar que .csproj tem 0 PackageReference
# (abrir HexagonalLab.Core.csproj e confirmar)

cd ../..

# Adicionar à solução
dotnet sln HexagonalLab.NET10.sln add src/HexagonalLab.Core/HexagonalLab.Core.csproj
```

### T3-T5: Implement Ports, UseCases, Models

#### **Arquivo: IItemInputPort.cs** (Input Port)

```csharp
namespace HexagonalLab.Core.Ports;

/// <summary>
/// Input Port - Define o contrato de entrada para o UseCase.
/// Não depende de nada externo (sem HTTP, sem banco, sem frameworks).
/// </summary>
public interface IItemInputPort
{
    /// <summary>
    /// Processa um item identificado pelo ID.
    /// </summary>
    /// <param name="itemId">ID do item a ser processado</param>
    /// <returns>Resultado do processamento</returns>
    Task<ItemResponse> ProcessAsync(string itemId);
}
```

#### **Arquivo: Models/ItemRequest.cs**

```csharp
namespace HexagonalLab.Core.Models;

/// <summary>
/// DTO simples para requisição de processamento.
/// Sem validações complexas, sem frameworks - apenas dados.
/// </summary>
public class ItemRequest
{
    public string ItemId { get; set; } = string.Empty;
}
```

#### **Arquivo: Models/ItemResponse.cs**

```csharp
namespace HexagonalLab.Core.Models;

/// <summary>
/// DTO para resposta do UseCase.
/// Contém resultado e metadados do processamento.
/// </summary>
public class ItemResponse
{
    public string ItemId { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public DateTime ProcessedAt { get; set; }
    public string Message { get; set; } = string.Empty;
}
```

#### **Arquivo: UseCases/ProcessItemUseCase.cs** (First UseCase)

```csharp
namespace HexagonalLab.Core.UseCases;

using HexagonalLab.Core.Models;
using HexagonalLab.Core.Ports;

/// <summary>
/// UseCase: Processa um item.
/// 
/// CARACTERÍSTICAS CRÍTICAS:
/// ✅ Zero dependências de framework
/// ✅ Recebe Port via constructor (Dependency Injection)
/// ✅ Pode ser testado em memória (sem DB)
/// ✅ Implementa Input Port (interface)
/// ✅ Isola lógica de negócio
/// </summary>
public class ProcessItemUseCase : IItemInputPort
{
    private readonly ILogger<ProcessItemUseCase>? _logger;

    /// <summary>
    /// Constructor - Recebe PORT via DI (mas pode ser null para fase 1).
    /// Assim, prova que não sabemos da existência de adapters.
    /// </summary>
    public ProcessItemUseCase(ILogger<ProcessItemUseCase>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Implementa Input Port.
    /// Lógica de negócio: "Processar um item".
    /// </summary>
    public Task<ItemResponse> ProcessAsync(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
            throw new ArgumentException("Item ID cannot be empty", nameof(itemId));

        _logger?.LogInformation("Processing item {ItemId}", itemId);

        var response = new ItemResponse
        {
            ItemId = itemId,
            Status = "Processed",           // ← Lógica simples de negócio
            ProcessedAt = DateTime.UtcNow,
            Message = $"Item {itemId} processed successfully"
        };

        return Task.FromResult(response);
    }
}

/// <summary>
/// SimpleLogger - Para Phase 1, não usamos ILogger real. 
/// Apenas mock mínimo, pois Core não pode depender de logging framework.
/// </summary>
public interface ILogger<out T>
{
    void LogInformation(string message, params object[] args);
}
```

### T6: Create Test Project

```bash
# Criar projeto de testes
dotnet new xunit -n HexagonalLab.Core.Tests -o tests/HexagonalLab.Core.Tests --force

# Adicionar à solução
dotnet sln HexagonalLab.NET10.sln add tests/HexagonalLab.Core.Tests/HexagonalLab.Core.Tests.csproj

# Adicionar referência do teste ao Core
cd tests/HexagonalLab.Core.Tests
dotnet add reference ../../src/HexagonalLab.Core/HexagonalLab.Core.csproj
cd ../..
```

### T7: Create and Run Unit Tests

#### **Arquivo: tests/HexagonalLab.Core.Tests/UseCases/ProcessItemUseCaseTests.cs**

```csharp
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
```

### Build and Test

```bash
# Build da solução
dotnet build HexagonalLab.NET10.sln

# Rodar testes (COMPORTAMENTO CRÍTICO: Nenhum banco é necessário!)
dotnet test tests/HexagonalLab.Core.Tests/ --verbosity detailed

# Output esperado:
# ✅ ProcessItemUseCaseTests.ProcessAsync_WithValidItemId_ReturnsProcessedStatus PASSED
# ✅ ProcessItemUseCaseTests.ProcessAsync_WithInvalidItemId_ThrowsArgumentException PASSED (3 items)
# ✅ ProcessItemUseCaseTests.ProcessAsync_WithMultipleItems_ReturnsIndependentResults PASSED
# 
# Test run successful: 3 passed, 0 failed
```

---

## ✅ Critério de Aceitação

Completar Phase 1 quando:

- [x] **Solução compilada** sem erros
  ```bash
  dotnet build --configuration Release
  # Should return: "Build succeeded"
  ```

- [x] **Zero PackageReference no Core**
  ```xml
  <!-- HexagonalLab.Core.csproj deve ter 0 <PackageReference> -->
  <Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
      <TargetFramework>net10.0</TargetFramework>
    </PropertyGroup>
    <!-- ✅ Vazio - sem dependencies! -->
  </Project>
  ```

- [x] **Testes passando**
  ```
  dotnet test
  ✅ 3 passed
  ```

- [x] **UseCase testado isolado**
  - Sem banco de dados
  - Sem API
  - Sem frameworks
  - Puro em memória

- [x] **Estrutura de pastas correta**
  - `src/HexagonalLab.Core/` ← Core
  - `tests/HexagonalLab.Core.Tests/` ← Tests
  - `Ports/` ← Interfaces
  - `UseCases/` ← Lógica
  - `Models/` ← DTOs

---

## 🏛️ Decisões Arquiteturais

### Por que começar aqui?

| Decisão | Justificativa |
|---------|---------------|
| **Sem dependências no Core** | Alistair Cockburn §2: "The innermost layer must be free of frameworks" |
| **Input Port como interface** | Contrato claro = facilita adapters posteriores |
| **UseCase simples** | Prova que lógica de negócio é separada de adaptadores |
| **Testes em memória** | Valida que Core não depende de infraestrutura |
| **DTO simples (Request/Response)** | Sem entidades complexas (DDD não é permitido) |

### Princípios Aplicados

```
✅ Dependency Inversion Principle
   UseCase recebe Port via constructor (abstração)

✅ Single Responsibility
   UseCase = ProcessarItem (apenas)
   Port = Contrato (apenas)
   Models = Dados (apenas)

✅ No Framework Dependencies
   Core usa apenas C# standard library

✅ Testable in Isolation
   Sem banco, sem mocks complexos, sem setup
```

---

## 🚨 Possíveis Problemas e Soluções

### Problema 1: "Core project depende de EF Core"
**Solução:** Remover qualquer `using EntityFrameworkCore;` do Core. Se necessário, mover para Adapter layer.

### Problema 2: "Testes precisam de banco"
**Solução:** UseCase **não deve** fazer queries. Apenas lógica. Queries vêm da Output Port (Phase 2).

### Problema 3: "UseCase está muito simples"
**Resposta:** Correto! Phase 1 é propositalmente simples. Complexidade vem nas fases seguintes (Output Port, Adapters, etc).

### Problema 4: .NET version incompatível
**Solução:**
```bash
dotnet --version  # Deve ser 10.0+
# Se não for, instalar 10 SDK:
# https://dotnet.microsoft.com/download
```

---

## 📌 Próximos Passos (Phase 2)

Quando Phase 1 estiver ✅ **DONE**:

1. Atualizar Story 276 status → `Done` (MCP)
2. Iniciar Story 273 (Phase 2 - Output Port Pattern)
3. Criar Output Port interface
4. Implementar In-Memory adapter

---

## 🔗 Referências

- [Alistair Cockburn - Hexagonal Architecture](https://alistair.cockburn.us/hexagonal-architecture/)
- [Microsoft .NET 10 Docs](https://learn.microsoft.com/en-us/dotnet/)
- [xUnit Testing Framework](https://xunit.net/)

---

**Status:** Ready for Implementation  
**Last Updated:** 2026-03-20  
**Author:** Architecture Team
