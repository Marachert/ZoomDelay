using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using LogAnalyzer.App.Commands;
using LogAnalyzer.Core.Interfaces;
using LogAnalyzer.Core.Models;
using LogAnalyzer.Core.Sorting;

namespace LogAnalyzer.App.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly ILogAnalysisService _analysisService;
    private readonly IArchiveWindowCorrelator _correlator;
    private readonly IResultRowBuilder _resultRowBuilder;
    private readonly IResultRowSorter _sorter;

    private CancellationTokenSource? _cancellationTokenSource;
    private string _statusText;
    private string _currentFolder;
    private double _progressValue;
    private double _progressMaximum;
    private bool _isBusy;
    private DiagnosticsStats _diagnostics;
    private string? _currentSortColumn;
    private ListSortDirection _currentSortDirection;

    public MainViewModel(ILogAnalysisService analysisService, IArchiveWindowCorrelator correlator, IResultRowBuilder resultRowBuilder, IResultRowSorter sorter)
    {
        _analysisService = analysisService;
        _correlator = correlator;
        _resultRowBuilder = resultRowBuilder;
        _sorter = sorter;
        Results = new ObservableCollection<ResultRowBase>();
        _statusText = "Drop a folder to begin.";
        _currentFolder = string.Empty;
        _diagnostics = new DiagnosticsStats();
        _progressMaximum = 1.0d;
        CancelCommand = new RelayCommand(Cancel, () => IsBusy);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<ResultRowBase> Results { get; }

    public RelayCommand CancelCommand { get; }

    public string StatusText
    {
        get => _statusText;
        private set
        {
            _statusText = value;
            OnPropertyChanged(nameof(StatusText));
        }
    }

    public string CurrentFolder
    {
        get => _currentFolder;
        private set
        {
            _currentFolder = value;
            OnPropertyChanged(nameof(CurrentFolder));
        }
    }

    public double ProgressValue
    {
        get => _progressValue;
        private set
        {
            _progressValue = value;
            OnPropertyChanged(nameof(ProgressValue));
        }
    }

    public double ProgressMaximum
    {
        get => _progressMaximum;
        private set
        {
            _progressMaximum = value;
            OnPropertyChanged(nameof(ProgressMaximum));
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            _isBusy = value;
            OnPropertyChanged(nameof(IsBusy));
            CancelCommand.RaiseCanExecuteChanged();
        }
    }

    public DiagnosticsStats Diagnostics
    {
        get => _diagnostics;
        private set
        {
            _diagnostics = value;
            OnPropertyChanged(nameof(Diagnostics));
        }
    }

    public async Task StartAnalysisAsync(string folderPath)
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        CurrentFolder = folderPath;
        StatusText = "Scanning files...";
        Results.Clear();
        _cancellationTokenSource = new CancellationTokenSource();
        Progress<AnalysisProgress> progress = new Progress<AnalysisProgress>(UpdateProgress);

        try
        {
            LogAnalysisResult analysisResult = await _analysisService.AnalyzeAsync(folderPath, progress, _cancellationTokenSource.Token).ConfigureAwait(true);
            CorrelationResult correlation = _correlator.Correlate(analysisResult.ArchiveWindows, analysisResult.MediaEvents);

            analysisResult.Diagnostics.UnmatchedMediaEvents = correlation.UnmatchedMedia.Count;
            Diagnostics = analysisResult.Diagnostics;

            IReadOnlyList<ResultRowBase> rows = _resultRowBuilder.BuildRows(correlation);
            ApplyRows(rows);

            StatusText = $"Completed: {analysisResult.Diagnostics.TotalFiles} files, {analysisResult.Diagnostics.TotalLines} lines.";
        }
        catch (OperationCanceledException)
        {
            StatusText = "Operation canceled.";
        }
        catch (Exception exception)
        {
            StatusText = $"Error: {exception.Message}";
        }
        finally
        {
            IsBusy = false;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }

    public ListSortDirection RequestSort(string columnKey)
    {
        if (Results.Count == 0)
        {
            return ListSortDirection.Ascending;
        }

        if (_currentSortColumn == columnKey)
        {
            _currentSortDirection = _currentSortDirection == ListSortDirection.Ascending
                ? ListSortDirection.Descending
                : ListSortDirection.Ascending;
        }
        else
        {
            _currentSortColumn = columnKey;
            _currentSortDirection = ListSortDirection.Ascending;
        }

        SortRequest request = new SortRequest(_currentSortColumn, _currentSortDirection);
        IReadOnlyList<ResultRowBase> sorted = _sorter.Sort(Results, request);
        ApplyRows(sorted);
        return _currentSortDirection;
    }

    private void ApplyRows(IReadOnlyList<ResultRowBase> rows)
    {
        Results.Clear();
        foreach (ResultRowBase row in rows)
        {
            Results.Add(row);
        }
    }

    private void UpdateProgress(AnalysisProgress progress)
    {
        ProgressMaximum = Math.Max(1, progress.FilesDiscovered);
        ProgressValue = progress.FilesProcessed;
        StatusText = $"Files: {progress.FilesProcessed}/{progress.FilesDiscovered}, Lines: {progress.LinesProcessed}, Archive: {progress.ArchiveWindowsFound}, Media: {progress.MediaEventsFound}";
    }

    private void Cancel()
    {
        _cancellationTokenSource?.Cancel();
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
