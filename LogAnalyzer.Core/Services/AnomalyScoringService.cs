using System;
using System.Collections.Generic;
using LogAnalyzer.Core.Interfaces;

namespace LogAnalyzer.Core.Services;

public sealed class AnomalyScoringService : IAnomalyScoringService
{
    public IReadOnlyList<double> NormalizePercentile(IReadOnlyList<double> values, double lowPercentile, double highPercentile)
    {
        if (values.Count == 0)
        {
            return Array.Empty<double>();
        }

        List<double> sorted = new List<double>(values);
        sorted.Sort();

        double lowValue = GetPercentileValue(sorted, lowPercentile);
        double highValue = GetPercentileValue(sorted, highPercentile);

        if (highValue <= lowValue)
        {
            double singleValue = Math.Max(0.0d, lowValue);
            List<double> flat = new List<double>(values.Count);
            for (int i = 0; i < values.Count; i += 1)
            {
                flat.Add(singleValue > 0.0d ? 1.0d : 0.0d);
            }

            return flat;
        }

        List<double> normalized = new List<double>(values.Count);
        for (int i = 0; i < values.Count; i += 1)
        {
            double value = values[i];
            double score = (value - lowValue) / (highValue - lowValue);
            score = Math.Clamp(score, 0.0d, 1.0d);
            normalized.Add(score);
        }

        return normalized;
    }

    private static double GetPercentileValue(IReadOnlyList<double> sortedValues, double percentile)
    {
        if (sortedValues.Count == 1)
        {
            return sortedValues[0];
        }

        double clamped = Math.Clamp(percentile, 0.0d, 100.0d);
        double position = (clamped / 100.0d) * (sortedValues.Count - 1);
        int lowerIndex = (int)Math.Floor(position);
        int upperIndex = (int)Math.Ceiling(position);

        if (lowerIndex == upperIndex)
        {
            return sortedValues[lowerIndex];
        }

        double lowerValue = sortedValues[lowerIndex];
        double upperValue = sortedValues[upperIndex];
        double fraction = position - lowerIndex;
        return lowerValue + ((upperValue - lowerValue) * fraction);
    }
}
