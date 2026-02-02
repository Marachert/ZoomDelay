using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using LogAnalyzer.Core.Interfaces;
using LogAnalyzer.Core.Models;

namespace LogAnalyzer.Infrastructure.Parsing;

public sealed class MediaRetrievalLogParser : IMediaRetrievalLogParser
{
    private static readonly string[] DateFormats = new string[]
    {
        "M/d/yyyy h:mm:ss tt zzz",
        "M/d/yyyy hh:mm:ss tt zzz",
        "MM/dd/yyyy h:mm:ss tt zzz",
        "MM/dd/yyyy hh:mm:ss tt zzz"
    };

    private readonly ILogLineTimestampParser _timestampParser;

    public MediaRetrievalLogParser(ILogLineTimestampParser timestampParser)
    {
        _timestampParser = timestampParser;
    }

    public IEnumerable<MediaRetrievalEvent> ParseFile(string filePath, CancellationToken cancellationToken, ParseContext context)
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

            if (!line.Contains("Processing request", StringComparison.Ordinal))
            {
                continue;
            }

            if (!_timestampParser.TryParseTimestamp(line, out DateTime lineTimestamp))
            {
                context.OnSkippedLine();
                continue;
            }

            if (!TryExtractGuid(line, "Id =", out Guid interactionId))
            {
                context.OnSkippedLine();
                continue;
            }

            if (!TryExtractDateTimeOffset(line, "StartTime =", out DateTimeOffset startTime))
            {
                context.OnSkippedLine();
                continue;
            }

            if (!TryExtractDateTimeOffset(line, "StopTime =", out DateTimeOffset stopTime))
            {
                context.OnSkippedLine();
                continue;
            }

            bool anomaly = false;
            TimeSpan duration = stopTime - startTime;
            if (duration < TimeSpan.Zero)
            {
                duration = TimeSpan.Zero;
                anomaly = true;
            }

            MediaRetrievalEvent mediaEvent = new MediaRetrievalEvent(interactionId, startTime, stopTime, duration, lineTimestamp, filePath, lineNumber, anomaly);
            context.OnMediaMatched();
            yield return mediaEvent;
        }
    }

    private static bool TryExtractGuid(string line, string token, out Guid value)
    {
        value = Guid.Empty;
        if (!TryExtractValue(line, token, out string extracted))
        {
            return false;
        }

        return Guid.TryParse(extracted, out value);
    }

    private static bool TryExtractDateTimeOffset(string line, string token, out DateTimeOffset value)
    {
        value = default(DateTimeOffset);
        if (!TryExtractValue(line, token, out string extracted))
        {
            return false;
        }

        return DateTimeOffset.TryParseExact(extracted, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out value);
    }

    private static bool TryExtractValue(string line, string token, out string value)
    {
        value = string.Empty;
        int index = line.IndexOf(token, StringComparison.Ordinal);
        if (index < 0)
        {
            return false;
        }

        int start = index + token.Length;
        int end = line.IndexOf(',', start);
        if (end < 0)
        {
            return false;
        }

        value = line.Substring(start, end - start).Trim();
        return !string.IsNullOrWhiteSpace(value);
    }
}
