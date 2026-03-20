# 📌 PHASE 5: Multi-Adapter Pattern (Dia 5)

**Story ID:** 278  
**Azure DevOps Link:** [Story 278](https://dev.azure.com/alexestudocertificacoes/e699c50b-3ca9-45b5-ba7c-29591ee19071/web/wi.aspx?pcguid=&id=278)  
**Feature:** 270 - Input Adapters  
**Epic:** 268 - Hexagonal Architecture Lab  

---

## 🎯 Objetivo

Demonstrar **plugabilidade máxima** criando segundo Input Adapter (Worker):
- **Reutilizar MESMO UseCase** em contexto diferente
- Prova que Core é agnóstico de como é acionado
- Input pode ser API, Worker, CLI, gRPC etc.

### Resultado Esperado
- ✅ Projeto Worker (BackgroundService)
- ✅ Worker injetando UseCase
- ✅ **Zero código duplicado** (reutiliza Core)
- ✅ Múltiplos Inputs, mesma saída
- ✅ Prova isolamento do Core

---

## 📋 Tarefas Técnicas

| # | Descrição | Tipo | Duração Est. | Dependência |
|---|-----------|------|-------------|-------------|
| T1 | Create Worker Project | Adapter | 15 min | Phase 4 ✅ |
| T2 | Implement BackgroundService | Adapter | 20 min | T1 |
| T3 | Inject UseCase in Worker | Adapter | 15 min | T2 |
| T4 | Configure DI for Worker | Bootstrap | 15 min | T3 |
| T5 | Create Tests: API + Worker | Test | 20 min | T4 |

**Total Estimado:** ~1.5 horas  
**Bloqueador Anterior:** ✅ Phase 4 (DONE)

---

## 🏗️ Estrutura de Pastas Esperada

```
HexagonalLab.NET10/
│
├── src/
│   ├── HexagonalLab.Core/                 ← (ZERO changes)
│   │
│   ├── HexagonalLab.API/                  ← (Adapter Input #1)
│   │
│   ├── HexagonalLab.Infrastructure/       ← (Adapter Output - EF Core)
│   │
│   └── HexagonalLab.Worker/               ← ADAPTER INPUT #2 (novo!)
│       ├── HexagonalLab.Worker.csproj
│       │   ├── <PackageReference>Microsoft.Extensions.Hosting</PackageReference>
│       │
│       ├── Program.cs                     ← DI + Bootstrap
│       ├── appsettings.json
│       │
│       └── Services/
│           └── ItemProcessingWorker.cs    ← BackgroundService
│
└── tests/
    └── HexagonalLab.Worker.Tests/         ← (novo)
        └── Services/
            └── ItemProcessingWorkerTests.cs
```

---

## 💻 Passo a Passo: Implementação

### T1: Create Worker Project

```bash
# Criar projeto Worker (Hosted Service)
dotnet new worker -n HexagonalLab.Worker -o src/HexagonalLab.Worker --force

# Adicionar à solução
dotnet sln HexagonalLab.NET10.sln add src/HexagonalLab.Worker/HexagonalLab.Worker.csproj

# Adicionar referências
cd src/HexagonalLab.Worker
dotnet add reference ../HexagonalLab.Core/HexagonalLab.Core.csproj
dotnet add reference ../HexagonalLab.Infrastructure/HexagonalLab.Infrastructure.csproj

# Adicionar pacotes necessários
dotnet add package Microsoft.Extensions.Hosting
dotnet add package Microsoft.Extensions.Logging

cd ../..
```

### T2: Implement BackgroundService

#### **Arquivo: src/HexagonalLab.Worker/Services/ItemProcessingWorker.cs** (novo)

```csharp
namespace HexagonalLab.Worker.Services;

using HexagonalLab.Core.Ports;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

/// <summary>
/// Background Worker Service: Input Adapter #2
/// 
/// PADRÃO CRÍTICO:
/// ✅ BackgroundService (IHostedService)
/// ✅ Recebe IItemInputPort via DI (Input Port)
/// ✅ Não diferente de API Adapter - mesmo contrato!
/// ✅ Orquestra: "Pego evento → Chamo UseCase"
/// ✅ Core sem saber que é worker!
/// 
/// COMPARAÇÃO com API Adapter:
/// - API: HTTP Request → Input Port
/// - Worker: Timer/Event → Input Port
/// 
/// E... Core é 100% idêntico em ambos os casos!
/// </summary>
public class ItemProcessingWorker : BackgroundService
{
    private readonly ILogger<ItemProcessingWorker> _logger;
    private readonly IItemInputPort _useCase;
    private Timer? _timer;

    public ItemProcessingWorker(
        ILogger<ItemProcessingWorker> logger,
        IItemInputPort useCase)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _useCase = useCase ?? throw new ArgumentNullException(nameof(useCase));
    }

    /// <summary>
    /// Chamado na startup.
    /// Inicializa o timer aqui (não em constructor).
    /// </summary>
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("ItemProcessingWorker is starting...");

        // Timer: a cada 30 segundos, processa um item
        _timer = new Timer(
            async _ => await DoWork(cancellationToken),
            null,
            TimeSpan.Zero,
            TimeSpan.FromSeconds(30));

        await base.StartAsync(cancellationToken);
    }

    /// <summary>
    /// Implementa BackgroundService.ExecuteAsync() (opcional aqui).
    /// O timer já está fazendo o trabalho.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    /// <summary>
    /// A magia: Chama UseCase (Input Port) diretamente.
    /// NÃO diferente de um API endpoint!
    /// </summary>
    private async Task DoWork(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing items at {time}", DateTimeOffset.Now);

            // Simular: pegar lista de itens pendentes
            // (Na produção, consultaria um banco ou fila)
            var itemsToProcess = new[] { "ITEM-001", "ITEM-002", "ITEM-003" };

            foreach (var itemId in itemsToProcess)
            {
                try
                {
                    // ✅ CHAMA INPUT PORT (MESMO QUE FOR API!)
                    var result = await _useCase.ProcessAsync(itemId);

                    if (result.Status == "Processed")
                    {
                        _logger.LogInformation("Item {ItemId} processed successfully", itemId);
                    }
                    else
                    {
                        _logger.LogWarning("Item {ItemId} returned status: {Status}", itemId, result.Status);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing item {ItemId}", itemId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ItemProcessingWorker");
        }
    }

    /// <summary>
    /// Cleanup na shutdown.
    /// </summary>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("ItemProcessingWorker is stopping...");
        _timer?.Dispose();
        await base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        _timer?.Dispose();
        base.Dispose();
    }
}
```

### T3-T4: Configure DI for Worker

#### **Arquivo: src/HexagonalLab.Worker/Program.cs** (novo)

```csharp
using HexagonalLab.Core.Ports;
using HexagonalLab.Core.UseCases;
using HexagonalLab.Infrastructure.Data;
using HexagonalLab.Infrastructure.Repositories;
using HexagonalLab.Worker.Services;
using Microsoft.EntityFrameworkCore;

// ========================================================================
// BOOTSTRAP - Worker Service
// ========================================================================
// NOTA: Mesma DI configuration que API!
// Apenas adiciona: AddHostedService<ItemProcessingWorker>
// ========================================================================

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // ─────────────────────────────────────────────────────────────
        // 1. Database (mesmo que API)
        // ─────────────────────────────────────────────────────────────
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(context.Configuration.GetConnectionString("DefaultConnection")));

        // ─────────────────────────────────────────────────────────────
        // 2. Core UseCase (Input Port)
        // ─────────────────────────────────────────────────────────────
        services.AddScoped<IItemInputPort, GetItemUseCase>();

        // ─────────────────────────────────────────────────────────────
        // 3. Output Port Adapter (EF Core)
        // ─────────────────────────────────────────────────────────────
        services.AddScoped<IItemRepositoryPort, EfCoreRepositoryAdapter>();

        // ─────────────────────────────────────────────────────────────
        // 4. WORKER-SPECIFIC: RegisterBackgroundService
        // ─────────────────────────────────────────────────────────────
        services.AddHostedService<ItemProcessingWorker>();

        // ─────────────────────────────────────────────────────────────
        // 5. Logging
        // ─────────────────────────────────────────────────────────────
        services.AddLogging(config =>
        {
            config.AddConsole();
            config.SetMinimumLevel(LogLevel.Information);
        });
    })
    .Build();

// Criar banco na startup (como em API)
using (var scope = host.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
}

await host.RunAsync();
```

#### **Arquivo: src/HexagonalLab.Worker/appsettings.json**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=HexagonalLab;Trusted_Connection=true;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

### T5: Create Tests

#### **Arquivo: tests/HexagonalLab.Worker.Tests/HexagonalLab.Worker.Tests.csproj** (novo)

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
    <PackageReference Include="Moq" Version="4.20.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="../../src/HexagonalLab.Core/HexagonalLab.Core.csproj" />
    <ProjectReference Include="../../src/HexagonalLab.Worker/HexagonalLab.Worker.csproj" />
    <ProjectReference Include="../../tests/HexagonalLab.Core.Tests/HexagonalLab.Core.Tests.csproj" />
  </ItemGroup>

</Project>
```

#### **Arquivo: tests/HexagonalLab.Worker.Tests/Services/ItemProcessingWorkerTests.cs** (novo)

```csharp
namespace HexagonalLab.Worker.Tests.Services;

using HexagonalLab.Core.Models;
using HexagonalLab.Core.Ports;
using HexagonalLab.Worker.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

/// <summary>
/// Unit tests: ItemProcessingWorker
/// 
/// PONTO CRÍTICO:
/// ✅ Testa que Worker chama Input Port
/// ✅ Valida que Core é reutilizado
/// ✅ E prova que múltiplos adapters funcionam identicamente
/// </summary>
public class ItemProcessingWorkerTests
{
    [Fact]
    public async Task ExecuteAsync_CallsInputPort()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ItemProcessingWorker>>();
        var mockUseCase = new Mock<IItemInputPort>();

        mockUseCase
            .Setup(x => x.ProcessAsync(It.IsAny<string>()))
            .ReturnsAsync(new ItemResponse
            {
                ItemId = "ITEM-001",
                Status = "Processed",
                ProcessedAt = DateTime.UtcNow
            });

        var worker = new ItemProcessingWorker(mockLogger.Object, mockUseCase.Object);

        // Act
        await worker.StartAsync(CancellationToken.None);

        // Aguardar um tick do timer
        await Task.Delay(1000);

        // Assert
        // Worker deve ter chamado UseCase pelo menos uma vez
        mockUseCase.Verify(
            x => x.ProcessAsync(It.IsAny<string>()),
            Times.AtLeastOnce);

        // Cleanup
        await worker.StopAsync(CancellationToken.None);
    }

    [Fact]
    public void Constructor_WithNullUseCase_ThrowsArgumentNullException()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ItemProcessingWorker>>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ItemProcessingWorker(mockLogger.Object, null!));
    }
}
```

### Build and Run

```bash
# Adicionar projeto de testes à solução
dotnet sln HexagonalLab.NET10.sln add tests/HexagonalLab.Worker.Tests/HexagonalLab.Worker.Tests.csproj

