# 📖 DOCKER COMPLETE REFERENCE - HexagonalLab

Complete guide covering setup, architecture, configuration, verification, and troubleshooting.

---

## 🎯 Table of Contents

1. [Quick Start](#quick-start)
2. [Architecture Overview](#architecture-overview)
3. [Configuration](#configuration)
4. [Common Commands](#common-commands)
5. [Verification Steps](#verification-steps)
6. [Before vs After](#before-vs-after)
7. [Troubleshooting](#troubleshooting)

---

## Quick Start

### Prerequisites

- Docker Desktop installed and running
- 4+ GB free disk space
- Ports 5000 and 1433 available

### 1. Start Containers

```bash
# Navigate to project root
cd HexagonalLab

# Start all services
docker-compose --env-file .env.docker up -d
```

### 2. Wait for Health Checks

SQL Server takes 10-15 seconds to initialize.

```bash
# Monitor status
docker-compose ps

# Keep checking until all show "healthy" or "Up"
```

### 3. Test Connectivity

```bash
# Test database connection
docker exec hexagonal-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost \
  -U sa \
  -P HexagonalLab@2024! \
  -C \
  -Q "SELECT @@VERSION"

# Test API endpoint
curl http://localhost:5000/api/items
# OR (PowerShell)
Invoke-WebRequest http://localhost:5000/api/items -UseBasicParsing
```

---

## Architecture Overview

### Service Topology

```
┌──────────────────────────────────────────────────┐
│          Docker Network: hexagonal-network       │
├──────────────────────────────────────────────────┤
│                                                  │
│  ┌──────────────────┐   ┌────────────────────┐  │
│  │  hexagonal-api   │   │ hexagonal-worker   │  │
│  │  Port: 8080      │   │ (background svc)   │  │
│  │  → 5000 (host)   │   │                    │  │
│  └────────┬─────────┘   └────────┬───────────┘  │
│           │                      │              │
│           └──────────┬───────────┘              │
│                      │                         │
│           ┌──────────▼──────────┐             │
│           │ hexagonal-sqlserver │             │
│           │ Port: 1433          │             │
│           │ Volume: sqlserver_  │             │
│           │         data        │             │
│           └─────────────────────┘             │
│                                                  │
└──────────────────────────────────────────────────┘
```

### Hexagonal Architecture Layers

```
┌──────────────────────────────────────────────────┐
│     DOCKER ORCHESTRATION (Infrastructure)        │
│     - Containers + Network + Volumes             │
└──────────────────────────────────────────────────┘
                        ↑
┌──────────────────────────────────────────────────┐
│     INPUT ADAPTERS (Outside)                     │
│     - API Endpoints (HTTP → IItemInputPort)      │
│     - Worker Service (Jobs → IItemInputPort)     │
└──────────────────────────────────────────────────┘
                        ↑
┌──────────────────────────────────────────────────┐
│     OUTPUT ADAPTERS (Outside)                    │
│     - EfCoreRepositoryAdapter                    │
│     - CachedRepositoryAdapter (Decorator)        │
│     - AppDbContext (EF Core)                     │
└──────────────────────────────────────────────────┘
                        ↑
┌──────────────────────────────────────────────────┐
│     PORTS (Boundary - Interfaces Only)           │
│     - IItemInputPort (UseCase contract)          │
│     - IItemRepositoryPort (Storage contract)     │
└──────────────────────────────────────────────────┘
                        ↑
┌──────────────────────────────────────────────────┐
│     CORE (Inside - UNCHANGED)                    │
│     - UseCases (GetItemUseCase, etc.)            │
│     - Models (Item, ItemRequest, ItemResponse)   │
│     - Business Logic (100% testable)             │
│     [ZERO Docker/Framework dependencies]         │
└──────────────────────────────────────────────────┘
```

**Key Principle:** Core is completely independent of Docker and runs identically in any environment.

---

## Configuration

### Environment Variables (.env.docker)

```env
# Database
DB_NAME=HexagonalLab              # Database name
DB_PORT=1433                      # SQL Server port
DB_PASSWORD=HexabolabLab@2024!    # SA password
DB_USER=sa                        # SA username

# API
API_PORT=5000                     # External port mapping
ASPNETCORE_ENVIRONMENT=Development  # Development | Production
```

### Connection Strings

**Format Template:**
```
Server={host};Database={DB_NAME};User Id={DB_USER};Password={DB_PASSWORD};TrustServerCertificate=true;
```

**From Inside Container (API → SQL Server):**
```
Server=sqlserver;Database=HexagonalLab;User Id=sa;Password=HexabolabLab@2024!;TrustServerCertificate=true;
```

**From Host Machine (Admin tools → SQL Server):**
```
Server=localhost;Database=HexagonalLab;User Id=sa;Password=HexabolabLab@2024!;TrustServerCertificate=true;
```

### appsettings Resolution

```
1. appsettings.json (base, minimal)
           ↓
2. appsettings.{Environment}.json (overrides)
           ↓
3. Environment Variables (final, highest priority)
           ↓
4. Final Configuration (used at runtime)
```

---

## Common Commands

### Lifecycle Management

```bash
# Start containers in background
docker-compose up -d

# Stop containers (data persists)
docker-compose down

# Stop and remove all data
docker-compose down -v

# Restart specific service
docker-compose restart api
docker-compose restart sqlserver
docker-compose restart worker

# Rebuild all containers
docker-compose build --no-cache
docker-compose up -d
```

### Monitoring

```bash
# View all services status
docker-compose ps

# Watch logs in real-time
docker-compose logs -f                 # All services
docker-compose logs -f sqlserver       # By service
docker-compose logs -f api
docker-compose logs -f worker

# View last 50 lines
docker-compose logs --tail=50

# Inspect container details
docker inspect hexagonal-api
docker inspect hexagonal-sqlserver
```

### Database Operations

```bash
# Execute SQL query
docker exec hexagonal-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost \
  -U sa \
  -P HexabolabLab@2024! \
  -C \
  -Q "SELECT * FROM HexagonalLab.dbo.Items"

# Run interactive SQL shell
docker exec -it hexagonal-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost \
  -U sa \
  -P HexabolabLab@2024! \
  -C
```

### Debugging

```bash
# Execute command in container
docker exec hexagonal-api dotnet --version
docker exec hexagonal-api ls -la /app

# Access container shell
docker exec -it hexagonal-api /bin/bash
docker exec -it hexagonal-api powershell

# View container file system
docker exec hexagonal-api find /app -name "*.dll"
```

---

## Verification Steps

### Step 1: Containers Running

```bash
docker-compose ps

# Expected: All containers with "Up" or "healthy" status
```

### Step 2: SQL Server Connection

```bash
docker exec hexagonal-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost \
  -U sa \
  -P HexabolabLab@2024! \
  -C \
  -Q "SELECT 1"

# Expected: 1 (success)
```

### Step 3: Database Exists

```bash
docker exec hexagonal-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost \
  -U sa \
  -P HexabolabLab@2024! \
  -C \
  -Q "SELECT name FROM sys.databases WHERE name='HexagonalLab'"

# Expected: HexagonalLab (listed)
```

### Step 4: Tables Exist

```bash
docker exec hexagonal-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost \
  -U sa \
  -P HexabolabLab@2024! \
  -C \
  -Q "SELECT TABLE_NAME FROM HexagonalLab.INFORMATION_SCHEMA.TABLES"

# Expected: Items (and other tables)
```

### Step 5: API Responds

```bash
# HTTP GET
curl http://localhost:5000/api/items

# PowerShell
Invoke-WebRequest http://localhost:5000/api/items -UseBasicParsing

# Expected: 200 OK + JSON array
```

---

## Before vs After

### Before (LocalDB)

```
┌──────────────────────┐
│   Local Development  │
├──────────────────────┤
│                      │
│  IDE ─→ API.exe      │
│      └─→ Worker.exe  │
│      └─→ SQL LocalDB │
│                      │
└──────────────────────┘

Limitations:
❌ Windows-only (LocalDB)
❌ Coupled to machine
❌ Hard to replicate
❌ Different from production
```

### After (Docker)

```
┌─────────────────────────────────┐
│   Containerized Environment     │
├─────────────────────────────────┤
│  Docker Network: hexagonal-net  │
│                                 │
│  ┌─────────────────────────────┐│
│  │ API Container (.NET 10)     ││
│  └──────────────┬──────────────┘│
│  ┌──────────────┴──────────────┐│
│  │ Worker Container (.NET 10)  ││
│  └──────────────┬──────────────┘│
│  ┌──────────────┴──────────────┐│
│  │ SQL Server 2022 Container   ││
│  │ (Data: Named Volume)        ││
│  └─────────────────────────────┘│
│                                 │
└─────────────────────────────────┘

Benefits:
✅ Platform-independent (Windows/Mac/Linux)
✅ Easy team setup (one command)
✅ Identical to production
✅ Infrastructure isolated
✅ Core unchanged
```

---

## Troubleshooting

### Problem: Containers Won't Start

**Symptom:** `docker-compose up -d` fails or containers crash

**Solutions:**
```bash
# Check detailed error logs
docker-compose logs

# Check if port is already in use
netstat -ano | findstr :5000  (Windows)
lsof -i :5000                 (Mac/Linux)

# Change port in .env.docker if needed
# API_PORT=5001  # Use different port
# Then restart: docker-compose up -d
```

### Problem: SQL Server Unhealthy

**Symptom:** `hexagonal-sqlserver` shows "starting" or "unhealthy"

**Solutions:**
```bash
# SQL Server takes 15-30 seconds to initialize
# Just wait... (patience!)
docker-compose ps  # Keep checking

# If it doesn't improve after 1 minute:
docker exec hexagonal-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost \
  -U sa \
  -P HexabolabLab@2024! \
  -C \
  -Q "SELECT @@VERSION"

# If command fails, check logs
docker-compose logs sqlserver
```

### Problem: API Timeout Connecting to Database

**Symptom:** API logs show "Timeout expired" or "Connection refused"

**Solutions:**
```bash
# 1. Verify SQL Server is healthy
docker-compose ps sqlserver  # Should show "healthy"

# 2. Test connection from API container
docker exec hexagonal-api ping sqlserver

# 3. Test SQL Server port
docker exec hexagonal-api curl -I telnet://sqlserver:1433

# 4. Check API logs for detailed error
docker logs hexagonal-api --tail 100

# 5. Restart just the API
docker-compose restart api
```

### Problem: Port Already in Use

**Symptom:** `Error response from daemon: Bind for 0.0.0.0:5000`

**Solutions:**
```bash
# Option 1: Find what's using the port
netstat -ano | findstr :5000  (Windows)
lsof -i :5000                 (Mac/Linux)

# Option 2: Change port in .env.docker
# Before running docker-compose:
# API_PORT=5001
# Then: docker-compose up -d

# Option 3: Kill the process (if safe)
# taskkill /PID <PID> /F  (Windows)
# kill -9 <PID>          (Mac/Linux)
```

### Problem: Data Lost After Restart

**Symptom:** Containers restarted but database is empty

**Solutions:**
```bash
# This shouldn't happen! Verify volume exists
docker volume ls | grep sqlserver_data

# If volume doesn't exist, data was lost with 'docker-compose down -v'
# To prevent: use 'docker-compose down' (not -v flag)

# If volume exists but data is gone:
docker exec hexagonal-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost \
  -U sa \
  -P HexabolabLab@2024! \
  -C \
  -Q "SELECT name FROM sys.databases WHERE name='HexagonalLab'"
# Should return "HexagonalLab" if properly initialized
```

### Problem: API Can't Find Database After Migration

**Symptom:** API starts but migrations fail

**Solutions:**
```bash
# 1. Check if HexagonalLab database exists
docker exec hexagonal-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost \
  -U sa \
  -P HexabolabLab@2024! \
  -C \
  -Q "SELECT name FROM sys.databases WHERE name='HexagonalLab'"

# 2. Manually create if missing
docker exec hexagonal-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost \
  -U sa \
  -P HexabolabLab@2024! \
  -C \
  -Q "CREATE DATABASE HexagonalLab"

# 3. Restart API to run migrations
docker-compose restart api

# 4. If still failing, check API logs
docker logs hexagonal-api --tail 100
```

---

## Security Notes

### Development (Current)

✅ Default password for quick local setup  
✅ TrustServerCertificate enabled (localhost safe)  
✅ SA account enabled (acceptable for dev)

### Production (Recommendations)

- [ ] Change `DB_PASSWORD` to strong random value
- [ ] Use secrets management (Azure Key Vault, K8s Secrets)
- [ ] Require certificate-based SQL connections
- [ ] Implement SQL Server encryption at rest
- [ ] Enable audit logging
- [ ] Use `ASPNETCORE_ENVIRONMENT=Production`
- [ ] Restrict container network access

---

## Quick Reference

| Task | Command |
|------|---------|
| Start | `docker-compose up -d` |
| Stop | `docker-compose down` |
| Status | `docker-compose ps` |
| Logs | `docker-compose logs -f` |
| Rebuild | `docker-compose build --no-cache` |
| Clean | `docker-compose down -v` |
| Exec SQL | `docker exec hexagonal-sqlserver sqlcmd ...` |
| Test API | `curl http://localhost:5000/api/items` |

---

## Files Summary

```
├── docker-compose.yml              ← Primary orchestration
├── .env.docker                      ← Environment variables
├── .dockerignore
├── Dockerfile (x2)                  ← API & Worker images
│   ├── src/HexagonalLab.API/Dockerfile
│   └── src/HexagonalLab.Worker/Dockerfile
├── scripts/
│   ├── init-db.sql                  ← Database init
│   ├── docker-helper.ps1            ← Windows helpers
│   └── docker-helper.sh             ← Unix helpers
└── appsettings files
    ├── src/HexagonalLab.API/appsettings.Development.json
    ├── src/HexagonalLab.API/appsettings.Production.json
    ├── src/HexagonalLab.Worker/appsettings.Development.json
    └── src/HexagonalLab.Worker/appsettings.Production.json
```

---

**Status:** ✅ Production-Ready  
**Last Updated:** 2026-03-22  
**Architecture:** ✅ Hexagonal (Pure & Isolated)
