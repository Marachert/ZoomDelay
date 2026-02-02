using System;

namespace LogAnalyzer.Core.Models;

public abstract class ResultRowBase
{
    protected ResultRowBase(RowType rowType, Guid archiveKey, Guid? parentArchiveKey, double anomalyScore, int displayIndentLevel)
    {
        RowType = rowType;
        ArchiveKey = archiveKey;
        ParentArchiveKey = parentArchiveKey;
        AnomalyScore = anomalyScore;
        DisplayIndentLevel = displayIndentLevel;
    }

    public RowType RowType { get; }

    public Guid ArchiveKey { get; }

    public Guid? ParentArchiveKey { get; }

    public double AnomalyScore { get; set; }

    public int DisplayIndentLevel { get; }

    public DateTime? ArchiveStartDateTimeDisplay { get; protected set; }

    public int? ArchivedCountDisplay { get; protected set; }

    public TimeSpan? DurationDisplay { get; protected set; }

    public Guid? InteractionIdDisplay { get; protected set; }

    public DateTimeOffset? InteractionStartTimeDisplay { get; protected set; }

    public DateTimeOffset? InteractionStopTimeDisplay { get; protected set; }

    public TimeSpan? InteractionDurationDisplay { get; protected set; }

    public string TooltipText { get; protected set; } = string.Empty;
}
