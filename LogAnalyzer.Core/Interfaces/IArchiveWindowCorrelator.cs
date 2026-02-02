using System.Collections.Generic;
using LogAnalyzer.Core.Models;

namespace LogAnalyzer.Core.Interfaces;

public interface IArchiveWindowCorrelator
{
    CorrelationResult Correlate(IReadOnlyList<ArchiveWindow> archiveWindows, IReadOnlyList<MediaRetrievalEvent> mediaEvents);
}
