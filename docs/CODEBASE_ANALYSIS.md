# 🏛️ HexagonalLab.NET10 — Análise de Codebase

**Data**: 29 de Março de 2026  
**Status**: ✅ **Aplicação em Execução**  
**Porta**: http://localhost:5000

---

## 📊 Sumário Executivo

O projeto **HexagonalLab.NET10** implementa rigorosamente a **Arquitetura Hexagonal (Ports & Adapters)** conforme definido por **Alistair Cockburn**. A aplicação está completamente funcional com:

- ✅ **Core** herméticamente fechado (zero dependências de framework)
- ✅ **API REST** em execução (Minimal APIs + Scalar docs)
- ✅ **Banco de Dados** SQL Server 2022 em Docker
- ✅ **Migrations** aplicadas com sucesso
- ✅ **Tests** estruturados em 3 camadas

---

## 🏗️ Arquitetura Hexagonal — Diagrama

```
┌─────────────────────────────────────────────────────────────┐
│              OUTSIDE (Adapters)                             │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │         INPUT ADAPTERS                              │   │
│  │  ┌─────────────────────┐                            │   │
│  │  │  API (Minimal APIs) │                            │   │
│  │  │  - ItemEndpoints    │                            │   │
│  │  │  - Scalar Docs      │                            │   │
│  │  └─────────────────────┘                            │   │
│  │  ┌─────────────────────┐                            │   │
│  │  │  Worker Service     │                            │   │
│  │  │  - BackgroundJob    │                            │   │
│  │  │  - ItemProcessor    │                            │   │
│  │  └─────────────────────┘                            │   │
│  └─────────────────────────────────────────────────────┘   │
│                          ↓                                  │
│  ┌─────────────────────────────────────────────────────┐   │
│  │           INSIDE (Core - Hexágono)                  │   │
│  │       Zero Framework Dependencies! ✓                │   │
│  │                                                     │   │
│  │  ┌───────────────────────────────────────────┐    │   │
│  │  │         UseCases (Negócio)                │    │   │
│  │  │  • GetItemUseCase                         │    │   │
│  │  │  • GetAllItemsUseCase                     │    │   │
│  │  └───────────────────────────────────────────┘    │   │
│  │                       ↕                            │   │
│  │  ┌───────────────────────────────────────────┐    │   │
│  │  │         Ports (Interfaces/Contratos)     │    │   │
│  │  │  Input:                                   │    │   │
│  │  │   • IItemInputPort                        │    │   │
│  │  │   • IGetAllItemsInputPort                 │    │   │
│  │  │  Output:                                  │    │   │
│  │  │   • IItemRepositoryPort                   │    │   │
│  │  └───────────────────────────────────────────┘    │   │
│  │                       ↕                            │   │
│  │  ┌───────────────────────────────────────────┐    │   │
│  │  │         Models (Dados Simples)            │    │   │
│  │  │  • Item.cs ← DTO, não Entity              │    │   │
│  │  └───────────────────────────────────────────┘    │   │
│  └─────────────────────────────────────────────────────┘   │
│                          ↑                                  │
│  ┌─────────────────────────────────────────────────────┐   │
│  │         OUTPUT ADAPTERS                             │   │
│  │  ┌─────────────────────────────────────────────┐   │   │
│  │  │  EF Core Adapter (Produção)                │   │   │
│  │  │  - EfCoreRepositoryAdapter                 │   │   │
│  │  │  - AppDbContext (EF Core)                  │   │   │
│  │  │  - SQL Server 2022                         │   │   │
│  │  └─────────────────────────────────────────────┘   │   │
│  │  ┌─────────────────────────────────────────────┐   │   │
│  │  │  In-Memory Adapter (Testes)                │   │   │
│  │  │  - InMemoryRepository                      │   │   │
│  │  │  - Usado por HexagonalLab.Core.Tests       │   │   │
│  │  └─────────────────────────────────────────────┘   │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

**Princípio Central**: A aplicação core **não sabe** que está sendo usada por HTTP ou armazenada em banco.

---

## 📁 Estrutura de Pastas

```
HexagonalLab.NET10/
│
├── src/                              # Código-fonte
│   │
│   ├── HexagonalLab.Core/           ❤️ Hexágono (Inside)
│   │   ├── UseCases/
│   │   │   ├── GetItemUseCase.cs
│   │   │   └── GetAllItemsUseCase.cs
│   │   │
│   │   ├── Ports/
│   │   │   ├── IItemInputPort.cs
│   │   │   ├── IGetAllItemsInputPort.cs
│   │   │   └── IItemRepositoryPort.cs
│   │   │
│   │   ├── Models/
│   │   │   └── Item.cs              (DTO simples, não Entity)
│   │   │
│   │   └── HexagonalLab.Core.csproj (¡Zero dependencies!)
│   │
│   ├── HexagonalLab.API/            📥 Input Adapter
│   │   ├── Endpoints/
│   │   │   └── ItemEndpoints.cs
│   │   │
│   │   ├── Program.cs               (Bootstrap/DI)
│   │   ├── appsettings.*.json
│   │   ├── Properties/launchSettings.json
│   │   │
│   │   └── HexagonalLab.API.csproj
│   │
│   ├── HexagonalLab.Worker/         📥 Input Adapter
│   │   ├── Services/
│   │   │   └── ItemProcessorService.cs
│   │   │
│   │   ├── Program.cs               (Bootstrap/DI)
│   │   ├── appsettings.*.json
│   │   │
│   │   └── HexagonalLab.Worker.csproj
│   │
│   └── HexagonalLab.Infrastructure/ 📤 Output Adapters
│       ├── Repositories/
│       │   ├── EfCoreRepositoryAdapter.cs   (Produção)
│       │   └── InMemoryRepository.cs        (Testes)
│       │
│       ├── Data/
│       │   ├── AppDbContext.cs
│       │   └── Migrations/
│       │       └── 20260321134350_InitialCreate.cs
│       │
│       └── HexagonalLab.Infrastructure.csproj
│
├── tests/                            🧪 Testes
│   │
│   ├── HexagonalLab.Core.Tests/
│   │   ├── UseCases/
│   │   │   ├── GetItemUseCaseTests.cs
│   │   │   └── GetAllItemsUseCaseTests.cs
│   │   │
│   │   ├── Ports/
│   │   └── Models/
│   │
│   ├── HexagonalLab.API.Tests/
│   │   ├── Endpoints/
│   │   │   └── ItemEndpointsTests.cs
│   │   │
│   │   ├── Fixtures/
│   │   │   └── CustomWebApplicationFactory.cs
│   │   │
│   │   └── MultiplAdapters/
│   │
│   └── HexagonalLab.Infrastructure.Tests/
│       ├── Repositories/
│       │   └── EfCoreRepositoryAdapterTests.cs
│       │
│       └── Data/
│
├── docs/                             📚 Documentação
│   ├── SETUP.md                     (Setup inicial)
│   ├── CODEBASE_ANALYSIS.md         (Este arquivo)
│   ├── docker/                      (Docker docs)
│   │   ├── DOCKER_COMPLETE.md
│   │   ├── DOCKER_IMPLEMENTATION_CHECKLIST.md
│   │   ├── DOCKER_QUICKSTART.md
│   │   └── README_DOCKER.md
│   │
│   └── README.md
│
├── scripts/                          🔧 Scripts
│   ├── docker-helper.ps1
│   ├── docker-helper.sh
│   ├── init-db.sql
│   │
│   └── ambiente/
│       ├── cleanup.ps1
│       └── startup.ps1
│
├── DOC_IA/                           🤖 Documentação de IA
│   ├── ARCHITECTURE_DOCKER.md
│   ├── ARCHITECTURE_REVIEW.md
│   ├── INITIATIVE_DOCKER_SETUP.md
│   │
│   ├── Fases/                       (Phases em progresso)
│   │   ├── INDEX.md
│   │   ├── PHASE_01_Foundations.md
│   │   ├── PHASE_02_OutputPorts.md
│   │   ├── PHASE_03_APIAdapter.md
│   │   ├── PHASE_04_RealAdapter.md
│   │   ├── PHASE_05_MultiAdapter.md
│   │   ├── PHASE_06_Testing.md
│   │   └── PHASE_07_Evolution.md
│   │
│   └── Fases_Concluidas/            (Phases finalizadas)
│       ├── PHASE_01_CODE_REVIEW.md
│       ├── PHASE_01_SUMMARY.md
│       ├── ... (e assim por diante)
│       └── PHASE_07_SUMMARY.md
│
├── .github/                          🔄 GitHub/CI-CD
│   └── copilot-instructions.md      (Instruções para IA)
│
├── docker-compose.yml               (Orquestração Docker)
├── env.exemplo                      (Template de variáveis)
├── env.docker.ejemplo               (Template Docker)
├── global.json                      (.NET versioning)
├── HexagonalLab.NET10.slnx         (Solution file)
├── handson.md                       (Hands-on lab guide)
└── README.md                        (Visão geral)
```

---

## 🔌 Interfaces (Ports)

### Input Ports

#### `IItemInputPort`
```csharp
namespace HexagonalLab.Core.Ports;

