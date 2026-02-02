using System.Collections.Generic;

namespace LogAnalyzer.Core.Models;

public sealed class LogAnalysisResult
{
    public LogAnalysisResult(IReadOnlyList<ArchiveWindow> archiveWindows, IReadOnlyList<MediaRetrievalEvent> mediaEvents, DiagnosticsStats diagnostics)
    {
        ArchiveWindows = archiveWindows;
        MediaEvents = mediaEvents;
        Diagnostics = diagnostics;
    }

    public IReadOnlyList<ArchiveWindow> ArchiveWindows { get; }

    public IReadOnlyList<MediaRetrievalEvent> MediaEvents { get; }

    public DiagnosticsStats Diagnostics { get; }
}
