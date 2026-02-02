using System;

namespace LogAnalyzer.Core.Models;

public sealed class DiagnosticsStats
{
    public int TotalFiles { get; set; }

    public int TotalLines { get; set; }

    public int MatchedArchiveLines { get; set; }

    public int MatchedMediaLines { get; set; }

    public int SkippedLines { get; set; }

    public int UnmatchedMediaEvents { get; set; }

    public TimeSpan Elapsed { get; set; }
}
