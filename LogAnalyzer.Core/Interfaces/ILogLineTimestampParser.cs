using System;

namespace LogAnalyzer.Core.Interfaces;

public interface ILogLineTimestampParser
{
    bool TryParseTimestamp(string line, out DateTime timestamp);
}
