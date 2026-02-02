namespace LogAnalyzer.Core.Models;

public sealed class AnalysisProgress
{
    public int FilesDiscovered { get; set; }

    public int FilesProcessed { get; set; }

    public int LinesProcessed { get; set; }

    public int ArchiveWindowsFound { get; set; }

    public int MediaEventsFound { get; set; }

    public string CurrentFile { get; set; } = string.Empty;
}
