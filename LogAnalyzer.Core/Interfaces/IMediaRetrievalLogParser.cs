using System.Collections.Generic;
using System.Threading;
using LogAnalyzer.Core.Models;

namespace LogAnalyzer.Core.Interfaces;

public interface IMediaRetrievalLogParser
{
    IEnumerable<MediaRetrievalEvent> ParseFile(string filePath, CancellationToken cancellationToken, ParseContext context);
}
