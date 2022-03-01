namespace WSLHostsUpdater.IO;

public class HostsFile
{
    private string _rawContents;
    private Dictionary<string, string> _hostMap;
        
    public List<HostsFileLine> Lines { get; protected set; }

    protected HostsFile(string fileContents)
    {
        _rawContents = fileContents;
        _hostMap = new();
        
        Lines = new();
    }

    protected void Parse()
    {
        var lines = _rawContents.Split(Environment.NewLine);

        foreach (var lineText in lines)
        {
            var parsedLine = HostsFileLine.Parse(lineText);
            Lines.Add(parsedLine);

            if (parsedLine.Type == HostsFileLine.LineType.RegularEntry && parsedLine.TargetAddress is not null)
            {
                foreach (var hostName in parsedLine.HostNames)
                {
                    _hostMap[hostName.ToLowerInvariant()] = parsedLine.TargetAddress;
                }
            }
        }
    }

    #region Read API
    public string? TryResolveHostName(string hostName)
    {
        return _hostMap.TryGetValue(hostName.ToLowerInvariant(), out var result) ? result : null;
    }
    #endregion
    
    #region Static API
    public static string GetFilePath()
    {
        var systemDir = Environment.GetFolderPath(Environment.SpecialFolder.System);
        return Path.Combine(systemDir, "drivers/etc/hosts");
    }

    public static HostsFile? TryReadFile()
    {
        string fileContents;

        try
        {
            fileContents = File.ReadAllText(GetFilePath());
        }
        catch (IOException)
        {
            return null;
        }

        return FromContents(fileContents);
    }

    public static HostsFile FromContents(string fileContents)
    {
        var file = new HostsFile(fileContents);
        file.Parse();
        return file;
    }
    #endregion
}