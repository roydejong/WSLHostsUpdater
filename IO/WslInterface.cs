using System.Diagnostics;
using System.Net;
using System.Text;

namespace WSLHostsUpdater.IO;

public class WslInterface
{
    protected ILogger _logger;

    public WslInterface(ILogger logger)
    {
        _logger = logger;
    }

    public async Task<bool> TryGetIsWslRunning()
    {
        var result = await Execute("-l -v");

        if (result == null)
            return false;

        var lines = result.Split(Environment.NewLine);

        foreach (var line in lines)
        {
            if (line.StartsWith('*'))
            {
                // Default instance, this is what we're looking for
                return line.Contains("Running"); 
            }
        }

        _logger.LogWarning("are you kitten me");
        return false;
    }
    
    public async Task<IPAddress?> TryGetAddress()
    {
        var result = await Execute("hostname -I");

        if (result is null)
            return null;
        
        if (IPAddress.TryParse(result.Trim(), out var ipAddress))
            return ipAddress;

        return null;
    }

    internal async Task<string?> Execute(string args)
    {
        var psi = new ProcessStartInfo("wsl", args);
        psi.RedirectStandardOutput = true;
        psi.StandardOutputEncoding = Encoding.UTF8;
        psi.RedirectStandardError = true;
        psi.StandardErrorEncoding = Encoding.UTF8;
        psi.WindowStyle = ProcessWindowStyle.Hidden;
        psi.CreateNoWindow = true;

        try
        {
            var process = new Process();
            process.StartInfo = psi;
            process.Start();
            
            _logger.LogDebug("[WSL] Execute: wsl {Args}", args);

            await process.WaitForExitAsync();
            
            if (process.StandardError.Peek() > -1)
            {
                var stderr = await process.StandardError.ReadToEndAsync();
                _logger.LogWarning("[WSL] stderr: {Text}", stderr);
            }

            if (process.StandardOutput.Peek() > -1)
            {
                var stdOut = await process.StandardOutput.ReadToEndAsync();
                stdOut = stdOut.Replace("\0", string.Empty); // remove null bytes (why are they here??)
                return stdOut;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("[WSL] Execute failed: {Ex}", ex);
        }
        
        return null;
    }
}