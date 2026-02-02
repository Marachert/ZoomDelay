using System;
using System.Collections.Generic;
using LogAnalyzer.Core.Models;
using LogAnalyzer.Core.Services;
using NUnit.Framework;

namespace LogAnalyzer.Tests.Services;

[TestFixture]
public sealed class ArchiveWindowCorrelatorTests
{
    [Test]
    public void Correlate_InclusiveBoundaries_Matches()
    {
        ArchiveWindow window = new ArchiveWindow(Guid.NewGuid(), new DateTime(2026, 1, 12, 8, 0, 0), new DateTime(2026, 1, 12, 9, 0, 0), 5, TimeSpan.FromHours(1), "file", 1);
        MediaRetrievalEvent media = new MediaRetrievalEvent(Guid.NewGuid(), DateTimeOffset.Now, DateTimeOffset.Now, TimeSpan.Zero, new DateTime(2026, 1, 12, 8, 0, 0), "file", 2, false);

        ArchiveWindowCorrelator correlator = new ArchiveWindowCorrelator();
        CorrelationResult result = correlator.Correlate(new List<ArchiveWindow> { window }, new List<MediaRetrievalEvent> { media });

        Assert.That(result.MediaByArchiveKey[window.ArchiveKey].Count, Is.EqualTo(1));
        Assert.That(result.UnmatchedMedia.Count, Is.EqualTo(0));
    }

    [Test]
    public void Correlate_MultipleMatches_PicksClosestEnd()
    {
        ArchiveWindow early = new ArchiveWindow(Guid.NewGuid(), new DateTime(2026, 1, 12, 8, 0, 0), new DateTime(2026, 1, 12, 9, 0, 0), 5, TimeSpan.FromHours(1), "file", 1);
        ArchiveWindow late = new ArchiveWindow(Guid.NewGuid(), new DateTime(2026, 1, 12, 7, 0, 0), new DateTime(2026, 1, 12, 10, 0, 0), 5, TimeSpan.FromHours(3), "file", 2);
        MediaRetrievalEvent media = new MediaRetrievalEvent(Guid.NewGuid(), DateTimeOffset.Now, DateTimeOffset.Now, TimeSpan.Zero, new DateTime(2026, 1, 12, 8, 30, 0), "file", 3, false);

        ArchiveWindowCorrelator correlator = new ArchiveWindowCorrelator();
        CorrelationResult result = correlator.Correlate(new List<ArchiveWindow> { late, early }, new List<MediaRetrievalEvent> { media });

        Assert.That(result.MediaByArchiveKey[early.ArchiveKey].Count, Is.EqualTo(1));
        Assert.That(result.MediaByArchiveKey[late.ArchiveKey].Count, Is.EqualTo(0));
    }
}
