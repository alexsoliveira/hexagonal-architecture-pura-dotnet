# 📌 PHASE 3: Connect API Adapter (Dia 3)

**Story ID:** 277  
**Azure DevOps Link:** [Story 277](https://dev.azure.com/alexestudocertificacoes/e699c50b-3ca9-45b5-ba7c-29591ee19071/web/wi.aspx?pcguid=&id=277)  
**Feature:** 270 - Input Adapters  
**Epic:** 268 - Hexagonal Architecture Lab  

---

## 🎯 Objetivo

Criar **primeiro Input Adapter (API HTTP)** que:
- Traduz requisições HTTP → Input Port
- Injeta UseCases via DI
- Prova que Core funciona com externos

### Resultado Esperado
- ✅ Projeto Web API criado
- ✅ Endpoints mapeados para Input Ports
- ✅ HTTP e-2-e com In-Memory adapter
- ✅ Core **inalterado** (prova isolation)
- ✅ API não conhece detalhes de implementação

---

## 📋 Tarefas Técnicas

| # | Descrição | Tipo | Duração Est. | Dependência |
|---|-----------|------|-------------|-------------|
| T1 | Create Web API Project | Adapter | 15 min | Phase 2 ✅ |
| T2 | Create Input Port Interface | Core | 15 min | Phase 1 ✅ |
| T3 | Create Minimal API Endpoints | Adapter | 25 min | T1-T2 |
| T4 | Configure DI Container | Bootstrap | 20 min | T3 |
| T5 | Create E2E Integration Tests | Test | 25 min | T4 |

**Total Estimado:** ~1.5 horas  
**Bloqueador Anterior:** ✅ Phase 2 (DONE)

---

## 🏗️ Estrutura de Pastas Esperada

```
HexagonalLab.NET10/
│
├── src/
│   ├── HexagonalLab.Core/
│   │   ├── Ports/
│   │   │   ├── IItemInputPort.cs          ← (já existe)
│   │   │   └── IItemRepositoryPort.cs     ← (já existe)
│   │   └── ...
│   │
│   └── HexagonalLab.API/                  ← INPUT ADAPTER (novo!)
│       ├── HexagonalLab.API.csproj
│       │   ├── <PackageReference>Microsoft.AspNetCore.App</PackageReference>
│       │
│       ├── Program.cs                     ← DI Configuration
│       ├── Endpoints/
│       │   └── ItemEndpoints.cs           ← Minimal API routes
│       │
│       └── Middlewares/ (opcional)
│           └── ErrorHandlingMiddleware.cs
│
└── tests/
    └── HexagonalLab.API.Tests/            ← Integration Tests (novo!)
        ├── HexagonalLab.API.Tests.csproj
        ├── Fixtures/
        │   └── WebApplicationFactory.cs
        └── Endpoints/
            └── ItemEndpointsTests.cs
```

---

## 💻 Passo a Passo: Implementação

### T1: Create Web API Project

```bash
# Criar Web API project
dotnet new webapi -n HexagonalLab.API -o src/HexagonalLab.API --force

# Adicionar à solução
dotnet sln HexagonalLab.NET10.sln add src/HexagonalLab.API/HexagonalLab.API.csproj

# Adicionar referência: API → Core
cd src/HexagonalLab.API
dotnet add reference ../HexagonalLab.Core/HexagonalLab.Core.csproj
cd ../..
```

### T2: Input Port (já feito em Phase 1, confirmar)

```csharp
// Já existe: src/HexagonalLab.Core/Ports/IItemInputPort.cs
// Deve ter:
namespace HexagonalLab.Core.Ports;

public interface IItemInputPort
{
    Task<ItemResponse> ProcessAsync(string itemId);
}
```

### T3: Create Minimal API Endpoints

#### **Arquivo: src/HexagonalLab.API/Endpoints/ItemEndpoints.cs** (novo)

```csharp
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
            .WithName("Items")
            .WithOpenApi();

        group.MapGet("/{id}", GetItem)
            .WithName("GetItem")
            .WithOpenApi();

        group.MapPost("/{id}/process", ProcessItem)
            .WithName("ProcessItem")
            .WithOpenApi();

        group.MapGet("/", GetAllItems)
            .WithName("GetAllItems")
            .WithOpenApi();
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
```

### T4: Configure Dependency Injection

#### **Arquivo: src/HexagonalLab.API/Program.cs** (novo)

```csharp
using HexagonalLab.API.Endpoints;
using HexagonalLab.Core.Ports;
using HexagonalLab.Core.UseCases;

// ========================================================================
// BOOTSTRAP - Dependency Injection Configuration
// ========================================================================
// NOTA: Este é o ÚNICO lugar onde DI é configurado!
// Se trocar adapter: apenas AQUI muda, nunca no Core.
// ========================================================================

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────────────────────────────
// 1. Add API Services
// ─────────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// ─────────────────────────────────────────────────────────────────
// 2. Register Core UseCases (Input Ports)
// ─────────────────────────────────────────────────────────────────
// phase 3: Usando In-Memory adapter ainda
// Phase 4: Trocar para EF Core adapter (UMA LINHA SÓ!)
// ─────────────────────────────────────────────────────────────────

builder.Services.AddScoped<IItemInputPort, GetItemUseCase>();

// ─────────────────────────────────────────────────────────────────
// 3. Register Output Port Adapters
// ─────────────────────────────────────────────────────────────────
// FASE 3 (Atual):  In-Memory Adapter (teste)
// FASE 4 (Próxima): EF Core Adapter (real)
// ─────────────────────────────────────────────────────────────────

// FASE 3 (In-Memory - Temporary for testing):
using var scope = builder.Services.BuildServiceProvider().CreateScope();
var inMemoryAdapter = new HexagonalLab.Core.Tests.Adapters.InMemoryRepositoryAdapter();
// TODO: Seed dados de teste aqui (optional)
builder.Services.AddSingleton<IItemRepositoryPort>(inMemoryAdapter);

// FASE 4 (Uncomment depois):
// builder.Services.AddScoped<IItemRepositoryPort, EfCoreRepositoryAdapter>();
// builder.Services.AddDbContext<AppDbContext>();

// ─────────────────────────────────────────────────────────────────
// 4. Build App
// ─────────────────────────────────────────────────────────────────

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();

// ─────────────────────────────────────────────────────────────────
// 5. Map Minimal API Endpoints
// ─────────────────────────────────────────────────────────────────

app.MapItemEndpoints();  // Registra endpoints

// ─────────────────────────────────────────────────────────────────

app.Run();
```

### T5: Create E2E Integration Tests

#### **Arquivo: tests/HexagonalLab.API.Tests/HexagonalLab.API.Tests.csproj** (novo)

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="10.0.0" />
    <PackageReference Include="xunit" Version="2.6.0" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.0" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="../../src/HexagonalLab.API/HexagonalLab.API.csproj" />
    <ProjectReference Include="../../src/HexagonalLab.Core/HexagonalLab.Core.csproj" />
    <ProjectReference Include="../../tests/HexagonalLab.Core.Tests/HexagonalLab.Core.Tests.csproj" />
  </ItemGroup>

</Project>
```

#### **Arquivo: tests/HexagonalLab.API.Tests/Fixtures/WebApplicationFactory.cs** (novo)

```csharp
namespace HexagonalLab.API.Tests.Fixtures;

using HexagonalLab.API;
using HexagonalLab.Core.Models;
using HexagonalLab.Core.Ports;
using HexagonalLab.Core.Tests.Adapters;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Custom WebApplicationFactory para E2E tests.
/// Configura API com In-Memory adapter para testes.
/// </summary>
public class ItemApiWebApplicationFactory : WebApplicationFactory<Program>
{
    public InMemoryRepositoryAdapter Repository { get; private set; } = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove real adapter (se houvesse)
            var descriptor = services.FirstOrDefault(
                d => d.ServiceType == typeof(IItemRepositoryPort));
            if (descriptor != null)
                services.Remove(descriptor);

            // Criar In-Memory adapter compartilhado
            Repository = new InMemoryRepositoryAdapter();
            services.AddSingleton<IItemRepositoryPort>(Repository);
        });
    }
}
```

#### **Arquivo: tests/HexagonalLab.API.Tests/Endpoints/ItemEndpointsTests.cs** (novo)

```csharp
namespace HexagonalLab.API.Tests.Endpoints;

