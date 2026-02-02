using System;
using System.Collections.Generic;
using LogAnalyzer.Core.Interfaces;
using LogAnalyzer.Core.Models;

namespace LogAnalyzer.Core.Services;

public sealed class ArchiveWindowCorrelator : IArchiveWindowCorrelator
{
    public CorrelationResult Correlate(IReadOnlyList<ArchiveWindow> archiveWindows, IReadOnlyList<MediaRetrievalEvent> mediaEvents)
    {
        List<ArchiveWindow> sortedWindows = new List<ArchiveWindow>(archiveWindows);
        sortedWindows.Sort((left, right) => left.ArchiveStartDateTime.CompareTo(right.ArchiveStartDateTime));

        Dictionary<Guid, IReadOnlyList<MediaRetrievalEvent>> mediaByArchive = new Dictionary<Guid, IReadOnlyList<MediaRetrievalEvent>>();
        Dictionary<Guid, List<MediaRetrievalEvent>> writableMedia = new Dictionary<Guid, List<MediaRetrievalEvent>>();
        List<MediaRetrievalEvent> unmatched = new List<MediaRetrievalEvent>();

        foreach (ArchiveWindow archiveWindow in sortedWindows)
        {
            List<MediaRetrievalEvent> bucket = new List<MediaRetrievalEvent>();
            writableMedia[archiveWindow.ArchiveKey] = bucket;
            mediaByArchive[archiveWindow.ArchiveKey] = bucket;
        }

        foreach (MediaRetrievalEvent mediaEvent in mediaEvents)
        {
            ArchiveWindow? matchingWindow = FindBestWindow(sortedWindows, mediaEvent.LineTimestamp);
            if (matchingWindow == null)
            {
                unmatched.Add(mediaEvent);
                continue;
            }

            List<MediaRetrievalEvent> list = writableMedia[matchingWindow.ArchiveKey];
            list.Add(mediaEvent);
        }

        return new CorrelationResult(sortedWindows, mediaByArchive, unmatched);
    }

    private static ArchiveWindow? FindBestWindow(IReadOnlyList<ArchiveWindow> windows, DateTime timestamp)
    {
        if (windows.Count == 0)
        {
            return null;
        }

        int low = 0;
        int high = windows.Count - 1;
        int index = -1;

        while (low <= high)
        {
            int mid = low + ((high - low) / 2);
            ArchiveWindow window = windows[mid];
            if (window.ArchiveStartDateTime <= timestamp)
            {
                index = mid;
                low = mid + 1;
            }
            else
            {
                high = mid - 1;
            }
        }

        if (index < 0)
        {
            return null;
        }

        ArchiveWindow? best = null;
        for (int i = index; i >= 0; i -= 1)
        {
            ArchiveWindow window = windows[i];
            if (window.ArchiveEndDateTime < timestamp)
            {
                break;
            }

            if (window.ArchiveEndDateTime >= timestamp)
            {
                if (best == null || window.ArchiveEndDateTime < best.ArchiveEndDateTime)
                {
                    best = window;
                }
            }
        }

        return best;
    }
}
