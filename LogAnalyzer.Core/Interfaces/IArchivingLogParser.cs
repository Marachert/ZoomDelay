using System.Collections.Generic;
using System.Threading;
using LogAnalyzer.Core.Models;

namespace LogAnalyzer.Core.Interfaces;

public interface IArchivingLogParser
{
    IEnumerable<ArchiveWindow> ParseFile(string filePath, CancellationToken cancellationToken, ParseContext context);
}