public interface IItemInputPort
{
    Task<Item?> ProcessAsync(string itemId);
}
```
**Implementado por**: GetItemUseCase  
**Consumido por**: ItemEndpoints (API)

#### `IGetAllItemsInputPort`
```csharp
namespace HexagonalLab.Core.Ports;

public interface IGetAllItemsInputPort
{
    Task<IEnumerable<Item>> GetAllAsync();
}
```
**Implementado por**: GetAllItemsUseCase  
**Consumido por**: ItemEndpoints (API)

---

### Output Ports

#### `IItemRepositoryPort`
```csharp
namespace HexagonalLab.Core.Ports;

public interface IItemRepositoryPort
{
    Task SaveAsync(Item item);
    Task<Item?> GetByIdAsync(string itemId);
    Task<IEnumerable<Item>> GetAllAsync();
}
```
**Implementações**:
- `EfCoreRepositoryAdapter` (SQL Server - Produção)
- `InMemoryRepository` (Dictionary - Testes)

---

## 🚀 Use Cases (Lógica de Negócio)

### GetItemUseCase
```
Entrada: string itemId
↓
Busca no repositório (abstrato)
↓
Retorna: Item? (ou null)
↓
Sem conhecimento de HTTP, DB, Framework
```

### GetAllItemsUseCase
```
Entrada: (nenhuma)
↓
Busca todos os itens (abstracto)
↓
Retorna: IEnumerable<Item>
↓
100% testável sem banco
```

---

## 📡 Endpoints (Input Adapters)

### ItemEndpoints.cs (API)

```csharp
// GET /items/{id}
// Injeta IItemInputPort (GetItemUseCase)
// Retorna HTTP 200 com Item ou 404

