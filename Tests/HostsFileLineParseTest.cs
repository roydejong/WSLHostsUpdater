using NUnit.Framework;
using WSLHostsUpdater.IO;

namespace WSLHostsUpdater.Tests;

public class HostsFileLineParseTest
{
    [Test]
    public void TestParsesEmptyLines()
    {
        var result = HostsFileLine.Parse("");
        Assert.AreEqual(HostsFileLine.LineType.Empty, result.Type);
        
        var result2 = HostsFileLine.Parse(" ");
        Assert.AreEqual(HostsFileLine.LineType.Empty, result2.Type);
        
        var result3 = HostsFileLine.Parse(" \t ");
        Assert.AreEqual(HostsFileLine.LineType.Empty, result3.Type);
    }
    
    [Test]
    public void TestParsesCommentLines()
    {
        var result = HostsFileLine.Parse("# Copyright (c) 1993-2009 Microsoft Corp.");
        Assert.AreEqual(HostsFileLine.LineType.Comment, result.Type);
        Assert.AreEqual("Copyright (c) 1993-2009 Microsoft Corp.", result.CommentText);
        
        var result2 = HostsFileLine.Parse("#127.0.0.1       localhost");
        Assert.AreEqual(HostsFileLine.LineType.Comment, result2.Type);
        Assert.AreEqual("127.0.0.1       localhost", result2.CommentText);
        Assert.AreEqual(null, result2.TargetAddress);
        Assert.AreEqual(0, result2.HostNames.Count);
    }

    [Test]
    public void TestParsesSimpleEntries()
    {
        var result = HostsFileLine.Parse("1.2.3.4 somehost.com");
        Assert.AreEqual(HostsFileLine.LineType.RegularEntry, result.Type);
        Assert.AreEqual("1.2.3.4", result.TargetAddress);
        Assert.AreEqual(1, result.HostNames.Count);
        Assert.AreEqual("somehost.com", result.HostNames[0]);
    }

    [Test]
    public void TestParsesMultiHostEntries()
    {
        var result = HostsFileLine.Parse("1.2.3.4   somehost.com somehost2.com somehost3.com");
        Assert.AreEqual(HostsFileLine.LineType.RegularEntry, result.Type);
        Assert.AreEqual("1.2.3.4", result.TargetAddress);
        Assert.AreEqual(3, result.HostNames.Count);
        Assert.AreEqual("somehost.com", result.HostNames[0]);
        Assert.AreEqual("somehost2.com", result.HostNames[1]);
        Assert.AreEqual("somehost3.com", result.HostNames[2]);
    }

    [Test]
    public void TestParsesEntriesWithComments()
    {
        var result = HostsFileLine.Parse("1.2.3.4 host1.com host2.net   # some comment host3.org");
        Assert.AreEqual(HostsFileLine.LineType.RegularEntry, result.Type);
        Assert.AreEqual("1.2.3.4", result.TargetAddress);
        Assert.AreEqual(2, result.HostNames.Count);
        Assert.AreEqual("host1.com", result.HostNames[0]);
        Assert.AreEqual("host2.net", result.HostNames[1]);
        Assert.AreEqual("some comment host3.org", result.CommentText);
    }
}