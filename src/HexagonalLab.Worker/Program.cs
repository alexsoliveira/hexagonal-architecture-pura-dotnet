using HexagonalLab.Core.Ports;
using HexagonalLab.Core.UseCases;
using HexagonalLab.Infrastructure.Data;
using HexagonalLab.Infrastructure.Repositories;
using HexagonalLab.Worker.Services;
using Microsoft.EntityFrameworkCore;

// ========================================================================
// BOOTSTRAP - Worker Service Configuration
// ========================================================================
// NOTA: Este é o ÚNICO lugar onde DI é configurado!
// Se trocar adapter: apenas AQUI muda, nunca no Core.
// 
// PADRÃO DE PLUGABILIDADE:
// - Phase 3 (API Adapter)
// - Phase 5 (Worker Adapter) ← ESTA AQUI
// - Phase 6+ (CLI, gRPC, etc.)
// 
// TODO: Core é 100% idêntico em TODOS os adapters!
// ========================================================================

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // ─────────────────────────────────────────────────────────────────
        // 1. Add Logging
        // ─────────────────────────────────────────────────────────────────
        services.AddLogging(config =>
        {
            config.AddConsole();
        });

        // ─────────────────────────────────────────────────────────────────
        // 2. Register Core UseCases (Input Ports)
        // ─────────────────────────────────────────────────────────────────
        // Worker Adapter = SEGUNDA entrada (Input Adapter #2)
        // API (Phase 3) era a primeira entrada
        // Core é AGNÓSTICO de qual adapter está usando!
        // 
        // PLUGABILIDADE: Worker pode PROCESSAR (ProcessItemUseCase)
        // ou OBTER (GetItemUseCase) dependendo da necessidade!
        // Aqui escolhemos PROCESSAR (ProcessItemUseCase) para demonstrar
        // ─────────────────────────────────────────────────────────────────

        services.AddScoped<IItemInputPort, ProcessItemUseCase>();  // ✅ Worker PROCESSA itens, não apenas lê
        services.AddScoped<IGetAllItemsInputPort, GetAllItemsUseCase>();        services.AddScoped<IUpdateItemStatusInputPort, UpdateItemStatusUseCase>();  // ✅ Worker PERSISTE status
        // ─────────────────────────────────────────────────────────────────
        // 3. Register Output Port Adapters (Real Database)
        // ─────────────────────────────────────────────────────────────────
        // Phase 3-5: EF Core adapter (banco de dados real)
        // ─────────────────────────────────────────────────────────────────

        var connectionString = context.Configuration.GetConnectionString("DefaultConnection");
        
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(
                connectionString,
                sqlOptions =>
                {
                    // ✅ Retry strategy for transient SQL Server errors
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5
                    );
                    
                    // ✅ Increase command timeout from default 30s to 300s (5 minutes)
                    sqlOptions.CommandTimeout(300);
                }
            );
        });

        services.AddScoped<IItemRepositoryPort, EfCoreRepositoryAdapter>();

        // ─────────────────────────────────────────────────────────────────
        // 4. Register BackgroundService (Worker)
        // ─────────────────────────────────────────────────────────────────
        services.AddHostedService<ItemProcessingWorker>();
    });

var host = builder.Build();

await host.RunAsync();
