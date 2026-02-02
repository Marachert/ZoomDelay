using System;
using System.Collections.Generic;

namespace LogAnalyzer.Core.Models;

public sealed class CorrelationResult
{
    public CorrelationResult(IReadOnlyList<ArchiveWindow> archiveWindows, IReadOnlyDictionary<Guid, IReadOnlyList<MediaRetrievalEvent>> mediaByArchiveKey, IReadOnlyList<MediaRetrievalEvent> unmatchedMedia)
    {
        ArchiveWindows = archiveWindows;
        MediaByArchiveKey = mediaByArchiveKey;
        UnmatchedMedia = unmatchedMedia;
    }

    public IReadOnlyList<ArchiveWindow> ArchiveWindows { get; }

    public IReadOnlyDictionary<Guid, IReadOnlyList<MediaRetrievalEvent>> MediaByArchiveKey { get; }

    public IReadOnlyList<MediaRetrievalEvent> UnmatchedMedia { get; }
}
