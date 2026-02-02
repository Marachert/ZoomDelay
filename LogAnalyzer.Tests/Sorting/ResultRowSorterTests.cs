using System;
using System.Collections.Generic;
using System.ComponentModel;
using LogAnalyzer.Core.Models;
using LogAnalyzer.Core.Sorting;
using NUnit.Framework;

namespace LogAnalyzer.Tests.Sorting;

[TestFixture]
public sealed class ResultRowSorterTests
{
    [Test]
    public void Sort_ByArchiveColumn_KeepsChildrenAttached()
    {
        Guid archiveKey1 = Guid.NewGuid();
        Guid archiveKey2 = Guid.NewGuid();

        ArchiveResultRow archive1 = new ArchiveResultRow(archiveKey1, new DateTime(2026, 1, 12, 9, 0, 0), 5, TimeSpan.FromMinutes(10));
        ArchiveResultRow archive2 = new ArchiveResultRow(archiveKey2, new DateTime(2026, 1, 12, 8, 0, 0), 4, TimeSpan.FromMinutes(8));

        MediaResultRow media1 = new MediaResultRow(Guid.NewGuid(), archiveKey1, Guid.NewGuid(), DateTimeOffset.Now, DateTimeOffset.Now, TimeSpan.FromMinutes(1), false);
        MediaResultRow media2 = new MediaResultRow(Guid.NewGuid(), archiveKey2, Guid.NewGuid(), DateTimeOffset.Now, DateTimeOffset.Now, TimeSpan.FromMinutes(2), false);

        List<ResultRowBase> rows = new List<ResultRowBase> { archive1, media1, archive2, media2 };
        ResultRowSorter sorter = new ResultRowSorter();

        IReadOnlyList<ResultRowBase> sorted = sorter.Sort(rows, new SortRequest(ColumnKeys.ArchiveStartDateTime, ListSortDirection.Ascending));

        Assert.That(sorted[0], Is.EqualTo(archive2));
        Assert.That(sorted[1], Is.EqualTo(media2));
        Assert.That(sorted[2], Is.EqualTo(archive1));
        Assert.That(sorted[3], Is.EqualTo(media1));
    }

    [Test]
    public void Sort_ByMediaColumn_SortsWithinParentOnly()
    {
        Guid archiveKey = Guid.NewGuid();
        ArchiveResultRow archive = new ArchiveResultRow(archiveKey, new DateTime(2026, 1, 12, 8, 0, 0), 4, TimeSpan.FromMinutes(8));
        MediaResultRow media1 = new MediaResultRow(Guid.NewGuid(), archiveKey, Guid.NewGuid(), DateTimeOffset.Now, DateTimeOffset.Now, TimeSpan.FromMinutes(5), false);
        MediaResultRow media2 = new MediaResultRow(Guid.NewGuid(), archiveKey, Guid.NewGuid(), DateTimeOffset.Now, DateTimeOffset.Now, TimeSpan.FromMinutes(1), false);

        List<ResultRowBase> rows = new List<ResultRowBase> { archive, media1, media2 };
        ResultRowSorter sorter = new ResultRowSorter();

        IReadOnlyList<ResultRowBase> sorted = sorter.Sort(rows, new SortRequest(ColumnKeys.InteractionDuration, ListSortDirection.Ascending));

        Assert.That(sorted[0], Is.EqualTo(archive));
        Assert.That(sorted[1], Is.EqualTo(media2));
        Assert.That(sorted[2], Is.EqualTo(media1));
    }

    [Test]
    public void Sort_ToggleDirection_Works()
    {
        Guid archiveKey = Guid.NewGuid();
        ArchiveResultRow archive = new ArchiveResultRow(archiveKey, new DateTime(2026, 1, 12, 8, 0, 0), 4, TimeSpan.FromMinutes(8));
        MediaResultRow media1 = new MediaResultRow(Guid.NewGuid(), archiveKey, Guid.NewGuid(), DateTimeOffset.Now, DateTimeOffset.Now, TimeSpan.FromMinutes(1), false);
        MediaResultRow media2 = new MediaResultRow(Guid.NewGuid(), archiveKey, Guid.NewGuid(), DateTimeOffset.Now, DateTimeOffset.Now, TimeSpan.FromMinutes(2), false);

        List<ResultRowBase> rows = new List<ResultRowBase> { archive, media1, media2 };
        ResultRowSorter sorter = new ResultRowSorter();

        IReadOnlyList<ResultRowBase> sortedDescending = sorter.Sort(rows, new SortRequest(ColumnKeys.InteractionDuration, ListSortDirection.Descending));

        Assert.That(sortedDescending[0], Is.EqualTo(archive));
        Assert.That(sortedDescending[1], Is.EqualTo(media2));
        Assert.That(sortedDescending[2], Is.EqualTo(media1));
    }
}
