using WSLHostsUpdater.IO;

namespace WSLHostsUpdater;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfigurationSection _config;
    private readonly WslInterface _wsl;

    public Worker(ILogger<Worker> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config.GetSection("Service");
        _wsl = new WslInterface(logger);
    }

    public bool ConfigCheckWslRunning => _config["CheckWslRunning"] == "True";
    public int ConfigUpdateIntervalMs => int.Parse(_config["UpdateInterval"]);
    public string[] ConfigHostnames => _config.GetSection("Hostnames").Get<string[]>();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Starting worker (CheckWslRunning={CheckWslRunning}, UpdateInterval={UpdateInterval}, Hostnames={Hostnames})",
            ConfigCheckWslRunning, ConfigUpdateIntervalMs, ConfigHostnames);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Refresh();
            await Task.Delay(ConfigUpdateIntervalMs, stoppingToken);
        }
    }

    private async Task<bool> Refresh()
    {
        // Check if running
        if (ConfigCheckWslRunning)
        {
            var wslIsRunning = await _wsl.TryGetIsWslRunning();

            if (!wslIsRunning)
            {
                _logger.LogDebug("WSL default instance is not running, skipping refresh");
                return false;
            }
        }

        // Get IP address
        var ipAddress = await _wsl.TryGetAddress();

        if (ipAddress is null)
        {
            _logger.LogError("Failed to get IP address from WSL default instance, skipping refresh");
            return false;
        }

        _logger.LogInformation("WSL instance is running ({Address}), refreshing...", ipAddress);
        
        // Read hosts file
        var hostsFile = HostsFile.TryReadFile();

        if (hostsFile is null)
        {
            _logger.LogError("Failed to open or parse hosts file");
            return false;
        }
        
        // Stage changes
        foreach (var hostName in ConfigHostnames)
            hostsFile.StageManagedLine(hostName, ipAddress.ToString());

        // Write hosts file
        try
        {
            hostsFile.Save();
            _logger.LogInformation("Updated hosts file successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to update hosts file: {Exception}", ex);
            return false;
        }
    }
}