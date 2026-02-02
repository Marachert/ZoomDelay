using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using LogAnalyzer.Core.Models;

namespace LogAnalyzer.Core.Sorting;

public sealed class ResultRowSorter : IResultRowSorter
{
    public IReadOnlyList<ResultRowBase> Sort(IReadOnlyList<ResultRowBase> rows, SortRequest request)
    {
        List<ArchiveGroup> groups = BuildGroups(rows);
        bool archiveSort = IsArchiveColumn(request.ColumnKey);
        if (archiveSort)
        {
            groups = SortArchiveGroups(groups, request);
        }
        else
        {
            SortMediaWithinGroups(groups, request);
        }

        return Flatten(groups);
    }

    private static List<ArchiveGroup> BuildGroups(IReadOnlyList<ResultRowBase> rows)
    {
        List<ArchiveGroup> groups = new List<ArchiveGroup>();
        Dictionary<Guid, ArchiveGroup> lookup = new Dictionary<Guid, ArchiveGroup>();

        foreach (ResultRowBase row in rows)
        {
            if (row.RowType == RowType.Archive)
            {
                ArchiveResultRow archiveRow = (ArchiveResultRow)row;
                ArchiveGroup group = new ArchiveGroup(archiveRow);
                groups.Add(group);
                lookup[archiveRow.ArchiveKey] = group;
            }
        }

        foreach (ResultRowBase row in rows)
        {
            if (row.RowType == RowType.Media)
            {
                MediaResultRow mediaRow = (MediaResultRow)row;
                if (mediaRow.ParentArchiveKey.HasValue && lookup.TryGetValue(mediaRow.ParentArchiveKey.Value, out ArchiveGroup? group))
                {
                    group.MediaRows.Add(mediaRow);
                }
            }
        }

        return groups;
    }

    private static List<ArchiveGroup> SortArchiveGroups(List<ArchiveGroup> groups, SortRequest request)
    {
        Func<ArchiveResultRow, object?> keySelector = GetArchiveKeySelector(request.ColumnKey);
        IOrderedEnumerable<ArchiveGroup> ordered = request.Direction == ListSortDirection.Ascending
            ? groups.OrderBy(group => keySelector(group.ArchiveRow))
            : groups.OrderByDescending(group => keySelector(group.ArchiveRow));

        return ordered.ToList();
    }

    private static void SortMediaWithinGroups(List<ArchiveGroup> groups, SortRequest request)
    {
        Func<MediaResultRow, object?> keySelector = GetMediaKeySelector(request.ColumnKey);
        foreach (ArchiveGroup group in groups)
        {
            List<MediaResultRow> sorted = request.Direction == ListSortDirection.Ascending
                ? group.MediaRows.OrderBy(keySelector).ToList()
                : group.MediaRows.OrderByDescending(keySelector).ToList();
            group.MediaRows = sorted;
        }
    }

    private static bool IsArchiveColumn(string columnKey)
    {
        return columnKey == ColumnKeys.ArchiveStartDateTime || columnKey == ColumnKeys.ArchivedCount || columnKey == ColumnKeys.Duration;
    }

    private static Func<ArchiveResultRow, object?> GetArchiveKeySelector(string columnKey)
    {
        return columnKey switch
        {
            ColumnKeys.ArchiveStartDateTime => row => row.ArchiveStartDateTime,
            ColumnKeys.ArchivedCount => row => row.ArchivedCount,
            ColumnKeys.Duration => row => row.Duration,
            _ => row => row.ArchiveStartDateTime
        };
    }

    private static Func<MediaResultRow, object?> GetMediaKeySelector(string columnKey)
    {
        return columnKey switch
        {
            ColumnKeys.InteractionId => row => row.InteractionId,
            ColumnKeys.InteractionStartTime => row => row.InteractionStartTime,
            ColumnKeys.InteractionStopTime => row => row.InteractionStopTime,
            ColumnKeys.InteractionDuration => row => row.InteractionDuration,
            _ => row => row.InteractionStartTime
        };
    }

    private static IReadOnlyList<ResultRowBase> Flatten(List<ArchiveGroup> groups)
    {
        List<ResultRowBase> rows = new List<ResultRowBase>();
        foreach (ArchiveGroup group in groups)
        {
            rows.Add(group.ArchiveRow);
            foreach (MediaResultRow mediaRow in group.MediaRows)
            {
                rows.Add(mediaRow);
            }
        }

        return rows;
    }

    private sealed class ArchiveGroup
    {
        public ArchiveGroup(ArchiveResultRow archiveRow)
        {
            ArchiveRow = archiveRow;
            MediaRows = new List<MediaResultRow>();
        }

        public ArchiveResultRow ArchiveRow { get; }

        public List<MediaResultRow> MediaRows { get; set; }
    }
}
