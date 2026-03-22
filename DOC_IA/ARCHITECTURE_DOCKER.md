# 🏗️ DOCKER INTEGRATION - ARCHITECTURAL PLAN

## 📊 Hexagonal Architecture + Docker Alignment

### Core Principle
**DOCKER IS NOT ARCHITECTURE. DOCKER IS INFRASTRUCTURE.**

The Hexagonal Architecture remains **UNCHANGED** when containerized. The separation between Core and Adapters is preserved completely.

```
📦 BEFORE (Local Development)
├─ Core (In-Memory)
├─ Adapters (In-Process)
└─ Infrastructure (Local Database)
    └─ SQL Server LocalDB

📦 AFTER (Containerized)
├─ Core (In-Memory) → SAME, NO CHANGES
├─ Adapters (In-Container Boundary)
└─ Infrastructure (Container Network)
    └─ SQL Server Container
```

---

## 🎯 Architecture Layers & Docker Responsibility

### Layer 1: CORE (Inside - Pure .NET)

**Location:** `src/HexagonalLab.Core/`

```csharp
// UseCases (Input Ports)
├─ GetItemUseCase.cs
├─ CreateItemUseCase.cs
└─ ...

// Models (Domain objects)
├─ Item.cs
├─ ItemRequest.cs
└─ ItemResponse.cs

// Ports (Interfaces - contracts only!)
├─ IItemInputPort.cs
├─ IItemRepositoryPort.cs
└─ ...
```

**Docker Impact:** ❌ **ZERO**
- Core NEVER references Docker
- Core NEVER references containers
- Core NEVER references infrastructure
- Core is 100% pure .NET (no frameworks, no infrastructure concerns)

**Why?** Testing the Core can be done WITHOUT Docker:
```csharp
[Fact]
public async Task GetItem_WithValidId_ReturnsItem()
{
    // No Docker needed!
    var fakeRepository = new FakeItemRepository(); // In-memory
    var useCase = new GetItemUseCase(fakeRepository);
    
    var result = await useCase.ExecuteAsync(1);
    
    Assert.NotNull(result);
}
```

---

### Layer 2: OUTPUT ADAPTERS (Outside - Implementation)

**Location:** `src/HexagonalLab.Infrastructure/`

```csharp
// EF Core Adapter (Output Port Implementation)
├─ Repositories/
│   ├─ EfCoreRepositoryAdapter.cs  // Implements IItemRepositoryPort
│   └─ CachedRepositoryAdapter.cs  // Decorator Pattern
│
// Database Context
├─ Data/
│   ├─ AppDbContext.cs
│   └─ EntityConfigurations/
│       └─ ItemConfiguration.cs
│
// Migrations
└─ Migrations/
    ├─ 20240101000000_InitialCreate.cs
    └─ ...
```

**Docker Responsibility:**
- Database lives in a **container** (`sqlserver`)
- Connection string: `Server=sqlserver;...` (internal DNS)
- Still implements `IItemRepositoryPort` (Core contract UNCHANGED)
- EF Core migrations run in API container during startup

**Bootstrap (Program.cs):**
```csharp
// Container orchestration ONLY affects DI registration
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// Core logic = SAME
// Only ENVIRONMENT changes WHERE the database is located
```

---

### Layer 3: INPUT ADAPTERS (Outside - Presentations)

**Location:** `src/HexagonalLab.API/` + `src/HexagonalLab.Worker/`

#### A. API Adapter (HTTP Input)

```
Docker Container: hexagonal-api
├─ Dockerfile (multi-stage build)
├─ Program.cs (DI configuration)
├─ Endpoints/
│   └─ ItemEndpoints.cs
└─ appsettings.json (with ENVVAR overrides)
```

**Responsibilities:**
- Receives HTTP requests
- Translates → Input Port (Core UseCase)
- Collects response
- Returns HTTP

**Docker Integration:**
- Exposed port: `5000` → `8080` (internal)
- Depends on: `sqlserver` service (health check)
- Environment variables: Connection string from `.env.docker`

#### B. Worker Adapter (Background Jobs)

```
Docker Container: hexagonal-worker
├─ Dockerfile (runtime image)
├─ Program.cs (background service host)
└─ Services/
    └─ BackgroundJobExecutor.cs
```

