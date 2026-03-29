namespace HexagonalLab.Worker.Services;

using HexagonalLab.Core.Ports;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

/// <summary>
/// Background Worker Service: Input Adapter #2
/// 
/// PADRÃO CRÍTICO DE PLUGABILIDADE:
/// ✅ BackgroundService (IHostedService) - Segundo Input Adapter
/// ✅ Recebe IItemInputPort via DI (exato como API faz em Phase 3)
/// ✅ Não diferente de API Adapter - mesmo contrato!
/// ✅ Orquestra: "Timer → Chama UseCase"
/// ✅ Core sem saber que é worker!
/// 
/// COMPARAÇÃO com API Adapter (Phase 3):
/// - API Adapter: HTTP Request → Input Port
/// - Worker Adapter: Timer/Event → Input Port
/// 
/// E... Core é 100% idêntico em AMBOS os casos!
/// demonstrando plugabilidade máxima da arquitetura.
/// </summary>
public class ItemProcessingWorker : BackgroundService
{
    private readonly ILogger<ItemProcessingWorker> _logger;
    private readonly IItemInputPort _useCase;
    private readonly IGetAllItemsInputPort _getAllUseCase;
    private readonly IUpdateItemStatusInputPort _updateStatusUseCase;
    private Timer? _timer;

    public ItemProcessingWorker(
        ILogger<ItemProcessingWorker> logger,
        IItemInputPort useCase,
        IGetAllItemsInputPort getAllUseCase,
        IUpdateItemStatusInputPort updateStatusUseCase)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _useCase = useCase ?? throw new ArgumentNullException(nameof(useCase));
        _getAllUseCase = getAllUseCase ?? throw new ArgumentNullException(nameof(getAllUseCase));
        _updateStatusUseCase = updateStatusUseCase ?? throw new ArgumentNullException(nameof(updateStatusUseCase));
    }

    /// <summary>
    /// Chamado na startup do host.
    /// Inicializa o timer para processamento periódico.
    /// </summary>
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("ItemProcessingWorker is starting... ⏱️");
        _logger.LogInformation("This demonstrates Phase 5 - Multi-Adapter Pattern:");
        _logger.LogInformation("→ Same Core UseCase");
        _logger.LogInformation("→ Different Input Adapter (Worker vs API)");
        _logger.LogInformation("→ Same Output Port (Database)");
        _logger.LogInformation("→ ZERO changes to Core! 🎉");

        // Timer: inicia após 10 segundos (aguarda migrations), depois processa a cada 20 segundos
        _timer = new Timer(
            async _ => await DoWork(cancellationToken),
            null,
            TimeSpan.FromSeconds(10),          // Delay inicial de 10s para garantir que migrations estão prontas
            TimeSpan.FromSeconds(20));         // Repete a cada 20s

        await base.StartAsync(cancellationToken);
    }

    /// <summary>
    /// BackgroundService.ExecuteAsync() - mantém o serviço rodando.
    /// O timer é que faz o trabalho real em DoWork().
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Mantém o serviço rodando até ser parado
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }

        _logger.LogInformation("ItemProcessingWorker is stopping...");
    }

    /// <summary>
    /// A MAGIA: Chama UseCase (Input Port) diretamente.
    /// Exatamente como um API Endpoint faria!
    /// 
    /// CRÍTICO PARA DEMONSTRAR PLUGABILIDADE:
    /// Este código é IDENTICAMENTE estruturado como
    /// um endpoint em ItemEndpoints.cs (Phase 3).
    /// 
    /// A única diferença: entrada é timer, não HTTP.
    /// Mas Core não sabe nem se importa!
    /// 
    /// MUDANÇA: Agora busca itens REAIS do banco em vez de hardcoded!
    /// </summary>
    private async Task DoWork(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing items at {time} ⚙️", DateTimeOffset.Now);

            // ✅ MUDANÇA: Buscar itens REAIS do banco usando GetAll UseCase
            // Antes: var itemsToProcess = new[] { "ITEM-001", "ITEM-002", "ITEM-003" };  // ❌ Hardcoded
            // Agora:
            var allItems = await _getAllUseCase.GetAllAsync();
            
            if (!allItems.Any())
            {
                _logger.LogInformation("ℹ️ No items found in database to process");
                return;
            }

            _logger.LogInformation("📊 Found {Count} items to process", allItems.Count());

            foreach (var item in allItems)
            {
                try
                {
                    // ✅ CHAMA INPUT PORT (IDENTICAMENTE COMO FAZ HTTP ENDPOINT!)
                    // Veja ItemEndpoints.cs para comparação
                    var result = await _useCase.ProcessAsync(item.Id);

                    if (result.Status == "Processed")
                    {
                        _logger.LogInformation("✅ Item {ItemId} ({Name}) processed successfully", item.Id, item.Name);
                        
                        // ✅ PERSISTE o novo status no banco de dados
                        // Demonstra a plugabilidade: Worker chama 2 Input Ports
                        var updateResult = await _updateStatusUseCase.UpdateStatusAsync(item.Id, "Processed");
                        _logger.LogInformation("✅ Item {ItemId} status persisted: {Message}", item.Id, updateResult.Message);
                    }
                    else
                    {
                        _logger.LogWarning("⚠️ Item {ItemId} ({Name}) returned status: {Status}", item.Id, item.Name, result.Status);
                    }
                }
                catch (ArgumentException ex)
                {
                    _logger.LogWarning("⚠️ Validation error processing item {ItemId}: {Message}", item.Id, ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error processing item {ItemId}", item.Id);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Esperado durante shutdown
        }
        catch (Exception ex)
        {
            if (ex.InnerException?.Message?.Contains("Invalid object name 'Items'") == true)
            {
                _logger.LogError("❌ Database schema not ready yet. Tables may still be migrating. Retrying in next cycle...");
            }
            else
            {
                _logger.LogError(ex, "❌ Unexpected error in DoWork");
            }
        }
    }

    /// <summary>
    /// Cleanup quando o serviço é parado.
    /// </summary>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("ItemProcessingWorker is stopping...");
        _timer?.Dispose();
        await base.StopAsync(cancellationToken);
    }

    /// <summary>
    /// Cleanup completo.
    /// </summary>
    public override void Dispose()
    {
        _timer?.Dispose();
        base.Dispose();
    }
}