# Build
dotnet build HexagonalLab.NET10.sln

# Rodar tests
dotnet test

# (Opcional) Rodar worker
cd src/HexagonalLab.Worker
dotnet run
# Output:
# [INF] ItemProcessingWorker is starting...
# [INF] Processing items at ...
# [INF] Item ITEM-001 processed successfully
```

---

## ✅ Critério de Aceitação

Completar Phase 5 quando:

- [x] **Worker project criado**
  - BackgroundService implementado
  - Zero código duplicado do Core

- [x] **Worker reutiliza UseCase**
  - Mesmo IItemInputPort que API
  - Sem mudança em Core

- [x] **DI configurado identicamente**
  - Setup.cs parecido com API
  - Services registrados igualmente

- [x] **Testes passam**
  ```
  dotnet test
  ✅ Todos os testes, incluindo Worker
  ```

- [x] **Multi-adapter validado**
  - API: HTTP → Core
  - Worker: Timer → Core
  - Ambos usam mesmo UseCase e Output Port

---

## 🏛️ Decisões Arquiteturais

### A Magia de Múltiplas Entradas

```
INPUT PORTS (Múltiplas entradas):
┌─────────────────────────────────┐
│  API (HTTP)                     │
│    endpoint: GET /items/{id}    │
│    ↓                            │
│  IItemInputPort (abstração)     │
│    ↑ (reutilizado!)            │
│  Worker (Timer)                 │
│    timer: a cada 30s            │
└─────────────────────────────────┘
        ↓↓↓ (Mesma implementação)

