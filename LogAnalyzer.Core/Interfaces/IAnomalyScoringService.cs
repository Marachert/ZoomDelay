using System.Collections.Generic;

namespace LogAnalyzer.Core.Interfaces;

public interface IAnomalyScoringService
{
    IReadOnlyList<double> NormalizePercentile(IReadOnlyList<double> values, double lowPercentile, double highPercentile);
}
