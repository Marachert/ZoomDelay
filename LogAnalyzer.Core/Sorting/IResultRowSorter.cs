using System.Collections.Generic;
using LogAnalyzer.Core.Models;

namespace LogAnalyzer.Core.Sorting;

public interface IResultRowSorter
{
    IReadOnlyList<ResultRowBase> Sort(IReadOnlyList<ResultRowBase> rows, SortRequest request);
}