**Responsibilities:**
- Periodically or event-driven execution
- Calls Input Ports (UseCases)
- Updates database via Output Ports

**Docker Integration:**
- NO external port (background service)
- Depends on: `sqlserver` service (health check)
- Access to same database as API via connection string

---

### Layer 4: INFRASTRUCTURE (Outside - Platform Concerns)

**Location:** `docker-compose.yml` + containers

```yaml
services:
  sqlserver:        # SQL Server 2022 container
  api:              # .NET API container
  worker:           # .NET Worker container
```

**Responsibilities:**
- Orchestrate containers
- Manage network (`hexagonal-network`)
- Manage volumes (`sqlserver_data`)
- Pass environment variables
- Health checks

**Docker Isolation:**
```
┌─────────────────────────────────────────────────┐
│      Docker Network: hexagonal-network          │
├────────────────┬─────────────────┬──────────────┤
│                │                 │              │
│  API Service   │ Worker Service  │ SQL Server   │
│  (Container)   │  (Container)    │ (Container)  │
│                │                 │              │
└────────────────┴─────────────────┴──────────────┘
       ↓                ↓                  ↑
    HTTP Req       Background Job    SQL Query
   :5000 → :8080   (no port)        :1433
```

---

## 🔄 Data Flow in Docker

### Scenario: GET /api/items/1

```
1. HTTP Request (external)
   ↓ 
   localhost:5000/api/items/1
   ↓
2. API Container (hexagonal-api)
   ├─ Receives HTTP request
   └─ Routes to ItemEndpoints
   ↓
3. Input Adapter (API Endpoint)
   ├─ Parses request
   └─ Calls GetItemUseCase (Input Port)
   ↓
4. CORE UseCase (IN-MEMORY, UNCHANGED)
   ├─ GetItemUseCase.cs
   ├─ Validates input
   └─ Calls IItemRepositoryPort (interface)
   ↓
5. Output Adapter (EF Core Repository)
   ├─ EfCoreRepositoryAdapter.cs
   ├─ Translates Core model → SQL
   └─ Queries database via DbContext
   ↓
6. SQL Server Container (hexagonal-sqlserver)
   ├─ Receives SQL query
   ├─ Executes: SELECT * FROM Items WHERE Id=1
   └─ Returns data
   ↓
7. Output Adapter (Response)
   ├─ Maps data → Core Model (Item)
   └─ Returns to UseCase
   ↓
8. CORE UseCase (Response)
   ├─ Formats response
   └─ Returns ItemResponse
   ↓
9. Input Adapter (API Response)
   ├─ Converts ItemResponse → HTTP JSON
   └─ Returns 200 OK + JSON
   ↓
10. HTTP Response (external)
    ↓
    localhost:5000 returns JSON
```

**Key Observation:**
- Core logic: IDENTICAL whether local or containerized
- Only INFRASTRUCTURE changed (where database executes)
- Ports remain UNCHANGED
- Adapters UNCHANGED in responsibility (only location changed)

---

## 📁 File Structure + Docker Mapping

```
HexagonalLab/
├── src/
│   ├── HexagonalLab.Core/                 [★ CORE - Docker ignored]
│   │   ├── UseCases/                      ✅ No Docker references
│   │   ├── Ports/                         ✅ Pure interfaces
│   │   ├── Models/                        ✅ Plain POCOs
│   │   └── HexagonalLab.Core.csproj
│   │
│   ├── HexagonalLab.Infrastructure/       [★ OUTPUT ADAPTERS]
│   │   ├── Data/
│   │   │   ├── AppDbContext.cs            ↔️ Connects to container
│   │   │   └── ...
│   │   ├── Repositories/
│   │   │   ├── EfCoreRepositoryAdapter.cs ✅ Implements IItemRepositoryPort
│   │   │   └── ...
│   │   └── HexagonalLab.Infrastructure.csproj
│   │       └─ EntityFrameworkCore.SqlServer
│   │
│   ├── HexagonalLab.API/                  [★ INPUT ADAPTER (HTTP)]
│   │   ├── Dockerfile                    🐳 Build recipe for API container
│   │   ├── Program.cs                     🔌 DI configuration
│   │   ├── Endpoints/
│   │   │   └── ItemEndpoints.cs           ✅ Maps HTTP → Input Ports
│   │   ├── appsettings.json               📋 Base config
│   │   ├── appsettings.Development.json   📋 Dev overrides
│   │   └── appsettings.Production.json    📋 Prod overrides (from env)
│   │
│   └── HexagonalLab.Worker/               [★ INPUT ADAPTER (Background)]
│       ├── Dockerfile                    🐳 Build recipe for Worker
│       ├── Program.cs                     🔌 DI + Host builder
│       ├── Services/
│       │   └── JobExecutor.cs             ✅ Calls Input Ports
│       ├── appsettings.json               📋 Base config
│       ├── appsettings.Development.json   📋 Dev overrides
│       └── appsettings.Production.json    📋 Prod overrides (from env)
│
├── docker-compose.yml                    🐳 ORCHESTRATION
├── .dockerignore                         🐳 Build optimization
├── .env.docker                          🐳 Environment variables
├── scripts/
│   ├── init-db.sql                      🐳 SQL Server init
│   ├── docker-helper.ps1                🐳 Management (Windows)
│   └── docker-helper.sh                 🐳 Management (Linux/Mac)
│
├── DOCKER.md                            📖 Setup guide
└── docs/
    └── ARCHITECTURE_DOCKER.md           📖 This document
```

