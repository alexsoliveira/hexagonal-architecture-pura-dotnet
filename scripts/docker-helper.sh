#!/bin/bash
# ──────────────────────────────────────────────────────────────
# Docker Management Script for HexagonalLab
# Bash helper for Linux/Mac users
# ──────────────────────────────────────────────────────────────

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
COMMAND="${1:-ps}"

# Color codes
CYAN='\033[0;36m'
YELLOW='\033[1;33m'
GREEN='\033[0;32m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Helper functions
write_header() {
    echo -e "${CYAN}═══════════════════════════════════════════════════${NC}"
    echo -e "${YELLOW}  $1${NC}"
    echo -e "${CYAN}═══════════════════════════════════════════════════${NC}\n"
}

start_containers() {
    write_header "Starting HexagonalLab Docker Containers"
    
    if [ -f "$SCRIPT_DIR/.env.docker" ]; then
        docker-compose --env-file "$SCRIPT_DIR/.env.docker" up -d
    else
        echo -e "${YELLOW}⚠️  .env.docker not found, using defaults${NC}"
        docker-compose up -d
    fi
    
    echo -e "${GREEN}✅ Containers starting... waiting for SQL Server health check${NC}"
    sleep 5
    
    show_container_status
}

stop_containers() {
    write_header "Stopping HexagonalLab Docker Containers"
    
    docker-compose down
    echo -e "${GREEN}✅ Containers stopped${NC}"
}

restart_containers() {
    write_header "Restarting HexagonalLab Docker Containers"
    
    stop_containers
    sleep 2
    start_containers
}

show_logs() {
    write_header "Docker Compose Logs"
    docker-compose logs -f
}

show_container_status() {
    write_header "Container Status"
    docker-compose ps
}

clean_resources() {
    write_header "Cleaning Docker Resources"
    
    echo -e "${YELLOW}Stopping containers...${NC}"
    docker-compose down -v
    
    echo -e "${YELLOW}Removing unused Docker resources...${NC}"
    docker system prune -f
    
    echo -e "${GREEN}✅ Cleanup complete${NC}"
}

test_database_connection() {
    write_header "Testing SQL Server Connection"
    
    PASSWORD="HexagonalLab@2024!"
    
    if docker exec hexagonal-sqlserver /opt/mssql-tools18/bin/sqlcmd \
        -S localhost \
        -U sa \
        -P "$PASSWORD" \
        -C \
        -Q "SELECT @@VERSION" &>/dev/null; then
        echo -e "${GREEN}✅ SQL Server is running and accessible${NC}"
        docker exec hexagonal-sqlserver /opt/mssql-tools18/bin/sqlcmd \
            -S localhost \
            -U sa \
            -P "$PASSWORD" \
            -C \
            -Q "SELECT @@VERSION"
    else
        echo -e "${RED}❌ Failed to connect to SQL Server${NC}"
    fi
}

rebuild_images() {
    write_header "Rebuilding Docker Images"
    
    echo -e "${YELLOW}Building images without cache...${NC}"
    docker-compose build --no-cache
    
    echo -e "${GREEN}✅ Images rebuilt successfully${NC}"
    
    echo -e "${YELLOW}Starting containers...${NC}"
    docker-compose up -d
    
    show_container_status
}

# Execute requested command
case "$COMMAND" in
    start)      start_containers ;;
    stop)       stop_containers ;;
    restart)    restart_containers ;;
    logs)       show_logs ;;
    ps)         show_container_status ;;
    clean)      clean_resources ;;
    test-db)    test_database_connection ;;
    rebuild)    rebuild_images ;;
    *)
        echo "Usage: $0 {start|stop|restart|logs|ps|clean|test-db|rebuild}"
        echo ""
        echo "Commands:"
        echo "  start      - Start all containers"
        echo "  stop       - Stop all containers"
        echo "  restart    - Restart all containers"
        echo "  logs       - Show container logs (follow)"
        echo "  ps         - Show container status"
        echo "  clean      - Remove all containers and volumes"
        echo "  test-db    - Test SQL Server connection"
        echo "  rebuild    - Rebuild images and start containers"
        exit 1
        ;;
esac
