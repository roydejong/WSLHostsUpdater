using System.Text;

namespace WSLHostsUpdater.IO;

public class HostsFile
{
    private const string MarkerCommentText = "WSLHostsUpdater";
    
    private string _rawContents;
    private Dictionary<string, string> _hostMap;
    private HostsFileLine? _managedLine;
        
    public List<HostsFileLine> Lines { get; protected set; }
    public bool DirtyFlag { get; protected set; }

    protected HostsFile(string fileContents)
    {
        _rawContents = fileContents;
        _hostMap = new();
        _managedLine = null;
        
        Lines = new();
        DirtyFlag = false;
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
                _managedLine = parsedLine;
            }
        }
        
        DirtyFlag = false;
    }

    #region Read API
    public string? TryResolveHostName(string hostName)
    {
        return _hostMap.TryGetValue(hostName.ToLowerInvariant(), out var result) ? result : null;
    }

    public void AddOrUpdate(string hostName, string targetAddress)
    {
        if (_managedLine == null)
        {
            _managedLine = new HostsFileLine(targetAddress, new List<string>() {hostName});
            Lines.Add(_managedLine);
            DirtyFlag = true;
        }
        else
        {
            if (!_managedLine.HostNames.Contains(hostName))
            {
                _managedLine.HostNames.Add(hostName);
                DirtyFlag = true;
            }

            if (_managedLine.TargetAddress != targetAddress)
            {
                _managedLine.TargetAddress = targetAddress;
                DirtyFlag = true;
            }
        }

        _managedLine.CommentText = MarkerCommentText;
        
        // Remove any duplicate lines for this host
        Lines.RemoveAll(line => line.Type == HostsFileLine.LineType.RegularEntry
                                && line.CommentText != MarkerCommentText
                                && line.HostNames.Contains(hostName));
    }
    #endregion

    #region Write API
    public void Save()
    {
        if (!DirtyFlag)
            return;

        var output = new StringBuilder();

        foreach (var line in Lines)
        {
            if (line == _managedLine)
                output.AppendLine(line.ToString());
            else
                output.AppendLine(line.RawLine);
        }

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