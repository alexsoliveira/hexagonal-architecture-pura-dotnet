# 📌 PHASE 4: Implement Real Adapter (Dia 4)

**Story ID:** 274  
**Azure DevOps Link:** [Story 274](https://dev.azure.com/alexestudocertificacoes/e699c50b-3ca9-45b5-ba7c-29591ee19071/web/wi.aspx?pcguid=&id=274)  
**Feature:** 271 - Output Adapters  
**Epic:** 268 - Hexagonal Architecture Lab  

---

## 🎯 Objetivo

**Substituir Fake Adapter por implementação real (EF Core)** demonstrando:
- Output Port agnóstico de implementação
- Troca de adapter sem alterar Core
- DI como ponto único de mudança

### Resultado Esperado
- ✅ Projeto Infrastructure com EF Core
- ✅ DbContext + Entity Mapping
- ✅ Real Repository Adapter implementado
- ✅ API funcionando com banco real
- ✅ Core **100% inalterado**

---

## 📋 Tarefas Técnicas

| # | Descrição | Tipo | Duração Est. | Dependência |
|---|-----------|------|-------------|-------------|
| T1 | Create Infrastructure Project | Adapter | 15 min | Phase 3 ✅ |
| T2 | Create DbContext + Entity Map | Adapter | 20 min | T1 |
| T3 | Implement EF Repository Adapter | Adapter | 25 min | T2 |
| T4 | Create Database Migrations | Adapter | 15 min | T3 |
| T5 | Update DI: Trocar Adapter | Bootstrap | 10 min | T4 |
| T6 | E2E Test com Real DB | Test | 20 min | T5 |

**Total Estimado:** ~1.5 horas  
**Bloqueador Anterior:** ✅ Phase 3 (DONE)

---

## 🏗️ Estrutura de Pastas Esperada

```
HexagonalLab.NET10/
│
├── src/
│   ├── HexagonalLab.Core/                 ← (ZERO changes in Phase 4)
│   │
│   ├── HexagonalLab.API/                  ← (Apenas Program.cs muda)
│   │   └── Program.cs                     ← Uma linha troca adapter!
│   │
│   └── HexagonalLab.Infrastructure/       ← OUTPUT ADAPTER (novo!)
│       ├── HexagonalLab.Infrastructure.csproj
│       │   ├── <PackageReference>EntityFrameworkCore</PackageReference>
│       │   ├── <PackageReference>EntityFrameworkCore.SqlServer</PackageReference>
│       │
│       ├── Data/
│       │   ├── AppDbContext.cs            ← DbContext
│       │   └── EntityConfigurations/
│       │       └── ItemConfiguration.cs   ← Entity Mapping
│       │
│       └── Repositories/
│           └── EfCoreRepositoryAdapter.cs ← Real Adapter
│
├── migrations/                             ← (opcional)
│   └── 001_InitialCreate.sql
│
└── tests/
    └── HexagonalLab.Infrastructure.Tests/ ← (novo - validar adapter)
        └── Repositories/
            └── EfCoreRepositoryAdapterTests.cs
```

---

## 💻 Passo a Passo: Implementação

### T1: Create Infrastructure Project

```bash
# Criar project
dotnet new classlib -n HexagonalLab.Infrastructure -o src/HexagonalLab.Infrastructure --force

# Adicionar à solução
dotnet sln HexagonalLab.NET10.sln add src/HexagonalLab.Infrastructure/HexagonalLab.Infrastructure.csproj

# Adicionar referência: Infrastructure → Core
cd src/HexagonalLab.Infrastructure
dotnet add reference ../HexagonalLab.Core/HexagonalLab.Core.csproj

# Adicionar EF Core packages
dotnet add package Microsoft.EntityFrameworkCore --version 8.0.0
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.0
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 8.0.0

cd ../..
```

### T2: Create DbContext + Entity Mapping

#### **Arquivo: src/HexagonalLab.Infrastructure/Data/AppDbContext.cs** (novo)

```csharp
namespace HexagonalLab.Infrastructure.Data;

using HexagonalLab.Core.Models;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// DbContext: Configuração de acesso a dados com EF Core.
/// 
/// CRÍTICO: Este arquivo:
/// ✅ Contém APENAS EF Core concerns
/// ✅ NUNCA vaza para Core
/// ✅ Adapter layer, não Core
/// ✅ Pode ser trocado por Dapper sem impacto
/// </summary>
public class AppDbContext : DbContext
{
    public DbSet<Item> Items { get; set; } = null!;

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplicar entity configurations
        modelBuilder.ApplyConfiguration(new ItemConfiguration());
    }
}
```

#### **Arquivo: src/HexagonalLab.Infrastructure/Data/EntityConfigurations/ItemConfiguration.cs** (novo)

```csharp
namespace HexagonalLab.Infrastructure.Data.EntityConfigurations;

using HexagonalLab.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// Entity Configuration: Mapping de Item no banco.
/// </summary>
public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        // Tabela
        builder.ToTable("Items");

        // Primary Key
        builder.HasKey(x => x.Id);

        // Properties
        builder.Property(x => x.Id)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Pending");

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(x => x.ProcessedAt)
            .IsRequired(false);

        // Indexes
        builder.HasIndex(x => x.Status).HasDatabaseName("IX_Items_Status");
        builder.HasIndex(x => x.CreatedAt).HasDatabaseName("IX_Items_CreatedAt");
    }
}
```

### T3: Implement EF Repository Adapter

#### **Arquivo: src/HexagonalLab.Infrastructure/Repositories/EfCoreRepositoryAdapter.cs** (novo)

```csharp
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
```

### T4: Create Database Migrations

```bash
# Adicionar EF Core CLI tools (se necessário)
dotnet tool install --global dotnet-ef

# Criar migration inicial
cd src/HexagonalLab.Infrastructure
dotnet ef migrations add InitialCreate --context AppDbContext --startup-project ../../src/HexagonalLab.API

# Gerar SQL (opcional, visual verification)
dotnet ef migrations script --context AppDbContext --startup-project ../../src/HexagonalLab.API

cd ../..
```

**Output esperado:**
```
Migrations/
├── [timestamp]_InitialCreate.cs
└── AppDbContextModelSnapshot.cs
```

### T5: Update DI - Trocar Adapter (O ponto mágico!)

#### **Arquivo: src/HexagonalLab.API/Program.cs** (MODIFICADO)

```csharp
// ANTES (Phase 3):
using var scope = builder.Services.BuildServiceProvider().CreateScope();
var inMemoryAdapter = new HexagonalLab.Core.Tests.Adapters.InMemoryRepositoryAdapter();
builder.Services.AddSingleton<IItemRepositoryPort>(inMemoryAdapter);

// ─────────────────────────────────────────────────────────────────

// DEPOIS (Phase 4 - UMA LINHA MUDA!):
using HexagonalLab.Infrastructure.Data;
using HexagonalLab.Infrastructure.Repositories;

// ... resto do código ...

// SÓ ADICIONAR ISTO:
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IItemRepositoryPort, EfCoreRepositoryAdapter>(); // ← ESTA!

// ─────────────────────────────────────────────────────────────────

// Criar banco na startup (opcional):
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
}
```

#### **Arquivo: src/HexagonalLab.API/appsettings.json** (novo)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=HexagonalLab;Trusted_Connection=true;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

### T6: E2E Test com Real DB

```bash
# Adicionar Infrastructure à solução (se não estiver)
dotnet sln HexagonalLab.NET10.sln add src/HexagonalLab.Infrastructure/HexagonalLab.Infrastructure.csproj

# Build
dotnet build HexagonalLab.NET10.sln

# Criar banco e rodar migrations
dotnet ef database update --context AppDbContext --startup-project src/HexagonalLab.API

# Rodar todos os testes
dotnet test

# (Opcional) Rodar API
cd src/HexagonalLab.API
dotnet run
# GET http://localhost:5000/api/items/ITEM-001
```

---

## ✅ Critério de Aceitação

Completar Phase 4 quando:

- [x] **Infrastructure project criado**
  - DbContext implementado
  - Entity mapping configurado
  - EF Core adapter implementado

- [x] **Migrations funcionam**
  ```
  dotnet ef database update
  ✅ Database created successfully
  ```

- [x] **API funciona com banco real**
  ```
  POST /api/items/ITEM-001/process
  ✅ 200 OK
  Data persisted in SQL Server
  ```

- [x] **Core inalterado**
  - Zero mudanças em HexagonalLab.Core

- [x] **Adapter intercambiável validado**
  - Trocar Program.cs de In-Memory para EF = DONE
  - Sem outras mudanças necessárias

- [x] **Testes E2E passam**
  ```
  dotnet test
  ✅ Todos os testes passam
  ```

---

## 🏛️ Decisões Arquiteturais

### Antes (Phase 3) vs Depois (Phase 4)

```
PHASE 3:
┌──────────────────────────────────────┐
│ API                                  │
│  └─ Program.cs (DI)                  │
│      └─ InMemoryRepositoryAdapter    │
│          └─ Dictionary (memória)     │
└──────────────────────────────────────┘

Core Funcionando: ✅ COM Fake

─────────────────────────────────────────────

PHASE 4:
┌──────────────────────────────────────┐
│ API                                  │
│  └─ Program.cs (DI)                  │
│      └─ EfCoreRepositoryAdapter      │
│          └─ SQL Server (real)        │
└──────────────────────────────────────┘

Core Funcionando: ✅ COM Real DB (ZERO mudanças!)
```

### Por que apenas 1 linha muda?

```csharp
// ANTES:
builder.Services.AddSingleton<IItemRepositoryPort>(inMemoryAdapter);

// DEPOIS:
builder.Services.AddScoped<IItemRepositoryPort, EfCoreRepositoryAdapter>();

// TUDO MAIS: IDENTICO!
```

---

## 🚨 Possíveis Problemas

### Problema 1: "Migration falha"
**Solução:**
```bash
# Remover migrations
rm src/HexagonalLab.Infrastructure/Migrations

# Recriar
dotnet ef migrations add InitialCreate --context AppDbContext --startup-project src/HexagonalLab.API
```

### Problema 2: "Database já existe"
**Solução:**
```bash
# Drop and recreate
dotnet ef database drop --context AppDbContext --startup-project src/HexagonalLab.API
dotnet ef database update
```

### Problema 3: "Core depende de EF Core"
**Resposta:** Se isso acontecer = bug arquitetural! Core NUNCA deve referenciar Infrastructure.

---

## 📌 Próximos Passos (Phase 5)

Quando Phase 4 estiver ✅ **DONE**:

1. Atualizar Story 274 status → `Done`
2. Iniciar Story 278 (Phase 5 - Multi-Adapter Pattern)
3. Criar Worker project
4. Reutilizar **MESMO** UseCase em novo contexto

---

**Status:** Ready for Implementation  
**Last Updated:** 2026-03-20  
**Author:** Architecture Team
