using System;
using System.IO;
using System.Linq;
using System.Threading;
using LogAnalyzer.Core.Models;
using LogAnalyzer.Infrastructure.Parsing;
using NUnit.Framework;

namespace LogAnalyzer.Tests.Parsing;

[TestFixture]
public sealed class MediaRetrievalLogParserTests
{
    [Test]
    public void ParseFile_ValidLine_ParsesInteractionFields()
    {
        string line = "2026-01-12 06:14:19.222 | INFO  | .NET TP Worker, 9 | , ProcessingRequest | | Interaction.MediaRetrieval.Application.MediaProviding.MediaProvider | Processing request (audioPlot: True, Accept: Mp3, Opus, M4A, Mp4) for InteractionEntity { Id = 0b72ec2a-c096-4714-a0fc-c7a7007b2ec1, StartTime = 12/2/2025 10:27:57 AM +00:00, StopTime = 12/2/2025 11:38:04 AM +00:00, Type = Voice }, media: MediaCodec: M4A, FingerprintStatus: Matching, InteractionEntity { Id = 0b72ec2a-c096-4714-a0fc-c7a7007b2ec1, StartTime = 12/2/2025 10:27:57 AM +00:00, StopTime = 12/2/2025 11:38:04 AM +00:00, Type = Voice },";
        string filePath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "media.log");
        File.WriteAllText(filePath, line + Environment.NewLine);

        LogLineTimestampParser timestampParser = new LogLineTimestampParser();
        MediaRetrievalLogParser parser = new MediaRetrievalLogParser(timestampParser);
        ParseContext context = CreateContext(CancellationToken.None);

        MediaRetrievalEvent[] events = parser.ParseFile(filePath, CancellationToken.None, context).ToArray();

        Assert.That(events.Length, Is.EqualTo(1));
        Assert.That(events[0].InteractionId, Is.EqualTo(Guid.Parse("0b72ec2a-c096-4714-a0fc-c7a7007b2ec1")));
        DateTimeOffset expectedStart = DateTimeOffset.ParseExact("12/2/2025 10:27:57 AM +00:00", "M/d/yyyy h:mm:ss tt zzz", System.Globalization.CultureInfo.InvariantCulture);
        DateTimeOffset expectedStop = DateTimeOffset.ParseExact("12/2/2025 11:38:04 AM +00:00", "M/d/yyyy h:mm:ss tt zzz", System.Globalization.CultureInfo.InvariantCulture);
        Assert.That(events[0].InteractionStartTime, Is.EqualTo(expectedStart));
        Assert.That(events[0].InteractionStopTime, Is.EqualTo(expectedStop));
        Assert.That(events[0].InteractionDuration, Is.EqualTo(events[0].InteractionStopTime - events[0].InteractionStartTime));
        Assert.That(events[0].HasTimeAnomaly, Is.False);
    }

    [Test]
    public void ParseFile_StopBeforeStart_ClampsDuration()
    {
        string line = "2026-01-12 06:14:19.222 | INFO  | .NET TP Worker, 9 | , ProcessingRequest | | Interaction.MediaRetrieval.Application.MediaProviding.MediaProvider | Processing request InteractionEntity { Id = 1b72ec2a-c096-4714-a0fc-c7a7007b2ec1, StartTime = 12/2/2025 11:38:04 AM +00:00, StopTime = 12/2/2025 10:27:57 AM +00:00, Type = Voice },";
        string filePath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "media_anomaly.log");
        File.WriteAllText(filePath, line + Environment.NewLine);

        LogLineTimestampParser timestampParser = new LogLineTimestampParser();
        MediaRetrievalLogParser parser = new MediaRetrievalLogParser(timestampParser);
        ParseContext context = CreateContext(CancellationToken.None);

        MediaRetrievalEvent[] events = parser.ParseFile(filePath, CancellationToken.None, context).ToArray();

        Assert.That(events.Length, Is.EqualTo(1));
        Assert.That(events[0].InteractionDuration, Is.EqualTo(TimeSpan.Zero));
        Assert.That(events[0].HasTimeAnomaly, Is.True);
    }

    private static ParseContext CreateContext(CancellationToken cancellationToken)
    {
        return new ParseContext("media.log", _ => { }, () => { }, () => { }, () => { }, cancellationToken);
    }
}