using HexagonalLab.API.Tests.Fixtures;
using HexagonalLab.Core.Models;
using System.Net;
using Xunit;

/// <summary>
/// E2E Integration Tests: Item Endpoints
/// 
/// CRÍTICO:
/// ✅ Testa API contra In-Memory adapter
/// ✅ Prova end-to-end HTTP flow
/// ✅ Valida serialization/deserialization
/// ✅ Core nunca foi alterado
/// </summary>
public class ItemEndpointsTests : IClassFixture<ItemApiWebApplicationFactory>
{
    private readonly ItemApiWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ItemEndpointsTests(ItemApiWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetItem_WithExistingItem_ReturnsOkAndItem()
    {
        // Arrange
        var item = new Item { Id = "ITEM-001", Name = "Test Item", Status = "Active" };
        await _factory.Repository.SaveAsync(item);

        // Act
        var response = await _client.GetAsync("/api/items/ITEM-001");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsAsync<ItemResponse>();
        Assert.NotNull(content);
        Assert.Equal("ITEM-001", content.ItemId);
    }

    [Fact]
    public async Task GetItem_WithNonExistentItem_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/items/ITEM-999");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsAsync<ItemResponse>();
        Assert.Equal("NotFound", content.Status);
    }

    [Fact]
    public async Task ProcessItem_WithValidId_ReturnsOk()
    {
        // Arrange
        var item = new Item { Id = "ITEM-001", Name = "Test", Status = "Pending" };
        await _factory.Repository.SaveAsync(item);

        // Act
        var response = await _client.PostAsync("/api/items/ITEM-001/process", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetItem_WithInvalidId_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/items/");

        // Assert
        // 404 ou 400 dependendo da rota
        Assert.True(response.StatusCode == HttpStatusCode.NotFound ||
                   response.StatusCode == HttpStatusCode.BadRequest);
    }
}
```

### Build and Run

```bash
# Add projeto de testes à solução
dotnet sln HexagonalLab.NET10.sln add tests/HexagonalLab.API.Tests/HexagonalLab.API.Tests.csproj

# Build tudo
dotnet build HexagonalLab.NET10.sln

# Rodar API tests
dotnet test tests/HexagonalLab.API.Tests/ --verbosity detailed

# (Opcional) Rodar API localmente
cd src/HexagonalLab.API
dotnet run
# Acessar: https://localhost:5001/api/items/ITEM-001
```

---

## ✅ Critério de Aceitação

Completar Phase 3 quando:

- [x] **Projeto Web API criado**
  - Mínimo: Program.cs + Endpoints

- [x] **Endpoints mapeados**
  - GET /api/items/{id}
  - POST /api/items/{id}/process
  - Todos recebem Port via DI

- [x] **DI configurado**
  - Input Port: GetItemUseCase
  - Output Port: InMemoryRepositoryAdapter
  - Apenas em Program.cs

- [x] **E2E tests passam**
  ```
  dotnet test tests/HexagonalLab.API.Tests/
  ✅ 4+ passed
  ```

- [x] **HTTP flow validado**
  - Request → Adapter → Core → Response
  
- [x] **Core inalterado**
  - Zero mudanças em HexagonalLab.Core projeto

---

## 🏛️ Decisões Arquiteturais

### Input vs Output Port

```
INPUT PORT (Adapter de Entrada):
┌─────────────────────────────────────────┐
│ POST /api/items/process                 │
├─────────────────────────────────────────┤
│ API Endpoint (Adapter externo)          │
│    ↓                                    │
│ IItemInputPort (Interface em Core)      │
│    ↓                                    │
│ GetItemUseCase (Implementação em Core)  │
└─────────────────────────────────────────┘
        ↓↓↓ (Executa) ↓↓↓

OUTPUT PORT (Adapter de Saída):
┌─────────────────────────────────────────┐
│ IItemRepositoryPort (Interface em Core) │
│    ↓                                    │
│ InMemoryRepositoryAdapter (Fake)        │
│ ou EfCoreRepositoryAdapter (Real)       │
└─────────────────────────────────────────┘
```

### Por que Minimal API?

✅ Moderno, leve, sem boilerplate  
✅ Fácil de entender padrão Ports & Adapters  
✅ Perfeito para laboratório  

---

## 🚨 Possíveis Problemas

### Problema 1: "API depende de Core.Tests"
**Solução:** É temporário. Phase 4 move InMemoryAdapter para packages reais.

### Problema 2: "HttpClient é complicado"
**Resposta:** Use `factory.CreateClient()` do WebApplicationFactory. Simples!

### Problema 3: "Endpoints recebem Port, mas nunca é null?"
**Resposta:** Correto! ASP.NET DI garante. Se for null, error em startup.

---

## 📌 Próximos Passos (Phase 4)

Quando Phase 3 estiver ✅ **DONE**:

1. Atualizar Story 277 status → `Done`
2. Iniciar Story 274 (Phase 4 - Real Adapter)
3. Criar projeto Infrastructure com EF Core
4. Substituir In-Memory por EF real (1 linha em Program.cs!)

---

**Status:** Ready for Implementation  
**Last Updated:** 2026-03-20  
**Author:** Architecture Team