---

## 🔐 Environment Variables & Configuration

### Three-Layer Config Strategy

#### 1. Base Configuration (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": ""  // ← Empty! Use environment
  }
}
```

**Why empty?** Promotes explicit environment control.

#### 2. Environment-Specific (appsettings.{Environment}.json)

**Development (Local):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=HexagonalLab;Trusted_Connection=true;"
  }
}
```

**Production (Docker):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": ""  // ← Overridden by environment variable
  }
}
```

#### 3. Runtime Overrides (.env.docker)

```env
ConnectionStrings__DefaultConnection=Server=sqlserver;Database=HexagonalLab;User Id=sa;Password=HexagonalLab@2024!;TrustServerCertificate=true;
```

**How it works:**
```
1. appsettings.json loaded
2. appsettings.Production.json overrides
3. Environment variables FINAL override
   ↓
   ConnectionStrings__DefaultConnection → Core never knows!
   Only adapters care about actual connection
```

---

## ✅ Verification Checklist

### Core Isolation
- [ ] `HexagonalLab.Core.csproj` has NO Docker dependencies
- [ ] Core classes NEVER reference `DbContext`
- [ ] Core tests run WITHOUT containers
- [ ] Core is framework-agnostic

### Adapter Responsibility
- [ ] `IItemRepositoryPort` defined in Core
- [ ] `EfCoreRepositoryAdapter` in Infrastructure
- [ ] API Endpoints only call Input Ports
- [ ] No business logic in Endpoints

### Docker Compliance
- [ ] `docker-compose.yml` orchestrates containers
- [ ] Connection strings use container DNS: `sqlserver`
- [ ] Health checks ensure dependencies ready
- [ ] Volumes persist SQL Server data
- [ ] Environment variables externalizes configuration

### Dependency Direction
```
✅ CORRECT:
  External (Docker) → Adapter (EF Core) → Port (IRepository) → Core (UseCase)

❌ WRONG:
  Core → Infrastructure
  Core → Docker
  Core → EF Core
```

---

## 🚀 Deployment Stages

### Stage 1: Development (Local)
```bash
# Manual build-run from IDE
dotnet run --project src/HexagonalLab.API/
# Uses: appsettings.Development.json + LocalDB
```

### Stage 2: Docker Development
```bash
# Container environment
docker-compose --env-file .env.docker up -d
# Uses: appsettings.Production.json + env overrides
# Database: SQL Server in container
```

### Stage 3: Docker Production
```bash
# Production secrets in platform (Kubernetes, Docker Swarm, etc.)
docker-compose -f docker-compose.prod.yml up -d
# Uses: Strong passwords from secrets manager
# Database: SQL Server in container (replicated/backed-up)
```

**Key:** Core code is IDENTICAL in all three stages!

---

## 📚 References

- **Hexagonal Architecture**: https://alistair.cockburn.us/hexagonal-architecture/
- **SQL Server on Docker**: https://learn.microsoft.com/en-us/sql/linux/quickstart-install-connect-docker
- **.NET in Docker**: https://github.com/dotnet/dotnet-docker
- **Docker Compose**: https://docs.docker.com/compose/
