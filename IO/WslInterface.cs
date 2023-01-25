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

            var stdErr = await TryReadCleanStd(process.StandardError);
            var stdOut = await TryReadCleanStd(process.StandardOutput);

            var haveStdErr = !string.IsNullOrEmpty(stdErr);
            var haveStdOut = !string.IsNullOrEmpty(stdOut);

            if (haveStdErr || !haveStdErr)
            {
                _logger.LogWarning("[WSL] WSL Interface did not get a result, or got stderr, details:\r\n" +
                                   " • Command executed: \"wsl {Args}\"\r\n" +
                                   " • Result from stdout: \"{StdOut}\"\r\n" +
                                   " • Result from stderr: \"{StdErr}\"",
                    args, stdOut, stdErr);
            }

            if (haveStdOut)
            {
                return stdOut;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("[WSL] WSL Interface execute failed: {Ex}", ex);
        }

        return null;
    }

    private static async Task<string?> TryReadCleanStd(StreamReader reader)
    {
        if (reader.Peek() == -1)
            return null;

        var stdStr = await reader.ReadToEndAsync();

        // Remove null bytes that may appear
        stdStr = stdStr.Replace("\0", string.Empty);

        // Remove excess newlines that may appear
        return stdStr.Trim();
    }
}