// GET /items
// Injeta IGetAllItemsInputPort (GetAllItemsUseCase)
// Retorna HTTP 200 com lista
```

**Padrão**: Mapeia requisições HTTP → Input Ports → UseCases

---

## 💾 Database & Migrations

### Schema (Items Table)

```sql
CREATE TABLE [Items] (
    [Id] nvarchar(50) NOT NULL PRIMARY KEY,
    [Name] nvarchar(255) NOT NULL,
    [Status] nvarchar(50) NOT NULL DEFAULT N'Pending',
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [ProcessedAt] datetime2 NULL
);

CREATE INDEX [IX_Items_CreatedAt] ON [Items] ([CreatedAt]);
CREATE INDEX [IX_Items_Status] ON [Items] ([Status]);
```

### Migration
- Arquivo: `20260321134350_InitialCreate.cs`
- Local: `src/HexagonalLab.Infrastructure/Migrations/`
- Criada via: `dotnet ef migrations add InitialCreate`
- Aplicada via: `dotnet ef database update`

---

## 🧪 Testes — Estrutura

### HexagonalLab.Core.Tests 🟢
**O núcleo roda sem dependências externas!**

```
✅ GetItemUseCaseTests.cs
   - Testa lógica do usecase
   - Usa InMemoryRepository (fake)
   - Sem HttpClient, sem Entity Framework

✅ GetAllItemsUseCaseTests.cs
   - Testa recuperação de múltiplos itens
   - Usa injeção de dependências
   - Isolado do banco de dados
```

### HexagonalLab.API.Tests 🟡
**Testa adaptadores de entrada (HTTP)**

```
✅ ItemEndpointsTests.cs
   - CustomWebApplicationFactory
   - HttpClient contra servidor em memória
   - Testa routing, serialização, status codes
```

### HexagonalLab.Infrastructure.Tests 🔴
**Testa adaptadores de saída (persistência)**

```
✅ EfCoreRepositoryAdapterTests.cs
   - DbContext em memória
   - Testa SaveAsync, GetByIdAsync, GetAllAsync
   - Verifica integridade com migrations
