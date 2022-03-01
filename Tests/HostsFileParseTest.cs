using System;
using System.IO;
using NUnit.Framework;
using WSLHostsUpdater.IO;

namespace WSLHostsUpdater.Tests;

public class HostsFileParseTest
{
    private static HostsFile? ParseSampleFile(string sampleFileName)
    {
        if (sampleFileName.Contains(".."))
            return null;
        
        var targetPath = Environment.CurrentDirectory + "/Samples/" + sampleFileName;

        if (!File.Exists(targetPath))
            return null;
        
        return HostsFile.FromContents(File.ReadAllText(targetPath));
    }
    
    [Test]
    public void TestParsesDefaultFile()
    {
        var result = ParseSampleFile("default-hosts-file.txt");
        Assert.NotNull(result);
        Assert.AreEqual(20, result!.Lines.Count);
        
        foreach (var parsedLine in result.Lines)
            Assert.AreEqual(HostsFileLine.LineType.Comment, parsedLine.Type);
    }
    
    [Test]
    public void TestParsesFilledFile()
    {
        var result = ParseSampleFile("filled-hosts-file.txt");
        Assert.NotNull(result);
        Assert.AreEqual(8, result!.Lines.Count);
        
        Assert.AreEqual("127.0.0.1", result.TryResolveHostName("reddit.com"));
        Assert.AreEqual("127.0.0.1", result.TryResolveHostName("www.reddit.com"));
        Assert.AreEqual("172.217.175.78", result.TryResolveHostName("google.com"));
        Assert.AreEqual("127.0.0.1", result.TryResolveHostName("localhosts"));
        Assert.AreEqual(null, result.TryResolveHostName("liquidweb.com"));
    }
}