using System.Threading;
using System.Threading.Tasks;
using LogAnalyzer.Core.Models;

namespace LogAnalyzer.Core.Interfaces;

public interface ILogAnalysisService
{
    Task<LogAnalysisResult> AnalyzeAsync(string rootFolder, IProgress<AnalysisProgress> progress, CancellationToken cancellationToken);
}
