using System.Collections.Generic;
using LogAnalyzer.Core.Services;
using NUnit.Framework;

namespace LogAnalyzer.Tests.Services;

[TestFixture]
public sealed class AnomalyScoringServiceTests
{
    [Test]
    public void NormalizePercentile_ClampsValues()
    {
        AnomalyScoringService service = new AnomalyScoringService();
        IReadOnlyList<double> scores = service.NormalizePercentile(new List<double> { 1.0d, 2.0d, 3.0d, 100.0d }, 10.0d, 90.0d);

        Assert.That(scores.Count, Is.EqualTo(4));
        Assert.That(scores[0], Is.GreaterThanOrEqualTo(0.0d));
        Assert.That(scores[3], Is.LessThanOrEqualTo(1.0d));
    }

    [Test]
    public void NormalizePercentile_SmallSamples_Deterministic()
    {
        AnomalyScoringService service = new AnomalyScoringService();
        IReadOnlyList<double> scores = service.NormalizePercentile(new List<double> { 10.0d }, 10.0d, 90.0d);

        Assert.That(scores.Count, Is.EqualTo(1));
        Assert.That(scores[0], Is.EqualTo(1.0d));
    }
}
