using System.Collections.Generic;
using LogAnalyzer.Core.Models;

namespace LogAnalyzer.Core.Interfaces;

public interface IResultRowBuilder
{
    IReadOnlyList<ResultRowBase> BuildRows(CorrelationResult correlationResult);
}
