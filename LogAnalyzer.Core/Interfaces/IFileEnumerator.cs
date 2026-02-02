using System.Collections.Generic;
using System.Threading;

namespace LogAnalyzer.Core.Interfaces;

public interface IFileEnumerator
{
    IEnumerable<string> EnumerateLogFiles(string rootFolder, CancellationToken cancellationToken);
}
