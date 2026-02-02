using System;
using System.Windows;
using LogAnalyzer.App.ViewModels;
using LogAnalyzer.Core.Interfaces;
using LogAnalyzer.Core.Services;
using LogAnalyzer.Core.Sorting;
using LogAnalyzer.Infrastructure.Parsing;
using LogAnalyzer.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LogAnalyzer.App;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    private void OnStartup(object sender, StartupEventArgs e)
    {
        ServiceCollection services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        MainWindow mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IFileEnumerator, FileEnumerator>();
        services.AddSingleton<ILogLineTimestampParser, LogLineTimestampParser>();
        services.AddSingleton<IArchivingLogParser, ArchivingLogParser>();
        services.AddSingleton<IMediaRetrievalLogParser, MediaRetrievalLogParser>();
        services.AddSingleton<ILogAnalysisService, LogAnalysisService>();
        services.AddSingleton<IArchiveWindowCorrelator, ArchiveWindowCorrelator>();
        services.AddSingleton<IAnomalyScoringService, AnomalyScoringService>();
        services.AddSingleton<IResultRowBuilder, ResultRowBuilder>();
        services.AddSingleton<IResultRowSorter, ResultRowSorter>();
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainWindow>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
