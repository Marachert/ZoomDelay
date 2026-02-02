using System;
using System.Globalization;
using LogAnalyzer.Core.Interfaces;

namespace LogAnalyzer.Infrastructure.Parsing;

public sealed class LogLineTimestampParser : ILogLineTimestampParser
{
    private static readonly string TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff";

    public bool TryParseTimestamp(string line, out DateTime timestamp)
    {
        timestamp = default(DateTime);
        if (string.IsNullOrWhiteSpace(line) || line.Length < TimestampFormat.Length)
        {
            return false;
        }

        string candidate = line.Substring(0, TimestampFormat.Length);
        return DateTime.TryParseExact(candidate, TimestampFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out timestamp);
    }
}
