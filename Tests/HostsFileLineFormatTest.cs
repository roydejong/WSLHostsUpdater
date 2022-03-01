using NUnit.Framework;
using WSLHostsUpdater.IO;

namespace WSLHostsUpdater.Tests;

public class HostsFileLineFormatTest
{
    [Test]
    public void TestFormatsHostEntries()
    {
        var testLine = new HostsFileLine("1.2.3.4", null);
        testLine.HostNames.Add("host1.com");
        testLine.HostNames.Add("host2.net");
        testLine.HostNames.Add("host3.org");
        testLine.CommentText = "some comment";
        
        Assert.AreEqual("1.2.3.4\thost1.com host2.net host3.org\t# some comment", testLine.ToString());
    }
}