```

---

## 🐳 Infraestrutura (Docker)

### docker-compose.yml

```yaml
services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: hexagonal-sqlserver
    ports:
      - "1433:1433"
    environment:
      ACCEPT_EULA: "Y"
      SA_PASSWORD: "HexLab@2024"
      MSSQL_PID: "Developer"
    volumes:
      - sqlserver_data:/var/opt/mssql
    healthcheck:
      test: ["CMD", "/opt/mssql-tools18/bin/sqlcmd", ...]
      interval: 15s
      retries: 10
```

**Status**: ✅ Container `hexagonal-sqlserver` rodando e "Healthy"

---

## ⚙️ Configuração & Variáveis de Ambiente

### .env.docker (Produção)
```bash
DB_NAME=HexagonalLab
DB_PORT=1433
DB_PASSWORD=HexagonalLab@2024!
DB_USER=sa
API_PORT=5000
API_ENVIRONMENT=Development
ASPNETCORE_ENVIRONMENT=Development
WORKER_ENVIRONMENT=Development
```

### appsettings.Development.json (API)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=HexagonalLab;User Id=sa;Password=HexLab@2024;TrustServerCertificate=true;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

---

## 📊 Estado Atual (29 de Março de 2026)

### ✅ Checklist de Setup

| Item | Status | Detalhes |
|------|--------|----------|
| **.NET SDK** | ✅ | v10.0.103 instalado |
| **Docker** | ✅ | v29.3.1 rodando |
| **Restore** | ✅ | `dotnet restore` (0.8s) |
| **Build** | ✅ | 6 projetos compilados (56.4s) |
| **SQL Server** | ✅ | Container "Healthy" em 16.4s |
| **Migrations** | ✅ | Database `HexagonalLab` criada |
| **Schema** | ✅ | Tabela `Items` com índices |
| **API** | ✅ | http://localhost:5000 |
| **Documentação** | ✅ | http://localhost:5000/scalar/v1 |

---

## 🌐 Endpoints Disponíveis

### API Base
```
https://localhost:5000
```

### Rotas de Item

| Método | Rota | Descrição | UseCase |
|--------|------|-----------|---------|
| **GET** | `/items/{id}` | Buscar item por ID | GetItemUseCase |
| **GET** | `/items` | Listar todos os itens | GetAllItemsUseCase |

### Documentação

| URL | Descrição | Ferramenta |
|-----|-----------|-----------|
| `http://localhost:5000/scalar/v1` | API interativa | Scalar |
| `http://localhost:5000/openapi/v1.json` | OpenAPI spec | OpenAPI 3.0 |

---

## 🎯 Decisões Arquiteturais Implementadas

### ✅ Arquitetura Hexagonal (Ports & Adapters)

| Princípio | Implementação | Status |
|-----------|---------------|--------|
| Core sem frameworks | ❌ ASP.NET, ❌ EF Core, ✅ Core limpo | ✓ |
| Ports como contratos | ✅ IItemInputPort, IItemRepositoryPort | ✓ |
| Adapters intercambiáveis | EF Core ↔ Dapper (uma linha no DI) | ✓ |
| Core independente de UI | Core roda em testes sem HTTP | ✓ |
| Core independente de BD | InMemoryRepository (fakes) | ✓ |

### ❌ NÃO implementado (por design)

| O que NÃO tem | Por quê |
|---------------|---------|
| **DDD** (Aggregates, Entities, Value Objects) | Cockburn usa "Models" simples, não DDD |
| **Clean Architecture layering** | Cockburn é mais simples (inside/outside) |
| **Domain Events** | Não necessário p/ esta fase |
| **CQRS** | Não necessário p/ esta fase |
| **Saga pattern** | Não necessário p/ esta fase |

---

## 🚀 Como Usar

### 1. Verificar Status

```powershell
# Terminal 1: Verificar que SQL Server está rodando
docker ps | Select-String hexagonal-sqlserver

# Terminal 2: Verificar que API está respondendo
Invoke-WebRequest http://localhost:5000/items | ConvertFrom-Json
```

### 2. Acessar Documentação

Abrir no navegador:
```
http://localhost:5000/scalar/v1
```

### 3. Executar Testes

