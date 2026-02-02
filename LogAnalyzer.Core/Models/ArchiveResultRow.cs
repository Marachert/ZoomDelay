using System;

namespace LogAnalyzer.Core.Models;

public sealed class ArchiveResultRow : ResultRowBase
{
    public ArchiveResultRow(Guid archiveKey, DateTime archiveStartDateTime, int archivedCount, TimeSpan duration)
        : base(RowType.Archive, archiveKey, null, 0.0d, 0)
    {
        ArchiveStartDateTime = archiveStartDateTime;
        ArchivedCount = archivedCount;
        Duration = duration;
        ArchiveStartDateTimeDisplay = archiveStartDateTime;
        ArchivedCountDisplay = archivedCount;
        DurationDisplay = duration;
    }

    public DateTime ArchiveStartDateTime { get; }

    public int ArchivedCount { get; }

    public TimeSpan Duration { get; }

    public double DurationPerArchivedSeconds { get; set; }

    public int MediaInteractionCount { get; set; }

    public TimeSpan TotalMediaInteractionDuration { get; set; }

    public double HeavyWindowScore { get; set; }
}
