# ✅ IMPLEMENTATION CHECKLIST - Docker Setup

## 🎯 Goal
Get Docker containers running locally with all three services: SQL Server, API, and Worker

---

## ✨ Pre-Flight Check

- [ ] Docker Desktop installed and running
- [ ] `docker --version` works in terminal
- [ ] `docker-compose --version` works
- [ ] At least 4 GB free disk space
- [ ] Port 5000 and 1433 available (or modify .env.docker)

**Command to verify:**
```bash
docker --version
docker-compose --version
docker ps  # Should show empty list or existing containers
```

---

## 🚀 Step 1: Start Containers

```bash
# Navigate to project root
cd HexagonalLab

# Start all containers with env file
docker-compose --env-file .env.docker up -d
```

**Expected output:**
```
Creating network "hexagonal_hexagonal-network" with driver "bridge"
Creating volume "hexagonal_sqlserver_data" with local driver
Creating hexagonal-sqlserver ... done
Creating hexagonal-api ... done
Creating hexagonal-worker ... done
```

---

## ⏳ Step 2: Wait for Health Checks

SQL Server takes 10-15 seconds to be ready.

```bash
# Check status (wait for all to be healthy)
docker-compose ps

# Keep checking until you see:
# NAME                 STATUS              PORTS
# hexagonal-sqlserver  healthy (1/1)       0.0.0.0:1433->1433/tcp
# hexagonal-api        Up (healthy)        0.0.0.0:5000->8080/tcp
# hexagonal-worker     Up (healthy)        (no ports)
```

⏹️ **If sqlserver shows "starting" or "unhealthy", wait another 10 seconds and check again**

---

## 🔍 Step 3: Verify SQL Server Connection

```bash
# Test SQL Server is accessible
docker exec hexagonal-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost \
  -U sa \
  -P HexagonalLab@2024! \
  -C \
  -Q "SELECT @@VERSION"
```

**Expected output:**
```
--------- 
Microsoft SQL Server 2022 (RTM) - 14.0.1000.169 (X64) 
...
```

❌ **If this fails, check docker logs:**
```bash
docker-compose logs sqlserver
```

---

## 🌐 Step 4: Test API Endpoint

```bash
# Test API is running and connected to database
curl http://localhost:5000/api/items

# OR using PowerShell (Windows)
Invoke-WebRequest http://localhost:5000/api/items -UseBasicParsing

# OR using browser
# Open: http://localhost:5000/api/items
```

**Expected output:**
```json
[]
```
(Empty array is OK - database is created but has no items yet)

✅ **If you get a response, the whole stack is working!**

❌ **If connection refused:**
- [ ] Check docker-compose ps shows api is running
- [ ] Check docker-compose logs api for errors
- [ ] Wait another 5 seconds (API might still starting)

---

## 📊 Step 5: Verify Database Exists

```bash
# Check database was created
docker exec hexagonal-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost \
  -U sa \
  -P HexagonalLab@2024! \
  -C \
  -Q "SELECT name FROM sys.databases WHERE name='HexagonalLab'"
```

**Expected output:**
```
name
---------------------------
HexagonalLab
```

---

## 🔄 Step 6: View Container Logs (Troubleshooting)

```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f sqlserver
docker-compose logs -f api
docker-compose logs -f worker

# Last 50 lines
docker-compose logs --tail=50 api
```

**Key log indicators:**
- API: `"Listening on http://+:8080"`
- Worker: `"Worker started"`
- SQL Server: `"SQL Server is now ready for client connections"`

---

## 📁 Step 7: Verify File Structure

Ensure all Docker-related files were created:

```bash
# Windows PowerShell
@(
  "docker-compose.yml",
  ".env.docker",
  ".dockerignore",
  "src/HexagonalLab.API/Dockerfile",
  "src/HexagonalLab.Worker/Dockerfile",
  "scripts/init-db.sql",
  "DOCKER.md",
  "DOCKER_QUICKSTART.md",
  "DOC_IA/ARCHITECTURE_DOCKER.md"
) | ForEach-Object { if (Test-Path $_) { "✅ $_" } else { "❌ $_" } }

# OR Linux/Mac
for file in \
  docker-compose.yml \
  .env.docker \
  .dockerignore \
  src/HexagonalLab.API/Dockerfile \
  src/HexagonalLab.Worker/Dockerfile \
  scripts/init-db.sql \
  DOCKER.md \
  DOCKER_QUICKSTART.md \
  DOC_IA/ARCHITECTURE_DOCKER.md; do
  [ -f "$file" ] && echo "✅ $file" || echo "❌ $file"
done
```

---

## 🛑 Step 8: Stop Containers (Cleanup)

When done testing:

```bash
# Stop all containers (data persists)
docker-compose down

# Restart later
docker-compose --env-file .env.docker up -d

# Complete cleanup (removes data!)
docker-compose down -v
```

---

## ✅ Full Verification Checklist

- [ ] Containers running (docker-compose ps shows all healthy)
- [ ] SQL Server connects (sqlcmd test passes)
- [ ] Database created (HexagonalLab database exists)
- [ ] API responds (curl /api/items returns JSON)
- [ ] All Docker files exist
- [ ] No errors in docker-compose logs

---

## 🎓 What Just Happened?

You successfully deployed:

1. **SQL Server 2022** - Running in container, persisting data to volume
2. **HexagonalLab.API** - Running in container, listening on port 5000
3. **HexagonalLab.Worker** - Running as background service in container
4. **Networking** - All communicating via internal Docker network
5. **Configuration** - All settings via environment variables

**Core Unchanged:** HexagonalLab.Core code was NOT modified. Only:
- Input Adapters now in containers
- Output Adapters connect to containerized SQL Server
- Bootstrap/DI reads connection string from environment

---

## 🚀 Next Steps

### Option A: Continue Development Locally (Without Docker)
```bash
# Stop containers
docker-compose down

# Run API locally
cd src/HexagonalLab.API
dotnet run
# Uses appsettings.Development.json (LocalDB connection)
```

### Option B: Develop With Docker
```bash
# Keep containers running
docker-compose logs -f api

# Make code changes locally
# Rebuild and restart containers when needed
docker-compose up -d --build
```

### Option C: Deploy to Production
See `DOC_IA/INITIATIVE_DOCKER_SETUP.md` section "Future Phases"

---

## 📞 Help / Troubleshooting

### Container won't start
```bash
docker-compose down -v
docker-compose up -d
```

### Port already in use
Edit `.env.docker`:
```env
API_PORT=5001           # Instead of 5000
DB_PORT=1434            # Instead of 1433
```

### Out of disk space
```bash
docker system prune -a  # Remove unused images
docker volume prune     # Remove unused volumes
```

### Database is empty
This is normal! The database exists but has no data yet.
Test by:
```bash
# Create an item via API (POST request)
# OR add seed data to scripts/init-db.sql
```

---

## 📚 Documentation Links

- **Complete Guide:** [DOCKER.md](./DOCKER.md)
- **Quick Reference:** [DOCKER_QUICKSTART.md](./DOCKER_QUICKSTART.md)
- **Architecture Alignment:** [DOC_IA/ARCHITECTURE_DOCKER.md](./DOC_IA/ARCHITECTURE_DOCKER.md)
- **Initiative Details:** [DOC_IA/INITIATIVE_DOCKER_SETUP.md](./DOC_IA/INITIATIVE_DOCKER_SETUP.md)

---

**Status: ✅ Complete - All containers running and ready for testing**
