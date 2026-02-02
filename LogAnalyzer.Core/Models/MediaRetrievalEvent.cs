using System;

namespace LogAnalyzer.Core.Models;

public sealed class MediaRetrievalEvent
{
    public MediaRetrievalEvent(Guid interactionId, DateTimeOffset interactionStartTime, DateTimeOffset interactionStopTime, TimeSpan interactionDuration, DateTime lineTimestamp, string sourceFile, int sourceLineNumber, bool hasTimeAnomaly)
    {
        InteractionId = interactionId;
        InteractionStartTime = interactionStartTime;
        InteractionStopTime = interactionStopTime;
        InteractionDuration = interactionDuration;
        LineTimestamp = lineTimestamp;
        SourceFile = sourceFile;
        SourceLineNumber = sourceLineNumber;
        HasTimeAnomaly = hasTimeAnomaly;
    }

    public Guid InteractionId { get; }

    public DateTimeOffset InteractionStartTime { get; }

    public DateTimeOffset InteractionStopTime { get; }

    public TimeSpan InteractionDuration { get; }

    public DateTime LineTimestamp { get; }

    public string SourceFile { get; }

    public int SourceLineNumber { get; }

    public bool HasTimeAnomaly { get; }
}
