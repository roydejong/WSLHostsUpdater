using System.Text;

namespace WSLHostsUpdater.IO;

public class HostsFile
{
    private const string MarkerCommentText = "WSLHostsUpdater";
    
    private string _rawContents;
    private Dictionary<string, string> _hostMap;
    private List<HostsFileLine> _managedLines;
    private Dictionary<string, string> _stagedEntries;

    public List<HostsFileLine> Lines { get; protected set; }

    protected HostsFile(string fileContents)
    {
        _rawContents = fileContents;
        _hostMap = new();
        _managedLines = new();
        _stagedEntries = new();
        
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

            if (parsedLine.CommentText == MarkerCommentText)
            {
                _managedLines.Add(parsedLine);
            }
        }
    }

    #region Read API
    public string? TryResolveHostName(string hostName)
    {
        return _hostMap.TryGetValue(hostName.ToLowerInvariant(), out var result) ? result : null;
    }

    public void StageManagedLine(string hostName, string targetAddress)
    {
        _stagedEntries[hostName] = targetAddress;
    }
    #endregion

    #region Write API

    private List<HostsFileLine> GenerateManagedLines()
    {
        return _stagedEntries.Select(entry => 
            new HostsFileLine(entry.Value, new List<string>() {entry.Key}, MarkerCommentText))
            .ToList();
    }
    
    public void Save()
    {
        var output = new StringBuilder();

        // Insert original host file lines
        foreach (var line in Lines)
        {
            if (_managedLines.Contains(line))
                // Managed lines need to be fully rebuilt
                continue;

            output.AppendLine(line.RawLine);
        }
        
        // (Re)generate and insert managed lines
        foreach (var line in GenerateManagedLines())
            output.AppendLine(line.ToString());

        // Write out
        File.WriteAllText(GetFilePath(), output.ToString().Trim());
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