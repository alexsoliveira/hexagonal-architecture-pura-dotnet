# ──────────────────────────────────────────────────────────────
# Dockerfile for HexagonalLab Worker
# Background service with same structure as API
# ──────────────────────────────────────────────────────────────

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

# Copy solution and project files
COPY ["HexagonalLab.NET10.slnx", "."]
COPY ["src/HexagonalLab.Core/HexagonalLab.Core.csproj", "src/HexagonalLab.Core/"]
COPY ["src/HexagonalLab.Infrastructure/HexagonalLab.Infrastructure.csproj", "src/HexagonalLab.Infrastructure/"]
COPY ["src/HexagonalLab.Worker/HexagonalLab.Worker.csproj", "src/HexagonalLab.Worker/"]

# Restore dependencies
RUN dotnet restore "src/HexagonalLab.Worker/HexagonalLab.Worker.csproj"

# Copy source code
COPY . .

# Build application
RUN dotnet build "src/HexagonalLab.Worker/HexagonalLab.Worker.csproj" -c Release -o /app/build

# Publish
RUN dotnet publish "src/HexagonalLab.Worker/HexagonalLab.Worker.csproj" -c Release -o /app/publish

# ──────────────────────────────────────────────────────────────
# Runtime stage
# ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app

# Copy published files
COPY --from=build /app/publish .

# Set environment variables for unbuffered logging
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
ENV DOTNET_SYSTEM_CONSOLE_ALLOWDIRECTPARSE=true
ENV DOTNET_EnableWriteXorExecuteMemory=false
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_CLI_PREFER_EXITCODE_FOR_FAILURES=true

# Create startup script that logs to both stdout and file
RUN echo '#!/bin/sh' > /app/start.sh && \
    echo 'mkdir -p /app/logs' >> /app/start.sh && \
    echo 'echo "[$(date)] Starting Worker..." >&1' >> /app/start.sh && \
    echo 'exec dotnet HexagonalLab.Worker.dll 2>&1 | tee -a /app/logs/worker.log' >> /app/start.sh && \
    chmod +x /app/start.sh

# Run application with startup script
ENTRYPOINT ["/app/start.sh"]
