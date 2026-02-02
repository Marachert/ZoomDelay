using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LogAnalyzer.App.ViewModels;
using LogAnalyzer.Core.Sorting;
using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;

namespace LogAnalyzer.App;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
    }

    private async void OnDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            string[] items = (string[])e.Data.GetData(DataFormats.FileDrop);
            string? folder = items.FirstOrDefault(item => System.IO.Directory.Exists(item));
            if (!string.IsNullOrWhiteSpace(folder))
            {
                await _viewModel.StartAnalysisAsync(folder).ConfigureAwait(true);
            }
        }
    }

    private void OnDragEnter(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    private async void OnSelectFolder(object sender, RoutedEventArgs e)
    {
        using FolderBrowserDialog dialog = new FolderBrowserDialog
        {
            Description = "Select a folder containing .log files",
            UseDescriptionForTitle = true
        };

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            await _viewModel.StartAnalysisAsync(dialog.SelectedPath).ConfigureAwait(true);
        }
    }

    private void OnGridSorting(object sender, DataGridSortingEventArgs e)
    {
        if (e.Column.Tag is string columnKey)
        {
            System.ComponentModel.ListSortDirection direction = _viewModel.RequestSort(columnKey);
            e.Column.SortDirection = direction;
            e.Handled = true;
        }
    }
}
