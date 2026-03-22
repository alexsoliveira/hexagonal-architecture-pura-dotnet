# 🚀 DOCKER QUICKSTART - HexagonalLab

## One-Command Setup

```bash
# Start all containers
docker-compose --env-file .env.docker up -d

# Wait ~15 seconds for SQL Server to be ready
```

## Verify Everything is Working

```bash
# Check container status
docker-compose ps

# Expected output:
# NAME                 STATUS              PORTS
# hexagonal-sqlserver  healthy (1/1)       0.0.0.0:1433->1433/tcp
# hexagonal-api        Up (healthy)        0.0.0.0:5000->8080/tcp
# hexagonal-worker     Up (healthy)        (no ports)
```

## Test API

```bash
# Simple test
curl http://localhost:5000/api/items

# Should return: [] or existing items
```

## Common Tasks

### Windows PowerShell
```bash
# Use helper script
.\scripts\docker-helper.ps1 start       # Start
.\scripts\docker-helper.ps1 logs        # View logs
.\scripts\docker-helper.ps1 test-db     # Test SQL connection
.\scripts\docker-helper.ps1 stop        # Stop
.\scripts\docker-helper.ps1 clean       # Remove all
.\scripts\docker-helper.ps1 rebuild     # Rebuild images
```

### Linux/Mac Bash
```bash
# Make script executable
chmod +x ./scripts/docker-helper.sh

# Use helper script
./scripts/docker-helper.sh start        # Start
./scripts/docker-helper.sh logs         # View logs
./scripts/docker-helper.sh test-db      # Test SQL connection
./scripts/docker-helper.sh stop         # Stop
./scripts/docker-helper.sh clean        # Remove all
./scripts/docker-helper.sh rebuild      # Rebuild images
```

## Key Files

| File | Purpose |
|------|---------|
| `docker-compose.yml` | Container orchestration |
| `.env.docker` | Environment variables |
| `.dockerignore` | Build optimization |
| `src/*/Dockerfile` | Container image recipes |
| `scripts/init-db.sql` | Database initialization |
| `DOCKER.md` | Full documentation |
| `DOC_IA/ARCHITECTURE_DOCKER.md` | Architectural alignment |

## Connection Strings

**From Host Machine:**
```
Server=localhost;Database=HexagonalLab;User Id=sa;Password=HexagonalLab@2024!;TrustServerCertificate=true;
```

**From Inside Container (API/Worker):**
```
Server=sqlserver;Database=HexagonalLab;User Id=sa;Password=HexagonalLab@2024!;TrustServerCertificate=true;
```

## Ports

| Service | External | Internal | Notes |
|---------|----------|----------|-------|
| API | 5000 | 8080 | HTTP endpoint |
| SQL Server | 1433 | 1433 | Database |
| Worker | - | - | No external port |

## ⚠️ Important Notes

1. **First startup**: SQL Server health check takes ~10-15 seconds
2. **Password**: Default `HexabolabLab@2024!` is for dev only - change in production
3. **Database**: Automatically created by init script
4. **Migrations**: Run automatically on API container start
5. **Data Persistence**: SQL Server data persists in Docker volume `hexagonal_sqlserver_data`

## 📖 Full Documentation

See:
- [DOCKER.md](./DOCKER.md) - Complete setup guide
- [DOC_IA/ARCHITECTURE_DOCKER.md](./DOC_IA/ARCHITECTURE_DOCKER.md) - Hexagonal + Docker alignment

## Troubleshooting

### SQL Server won't connect
```bash
# Check container is healthy
docker-compose ps

# Check logs
docker-compose logs sqlserver

# Wait and try again (health checks can take time)
sleep 15
docker-compose ps
```

### API won't start
```bash
# Check logs
docker-compose logs api

# Most common issues:
# 1. SQL Server not healthy yet - wait
# 2. Connection string wrong - check .env.docker
# 3. Port already in use - change API_PORT in .env.docker
```

### Port already in use
```bash
# Edit .env.docker
API_PORT=5001          # Instead of 5000
DB_PORT=1434           # Instead of 1433

# Restart
docker-compose down
docker-compose up -d
```

---

**Ready to go!** 🎉
