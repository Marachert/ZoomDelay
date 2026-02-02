using System.ComponentModel;

namespace LogAnalyzer.Core.Sorting;

public sealed class SortRequest
{
    public SortRequest(string columnKey, ListSortDirection direction)
    {
        ColumnKey = columnKey;
        Direction = direction;
    }

    public string ColumnKey { get; }

    public ListSortDirection Direction { get; }
}
