using System;
using System.Collections.Generic;
using LogAnalyzer.Core.Interfaces;
using LogAnalyzer.Core.Models;

namespace LogAnalyzer.Core.Services;

public sealed class ResultRowBuilder : IResultRowBuilder
{
    private readonly IAnomalyScoringService _anomalyScoringService;

    public ResultRowBuilder(IAnomalyScoringService anomalyScoringService)
    {
        _anomalyScoringService = anomalyScoringService;
    }

    public IReadOnlyList<ResultRowBase> BuildRows(CorrelationResult correlationResult)
    {
        List<ResultRowBase> rows = new List<ResultRowBase>();
        List<double> archiveDurationPerItem = new List<double>();
        List<double> mediaDurations = new List<double>();
        List<double> heavyWindowTotals = new List<double>();

        Dictionary<Guid, List<MediaRetrievalEvent>> mediaLookup = new Dictionary<Guid, List<MediaRetrievalEvent>>();
        foreach (KeyValuePair<Guid, IReadOnlyList<MediaRetrievalEvent>> pair in correlationResult.MediaByArchiveKey)
        {
            mediaLookup[pair.Key] = new List<MediaRetrievalEvent>(pair.Value);
        }

        foreach (ArchiveWindow archiveWindow in correlationResult.ArchiveWindows)
        {
            double perArchived = archiveWindow.Duration.TotalSeconds / Math.Max(archiveWindow.ArchivedCount, 1);
            archiveDurationPerItem.Add(perArchived);

            List<MediaRetrievalEvent> media = mediaLookup[archiveWindow.ArchiveKey];
            TimeSpan totalMediaDuration = TimeSpan.Zero;
            foreach (MediaRetrievalEvent mediaEvent in media)
            {
                totalMediaDuration = totalMediaDuration.Add(mediaEvent.InteractionDuration);
                mediaDurations.Add(mediaEvent.InteractionDuration.TotalSeconds);
            }

            heavyWindowTotals.Add(totalMediaDuration.TotalSeconds);
        }

        IReadOnlyList<double> archiveScores = _anomalyScoringService.NormalizePercentile(archiveDurationPerItem, 10.0d, 90.0d);
        IReadOnlyList<double> mediaScores = _anomalyScoringService.NormalizePercentile(mediaDurations, 10.0d, 90.0d);
        IReadOnlyList<double> heavyScores = _anomalyScoringService.NormalizePercentile(heavyWindowTotals, 10.0d, 90.0d);

        int mediaScoreIndex = 0;
        for (int i = 0; i < correlationResult.ArchiveWindows.Count; i += 1)
        {
            ArchiveWindow archiveWindow = correlationResult.ArchiveWindows[i];
            ArchiveResultRow archiveRow = new ArchiveResultRow(archiveWindow.ArchiveKey, archiveWindow.ArchiveStartDateTime, archiveWindow.ArchivedCount, archiveWindow.Duration);
            archiveRow.DurationPerArchivedSeconds = archiveDurationPerItem[i];
            archiveRow.AnomalyScore = archiveScores[i];
            archiveRow.HeavyWindowScore = heavyScores.Count > i ? heavyScores[i] : 0.0d;

            List<MediaRetrievalEvent> media = mediaLookup[archiveWindow.ArchiveKey];
            archiveRow.MediaInteractionCount = media.Count;
            TimeSpan totalDuration = TimeSpan.Zero;
            foreach (MediaRetrievalEvent mediaEvent in media)
            {
                totalDuration = totalDuration.Add(mediaEvent.InteractionDuration);
            }

            archiveRow.TotalMediaInteractionDuration = totalDuration;
            archiveRow.TooltipText = $"Duration per archived: {archiveRow.DurationPerArchivedSeconds:F2}s, Media count: {archiveRow.MediaInteractionCount}, Total media duration: {archiveRow.TotalMediaInteractionDuration}";
            rows.Add(archiveRow);

            foreach (MediaRetrievalEvent mediaEvent in media)
            {
                MediaResultRow mediaRow = new MediaResultRow(Guid.NewGuid(), archiveWindow.ArchiveKey, mediaEvent.InteractionId, mediaEvent.InteractionStartTime, mediaEvent.InteractionStopTime, mediaEvent.InteractionDuration, mediaEvent.HasTimeAnomaly);
                mediaRow.AnomalyScore = mediaScores.Count > mediaScoreIndex ? mediaScores[mediaScoreIndex] : 0.0d;
                mediaScoreIndex += 1;
                mediaRow.TooltipText = $"Interaction duration: {mediaEvent.InteractionDuration}, Start: {mediaEvent.InteractionStartTime}, Stop: {mediaEvent.InteractionStopTime}";
                rows.Add(mediaRow);
            }
        }

        return rows;
    }
}
