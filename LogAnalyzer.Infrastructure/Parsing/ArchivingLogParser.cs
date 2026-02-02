using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using LogAnalyzer.Core.Interfaces;
using LogAnalyzer.Core.Models;

namespace LogAnalyzer.Infrastructure.Parsing;

public sealed class ArchivingLogParser : IArchivingLogParser
{
    private readonly ILogLineTimestampParser _timestampParser;

    public ArchivingLogParser(ILogLineTimestampParser timestampParser)
    {
        _timestampParser = timestampParser;
    }

    public IEnumerable<ArchiveWindow> ParseFile(string filePath, CancellationToken cancellationToken, ParseContext context)
    {
        using FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using StreamReader reader = new StreamReader(fileStream);
        string? line;
        int lineNumber = 0;

        while ((line = reader.ReadLine()) != null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            lineNumber += 1;
            context.OnLinesProcessed(1);

            if (!line.Contains("Archived ", StringComparison.OrdinalIgnoreCase) || !line.Contains("Duration:", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!_timestampParser.TryParseTimestamp(line, out DateTime timestamp))
            {
                context.OnSkippedLine();
                continue;
            }

            if (!TryParseArchivedCount(line, out int archivedCount) || !TryParseDuration(line, out TimeSpan duration))
            {
                context.OnSkippedLine();
                continue;
            }

            DateTime archiveStart = timestamp - duration;
            ArchiveWindow window = new ArchiveWindow(Guid.NewGuid(), archiveStart, timestamp, archivedCount, duration, filePath, lineNumber);
            context.OnArchiveMatched();
            yield return window;
        }
    }

    private static bool TryParseArchivedCount(string line, out int archivedCount)
    {
        archivedCount = 0;
        int index = line.IndexOf("Archived ", StringComparison.OrdinalIgnoreCase);
        if (index < 0)
        {
            return false;
        }

        int start = index + "Archived ".Length;
        int end = start;
        while (end < line.Length && char.IsDigit(line[end]))
        {
            end += 1;
        }

        if (end == start)
        {
            return false;
        }

        string number = line.Substring(start, end - start);
        return int.TryParse(number, NumberStyles.Integer, CultureInfo.InvariantCulture, out archivedCount);
    }

    private static bool TryParseDuration(string line, out TimeSpan duration)
    {
        duration = TimeSpan.Zero;
        int index = line.IndexOf("Duration:", StringComparison.OrdinalIgnoreCase);
        if (index < 0)
        {
            return false;
        }

        string value = line.Substring(index + "Duration:".Length).Trim();
        return TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out duration);
    }
}
