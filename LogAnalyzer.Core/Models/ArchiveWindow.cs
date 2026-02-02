using System;

namespace LogAnalyzer.Core.Models;

public sealed class ArchiveWindow
{
    public ArchiveWindow(Guid archiveKey, DateTime archiveStartDateTime, DateTime archiveEndDateTime, int archivedCount, TimeSpan duration, string sourceFile, int sourceLineNumber)
    {
        ArchiveKey = archiveKey;
        ArchiveStartDateTime = archiveStartDateTime;
        ArchiveEndDateTime = archiveEndDateTime;
        ArchivedCount = archivedCount;
        Duration = duration;
        SourceFile = sourceFile;
        SourceLineNumber = sourceLineNumber;
    }

    public Guid ArchiveKey { get; }

    public DateTime ArchiveStartDateTime { get; }

    public DateTime ArchiveEndDateTime { get; }

    public int ArchivedCount { get; }

    public TimeSpan Duration { get; }

    public string SourceFile { get; }

    public int SourceLineNumber { get; }
}
