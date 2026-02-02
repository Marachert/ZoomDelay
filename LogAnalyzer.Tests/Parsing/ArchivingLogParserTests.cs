using System;
using System.IO;
using System.Linq;
using System.Threading;
using LogAnalyzer.Core.Models;
using LogAnalyzer.Infrastructure.Parsing;
using NUnit.Framework;

namespace LogAnalyzer.Tests.Parsing;

[TestFixture]
public sealed class ArchivingLogParserTests
{
    [Test]
    public void ParseFile_ValidLine_ParsesArchivedCountAndDuration()
    {
        string line = "2026-01-12 08:23:38.627 | INFO  | .NET Long Running Task, 34 | , 0 | | AM.Archiving.Handler.Scheduler.ArchivingScheduler | Archived 33, unrecoverable failures 0, scheduled for new attempt 0, scheduled for continuation 0, awaiting feedback 0, skipped interaction 0. Duration: 02:14:34.1998299";
        string filePath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "archiving.log");
        File.WriteAllText(filePath, line + Environment.NewLine);

        LogLineTimestampParser timestampParser = new LogLineTimestampParser();
        ArchivingLogParser parser = new ArchivingLogParser(timestampParser);
        ParseContext context = CreateContext(CancellationToken.None);

        ArchiveWindow[] windows = parser.ParseFile(filePath, CancellationToken.None, context).ToArray();

        Assert.That(windows.Length, Is.EqualTo(1));
        Assert.That(windows[0].ArchivedCount, Is.EqualTo(33));
        Assert.That(windows[0].Duration, Is.EqualTo(TimeSpan.Parse("02:14:34.1998299")));
        Assert.That(windows[0].ArchiveStartDateTime, Is.EqualTo(windows[0].ArchiveEndDateTime - windows[0].Duration));
    }

    private static ParseContext CreateContext(CancellationToken cancellationToken)
    {
        return new ParseContext("archiving.log", _ => { }, () => { }, () => { }, () => { }, cancellationToken);
    }
}
