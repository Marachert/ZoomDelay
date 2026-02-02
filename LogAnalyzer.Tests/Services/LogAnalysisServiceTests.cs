using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LogAnalyzer.Core.Interfaces;
using LogAnalyzer.Core.Models;
using LogAnalyzer.Infrastructure.Services;
using Moq;
using NUnit.Framework;

namespace LogAnalyzer.Tests.Services;

[TestFixture]
public sealed class LogAnalysisServiceTests
{
    [Test]
    public async Task AnalyzeAsync_NoFiles_ReturnsEmpty()
    {
        Mock<IFileEnumerator> fileEnumerator = new Mock<IFileEnumerator>(MockBehavior.Strict);
        Mock<IArchivingLogParser> archivingParser = new Mock<IArchivingLogParser>(MockBehavior.Strict);
        Mock<IMediaRetrievalLogParser> mediaParser = new Mock<IMediaRetrievalLogParser>(MockBehavior.Strict);

        fileEnumerator.Setup(enumerator => enumerator.EnumerateLogFiles("root", It.IsAny<CancellationToken>()))
            .Returns(new List<string>());

        LogAnalysisService service = new LogAnalysisService(fileEnumerator.Object, archivingParser.Object, mediaParser.Object);
        Progress<AnalysisProgress> progress = new Progress<AnalysisProgress>(_ => { });

        LogAnalysisResult result = await service.AnalyzeAsync("root", progress, CancellationToken.None);

        Assert.That(result.ArchiveWindows.Count, Is.EqualTo(0));
        Assert.That(result.MediaEvents.Count, Is.EqualTo(0));
        fileEnumerator.VerifyAll();
    }
}
