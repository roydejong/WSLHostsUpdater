using WSLHostsUpdater.IO;

namespace WSLHostsUpdater;

public class Worker : BackgroundService
{
    private const int CheckInterval = 30000;
    
    private readonly ILogger<Worker> _logger;
    private readonly WslInterface _wsl;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
        _wsl = new WslInterface(logger);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Refresh();
            await Task.Delay(CheckInterval, stoppingToken);
        }
    }

    private async Task<bool> Refresh()
    {
        var wslIsRunning = await _wsl.TryGetIsWslRunning();

        if (!wslIsRunning)
        {
            _logger.LogWarning("WSL default instance is not running, skipping refresh");
            return false;
        }

        var ipAddress = await _wsl.TryGetAddress();

        if (ipAddress is null)
        {
            _logger.LogWarning("Failed to get IP address from WSL default instance, skipping refresh");
            return false;
        }
        
        _logger.LogInformation("WSL instance is running ({Address}), refreshing...", ipAddress);
        // TODO
        return true;
    }
}