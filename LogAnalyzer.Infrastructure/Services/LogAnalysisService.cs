using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LogAnalyzer.Core.Interfaces;
using LogAnalyzer.Core.Models;

namespace LogAnalyzer.Infrastructure.Services;

public sealed class LogAnalysisService : ILogAnalysisService
{
    private readonly IFileEnumerator _fileEnumerator;
    private readonly IArchivingLogParser _archivingLogParser;
    private readonly IMediaRetrievalLogParser _mediaRetrievalLogParser;

    public LogAnalysisService(IFileEnumerator fileEnumerator, IArchivingLogParser archivingLogParser, IMediaRetrievalLogParser mediaRetrievalLogParser)
    {
        _fileEnumerator = fileEnumerator;
        _archivingLogParser = archivingLogParser;
        _mediaRetrievalLogParser = mediaRetrievalLogParser;
    }

    public async Task<LogAnalysisResult> AnalyzeAsync(string rootFolder, IProgress<AnalysisProgress> progress, CancellationToken cancellationToken)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        List<string> files = _fileEnumerator.EnumerateLogFiles(rootFolder, cancellationToken).ToList();
        List<string> archivingFiles = files.Where(file => Path.GetFileName(file).Contains("AM.Archiving", StringComparison.OrdinalIgnoreCase)).ToList();
        List<string> mediaFiles = files.Where(file => Path.GetFileName(file).Contains("Interaction.MediaRetrieval", StringComparison.OrdinalIgnoreCase)).ToList();

        ProgressTracker tracker = new ProgressTracker(progress)
        {
            FilesDiscovered = files.Count
        };

        ConcurrentBag<ArchiveWindow> windows = new ConcurrentBag<ArchiveWindow>();
        ConcurrentBag<MediaRetrievalEvent> mediaEvents = new ConcurrentBag<MediaRetrievalEvent>();

        int maxParallelism = Math.Max(1, Environment.ProcessorCount / 2);
        SemaphoreSlim semaphore = new SemaphoreSlim(maxParallelism, maxParallelism);

        List<Task> tasks = new List<Task>();
        foreach (string file in archivingFiles)
        {
            tasks.Add(Task.Run(async () =>
            {
                await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                try
                {
                    ParseArchiveFile(file, windows, tracker, cancellationToken);
                }
                finally
                {
                    semaphore.Release();
                }
            }, cancellationToken));
        }

        foreach (string file in mediaFiles)
        {
            tasks.Add(Task.Run(async () =>
            {
                await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                try
                {
                    ParseMediaFile(file, mediaEvents, tracker, cancellationToken);
                }
                finally
                {
                    semaphore.Release();
                }
            }, cancellationToken));
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);

        List<ArchiveWindow> orderedWindows = windows.OrderBy(window => window.ArchiveStartDateTime).ToList();
        List<MediaRetrievalEvent> orderedMedia = mediaEvents.OrderBy(media => media.LineTimestamp).ToList();

        DiagnosticsStats diagnostics = tracker.BuildDiagnostics(stopwatch.Elapsed, files.Count);
        return new LogAnalysisResult(orderedWindows, orderedMedia, diagnostics);
    }

    private void ParseArchiveFile(string file, ConcurrentBag<ArchiveWindow> windows, ProgressTracker tracker, CancellationToken cancellationToken)
    {
        tracker.SetCurrentFile(file);
        ParseContext context = tracker.CreateContext(file, cancellationToken);
        foreach (ArchiveWindow window in _archivingLogParser.ParseFile(file, cancellationToken, context))
        {
            windows.Add(window);
        }

        tracker.IncrementFilesProcessed();
    }

    private void ParseMediaFile(string file, ConcurrentBag<MediaRetrievalEvent> mediaEvents, ProgressTracker tracker, CancellationToken cancellationToken)
    {
        tracker.SetCurrentFile(file);
        ParseContext context = tracker.CreateContext(file, cancellationToken);
        foreach (MediaRetrievalEvent mediaEvent in _mediaRetrievalLogParser.ParseFile(file, cancellationToken, context))
        {
            mediaEvents.Add(mediaEvent);
        }

        tracker.IncrementFilesProcessed();
    }

    private sealed class ProgressTracker
    {
        private readonly IProgress<AnalysisProgress> _progress;
        private readonly AnalysisProgress _current;
        private int _linesProcessed;
        private int _archiveFound;
        private int _mediaFound;
        private int _skipped;
        private int _filesProcessed;

        public ProgressTracker(IProgress<AnalysisProgress> progress)
        {
            _progress = progress;
            _current = new AnalysisProgress();
        }

        public int FilesDiscovered
        {
            get => _current.FilesDiscovered;
            set => _current.FilesDiscovered = value;
        }

        public ParseContext CreateContext(string currentFile, CancellationToken cancellationToken)
        {
            return new ParseContext(
                currentFile,
                OnLinesProcessed,
                OnArchiveMatched,
                OnMediaMatched,
                OnSkippedLine,
                cancellationToken);
        }

        public void SetCurrentFile(string currentFile)
        {
            _current.CurrentFile = currentFile;
            Report();
        }

        public void IncrementFilesProcessed()
        {
            Interlocked.Increment(ref _filesProcessed);
            _current.FilesProcessed = _filesProcessed;
            Report();
        }

        private void OnLinesProcessed(int count)
        {
            Interlocked.Add(ref _linesProcessed, count);
            _current.LinesProcessed = _linesProcessed;
            Report();
        }

        private void OnArchiveMatched()
        {
            Interlocked.Increment(ref _archiveFound);
            _current.ArchiveWindowsFound = _archiveFound;
            Report();
        }

        private void OnMediaMatched()
        {
            Interlocked.Increment(ref _mediaFound);
            _current.MediaEventsFound = _mediaFound;
            Report();
        }

        private void OnSkippedLine()
        {
            Interlocked.Increment(ref _skipped);
        }

        public DiagnosticsStats BuildDiagnostics(TimeSpan elapsed, int totalFiles)
        {
            DiagnosticsStats diagnostics = new DiagnosticsStats
            {
                TotalFiles = totalFiles,
                TotalLines = _linesProcessed,
                MatchedArchiveLines = _archiveFound,
                MatchedMediaLines = _mediaFound,
                SkippedLines = _skipped,
                UnmatchedMediaEvents = 0,
                Elapsed = elapsed
            };

            return diagnostics;
        }

        private void Report()
        {
            _progress.Report(_current);
        }
    }
}