```bash
# Todos os testes
dotnet test

# Um projeto específico
dotnet test tests/HexagonalLab.Core.Tests/

# Com cobertura
dotnet test /p:CollectCoverage=true
```

### 4. Parar Aplicação

```bash
# se rodar em background (Terminal ID: 31f92d6c-0846-45e6-b791-67ac34fe8610)
# Parar com Ctrl+C ou:
# Get-Process -Name dotnet | Stop-Process
```

### 5. Parar Banco de Dados

```bash
docker-compose down
```

---

## 📚 Fases de Desenvolvimento (Roadmap)

| Fase | Nome | Objetivo | Status |
|------|------|----------|--------|
| **1** | Fundamentos | Core + Ports básicos | ✅ Completa |
| **2** | Output Ports | Abstrair dependências externas | ✅ Completa |
| **3** | Input Adapter | API REST (este projeto) | ✅ Completa |
| **4** | Real Adapter | EF Core + SQL | ✅ Completa |
| **5** | Multi-Adapter | Worker Service | ✅ Completa |
| **6** | Testing | xUnit em 3 camadas | ✅ Completa |
| **7** | Evolution | Docker, CI/CD, Registros | ⏳ Em progresso |

---

## 🤝 Padrões de Desenvolvimento

### Dependency Injection (Bootstrap)

**API (`src/HexagonalLab.API/Program.cs`)**:
```csharp
// Registra Input Ports (UseCases)
builder.Services.AddScoped<IItemInputPort, GetItemUseCase>();
builder.Services.AddScoped<IGetAllItemsInputPort, GetAllItemsUseCase>();

// Registra Output Port Adaptador
builder.Services.AddScoped<IItemRepositoryPort, EfCoreRepositoryAdapter>();

// Configura DbContext
builder.Services.AddDbContext<AppDbContext>(
    options => options.UseSqlServer(connectionString)
);
```

**Para trocar EF Core por Dapper**:
```csharp
// mudança de 1 linha!
builder.Services.AddScoped<IItemRepositoryPort, DapperRepositoryAdapter>();
```

---

## 💡 Insights Técnicos

### Força 1: Testabilidade
O Core roda **sem banco, sem HTTP, sem framework**. Testes são rápidos e confiáveis.

### Força 2: Flexibilidade
Trocar tecnologias (EF → Dapper, SQL → PostgreSQL, HTTP → gRPC) é uma mudança de DI.

### Força 3: Clareza Arquitetural
O código reflete exatamente a separação "inside/outside" do Cockburn.

### Oportunidade: Multi-Adapter
Worker Service reutiliza exatamente os mesmos UseCases da API. Demonstra plugabilidade.

---

## 🔍 Próximas Ações Sugeridas

1. **CI/CD**:
   - [ ] GitHub Actions para build/test automático
   - [ ] Azure DevOps Pipeline
   - [ ] Docker image build na CI

2. **APIs Adicionais**:
   - [ ] gRPC adapter
   - [ ] GraphQL adapter
   - [ ] WebSocket adapter

3. **Persistência Alternativa**:
   - [ ] Dapper adapter
   - [ ] MongoDB adapter
   - [ ] Redis cache adapter

4. **Observabilidade**:
   - [ ] Application Insights
   - [ ] Estruturado logging
   - [ ] Request correlation IDs

5. **Documentação**:
   - [ ] OpenAPI V3 completo
   - [ ] Diagrama C4 model
   - [ ] Decision Records (ADR)

---

## 📌 Referências

- **Alistair Cockburn - Hexagonal Architecture**:  
  https://alistair.cockburn.us/hexagonal-architecture/

- **.NET 10 Documentation**:  
  https://learn.microsoft.com/en-us/dotnet/

- **Entity Framework Core**:  
  https://learn.microsoft.com/en-us/ef/core/

- **ASP.NET Core Minimal APIs**:  
  https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis

---

## 📝 Metadados do Documento

| Campo | Valor |
|-------|-------|
| **Data de Criação** | 29 de Março, 2026 |
| **Última Atualização** | 29 de Março, 2026 |
| **Versão do Projeto** | Phase 7 (Evolution) |
| **.NET Version** | 10.0.103 |
| **SQL Server** | 2022-latest |
| **Status da Aplicação** | ✅ Running on http://localhost:5000 |

---

**Documento gerado automaticamente com análise completa do codebase HexagonalLab.NET10**
