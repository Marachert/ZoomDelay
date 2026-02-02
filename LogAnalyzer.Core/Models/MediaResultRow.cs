using System;

namespace LogAnalyzer.Core.Models;

public sealed class MediaResultRow : ResultRowBase
{
    public MediaResultRow(Guid archiveKey, Guid parentArchiveKey, Guid interactionId, DateTimeOffset interactionStartTime, DateTimeOffset interactionStopTime, TimeSpan interactionDuration, bool hasTimeAnomaly)
        : base(RowType.Media, archiveKey, parentArchiveKey, 0.0d, 1)
    {
        InteractionId = interactionId;
        InteractionStartTime = interactionStartTime;
        InteractionStopTime = interactionStopTime;
        InteractionDuration = interactionDuration;
        HasTimeAnomaly = hasTimeAnomaly;
        InteractionIdDisplay = interactionId;
        InteractionStartTimeDisplay = interactionStartTime;
        InteractionStopTimeDisplay = interactionStopTime;
        InteractionDurationDisplay = interactionDuration;
    }

    public Guid InteractionId { get; }

    public DateTimeOffset InteractionStartTime { get; }

    public DateTimeOffset InteractionStopTime { get; }

    public TimeSpan InteractionDuration { get; }

    public bool HasTimeAnomaly { get; }
}
