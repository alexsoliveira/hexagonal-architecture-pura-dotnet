# ──────────────────────────────────────────────────────────────
# Dockerfile for HexagonalLab API
# Multi-stage build for optimal image size
# ──────────────────────────────────────────────────────────────

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

# Copy solution and project files
COPY ["HexagonalLab.NET10.slnx", "."]
COPY ["src/HexagonalLab.Core/HexagonalLab.Core.csproj", "src/HexagonalLab.Core/"]
COPY ["src/HexagonalLab.Infrastructure/HexagonalLab.Infrastructure.csproj", "src/HexagonalLab.Infrastructure/"]
COPY ["src/HexagonalLab.API/HexagonalLab.API.csproj", "src/HexagonalLab.API/"]

# Restore dependencies
RUN dotnet restore "src/HexagonalLab.API/HexagonalLab.API.csproj"

# Copy source code
COPY . .

# Build application
RUN dotnet build "src/HexagonalLab.API/HexagonalLab.API.csproj" -c Release -o /app/build

# Publish
RUN dotnet publish "src/HexagonalLab.API/HexagonalLab.API.csproj" -c Release -o /app/publish

# ──────────────────────────────────────────────────────────────
# Runtime stage
# ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# Copy published files
COPY --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Healthcheck
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD dotnet-health 2>/dev/null || exit 1

# Expose port
EXPOSE 8080

# Run application
ENTRYPOINT ["dotnet", "HexagonalLab.API.dll"]
