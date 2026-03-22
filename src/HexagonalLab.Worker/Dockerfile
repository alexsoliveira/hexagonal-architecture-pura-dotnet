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

# Set environment variable
ENV ASPNETCORE_ENVIRONMENT=Production

# Run application
ENTRYPOINT ["dotnet", "HexagonalLab.Worker.dll"]
