using System.Text;

namespace WSLHostsUpdater.IO;

public class HostsFileLine
{
    public enum LineType : byte
    {
        Unknown = 0,
        Empty = 1,
        Comment = 2,
        RegularEntry = 3
    }

    public LineType Type { get; protected set; }
    public string RawLine { get; protected set; }
    public string? TargetAddress { get; set; }
    public List<string> HostNames { get; set; }
    public string? CommentText { get; set; }

    public HostsFileLine(string? targetAddress, List<string>? hostNames, string? commentText = null)
    {
        Type = LineType.RegularEntry;
        RawLine = "";
        TargetAddress = targetAddress;
        HostNames = hostNames ?? new();
        CommentText = commentText;
    }
    
    protected HostsFileLine(string rawLine)
    {
        Type = LineType.Unknown;
        RawLine = rawLine;
        TargetAddress = null;
        HostNames = new List<string>();
        CommentText = null;
    }

    public static HostsFileLine Parse(string lineText)
    {
        var result = new HostsFileLine(lineText);

        if (String.IsNullOrWhiteSpace(lineText))
        {
            result.Type = LineType.Empty;
            return result;
        }
        
        result.Type = LineType.RegularEntry;

        var parseBuffer = "";
        var readingTargetAddress = true;
        var readingHostNames = false;

        for (var i = 0; i < lineText.Length; i++)
        {
            var bChar = lineText[i];
            var bCharIsWhiteSpace = Char.IsWhiteSpace(bChar);

            if (bChar == '#')
            {
                // Special: The rest of the line is a comment
                if (String.IsNullOrEmpty(result.TargetAddress))
                {
                    // Line appears to be a comment in its entirety 
                    result.Type = LineType.Comment;
                }

                result.CommentText = lineText.Substring(i + 1).Trim();
                break;
            }
            
            if (readingTargetAddress)
            {
                // Phase 1: read host name until we hit a tab or space character, the result is our target address
                if (bCharIsWhiteSpace)
                {
                    result.TargetAddress = parseBuffer;
                    
                    parseBuffer = "";
                    readingTargetAddress = false;
                    readingHostNames = true;
                }
                else
                {
                    parseBuffer += bChar;
                }

                continue;
            }
            
            if (readingHostNames)
            {
                // Phase 2: read host names, seperated by spacing, until the end of the line
                if (bCharIsWhiteSpace)
                {
                    if (!String.IsNullOrEmpty(parseBuffer))
                    {
                        result.HostNames.Add(parseBuffer);
                    }
                    
                    parseBuffer = "";
                }
                else
                {
                    parseBuffer += bChar;
                }
            }
        }

        if (!String.IsNullOrEmpty(parseBuffer))
        {
            if (readingTargetAddress)
            {
                result.TargetAddress = parseBuffer;
            }
            else if (readingHostNames)
            {
                result.HostNames.Add(parseBuffer);
            }
        }

        return result;
    }

    public override string ToString()
    {
        if (Type != LineType.RegularEntry)
            return RawLine;

        var result = new StringBuilder();
        
        if (TargetAddress != null)
            result.Append(TargetAddress);
        result.Append("\t");
        
        for (var i = 0; i < HostNames.Count; i++)
        {
            if (i > 0)
                result.Append(' ');
            result.Append(HostNames[i]);
        }

        if (!String.IsNullOrEmpty(CommentText))
        {
            result.Append("\t# ");
            result.Append(CommentText);
        }

        return result.ToString();
    }
}