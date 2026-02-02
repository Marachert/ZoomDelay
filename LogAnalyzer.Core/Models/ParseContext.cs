using System;
using System.Threading;

namespace LogAnalyzer.Core.Models;

public sealed class ParseContext
{
    public ParseContext(string currentFile, Action<int> onLinesProcessed, Action onArchiveMatched, Action onMediaMatched, Action onSkippedLine, CancellationToken cancellationToken)
    {
        CurrentFile = currentFile;
        OnLinesProcessed = onLinesProcessed;
        OnArchiveMatched = onArchiveMatched;
        OnMediaMatched = onMediaMatched;
        OnSkippedLine = onSkippedLine;
        CancellationToken = cancellationToken;
    }

    public string CurrentFile { get; }

    public Action<int> OnLinesProcessed { get; }

    public Action OnArchiveMatched { get; }

    public Action OnMediaMatched { get; }

    public Action OnSkippedLine { get; }

    public CancellationToken CancellationToken { get; }
}
