using WSLHostsUpdater;

var binPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.RelativeSearchPath ?? "");

IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService()
    .ConfigureLogging(loggerFactory =>
    {
        loggerFactory.AddEventLog();
    })
    .ConfigureServices(services => { services.AddHostedService<Worker>(); })
    .ConfigureAppConfiguration(config =>
    {
        // Load appsettings.json from the executable path, and automatically reload it on change
        config.SetBasePath(binPath);
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        config.AddEnvironmentVariables();
    })
    .Build();

await host.RunAsync();