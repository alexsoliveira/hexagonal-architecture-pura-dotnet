using HexagonalLab.Core.Ports;
using HexagonalLab.Core.UseCases;
using HexagonalLab.Infrastructure.Data;
using HexagonalLab.Infrastructure.Repositories;
using HexagonalLab.Worker.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

// ========================================================================
// BOOTSTRAP - Worker Service Configuration with Serilog
// ========================================================================
// Setup Serilog FIRST for maximum logging coverage
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "/app/logs/worker.log",
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level:u3}] {Message:lj}{NewLine}{Exception}",
        rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("═══════════════════════════════════════════════════");
    Log.Information("🚀 HEXAGONAL LAB WORKER - STARTING UP");
    Log.Information("═══════════════════════════════════════════════════");
    Log.Information("Phase 5: Multi-Adapter Pattern (Worker Input Adapter)");

    var builder = Host.CreateDefaultBuilder(args)
        .UseSerilog()  // Use Serilog instead of default logging
        .ConfigureServices((context, services) =>
        {
        // ─────────────────────────────────────────────────────────────────
        // 1. Register Core UseCases (Input Ports)
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
        services.AddScoped<IGetAllItemsInputPort, GetAllItemsUseCase>();
        services.AddScoped<IUpdateItemStatusInputPort, UpdateItemStatusUseCase>();  // ✅ Worker PERSISTE status
        
        // ─────────────────────────────────────────────────────────────────
        // 2. Register Output Port Adapters (Real Database)
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
        // 3. Register BackgroundService (Worker)
        // ─────────────────────────────────────────────────────────────────
        services.AddHostedService<ItemProcessingWorker>();
    });

    var host = builder.Build();

    // DEBUG: Verify host is built  
    Log.Information("[AFTER BUILD] Host was built successfully");

    await host.RunAsync();

    Log.Information("[AFTER RUNASYNC] Host.RunAsync completed");
}
catch (Exception ex)
{
    Log.Fatal(ex, "💥 FATAL: Application terminated unexpectedly");
    Environment.Exit(1);
}
finally
{
    Log.Information("═══════════════════════════════════════════════════");
    Log.Information("🛑 WORKER SHUTDOWN - See logs above for details");
    Log.Information("═══════════════════════════════════════════════════");
    await Log.CloseAndFlushAsync();  // Ensure all logs are written before exit
}