Core (GetItemUseCase):
├─ Não sabe se vem de HTTP ou Timer
├─ Não sabe de UI ou plataforma
├─ Apenas: recebo Port → processo → retorno


OUTPUT PORTS (Mesma saída):
┌─────────────────────────────────┐
│  IItemRepositoryPort (EF Core)  │
│    → SQL Server                 │
│    → Dados persistidos          │
└─────────────────────────────────┘
```

### Reutilização = Sucesso Arquitetural

```
PHASE 3-4 (API Adapter):
- Código: ~200 linhas (Program.cs + Endpoints)
- UseCase: GetItemUseCase
- Output: EfCoreRepositoryAdapter

PHASE 5 (Worker Adapter):
- Código: ~200 linhas (Program.cs + Worker)
- UseCase: MESMO GetItemUseCase ← Reutilizado!
- Output: MESMA EfCoreRepositoryAdapter ← Reutilizado!

RESULTADO: Zero duplicação de lógica!
```

---

## 🚨 Possíveis Problemas

### Problema 1: "Worker e API compartilham mesmo DbContext"
**Resposta:** Correto! Ambos acessam mesma Base. Apenas abstração (Port) importa.

### Problema 2: "Timer processing é lento"
**Resposta:** Correto para demo! Produção: usar Background Job (Hangfire/Quartz), mas padrão seria idêntico.

### Problema 3: "Worker nunca termina"
**Resposta:** É esperado! BackgroundService roda enquanto app roda. Parar com Ctrl+C.

---

## 📌 Próximos Passos (Phase 6)

Quando Phase 5 estiver ✅ **DONE**:

1. Atualizar Story 278 status → `Done`
2. Iniciar Story 279 (Phase 6 - Testing Strategy)
3. Comprehensive testing
4. Validar isolamento do Core

---

**Status:** Ready for Implementation  
**Last Updated:** 2026-03-20  
**Author:** Architecture Team
