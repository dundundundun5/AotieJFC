using System.Linq;
using System.Threading.Tasks;
using AlgoritmAcceptanceToolAvalonia.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using SukiUI.Controls;

namespace AlgoritmAcceptanceToolAvalonia.Views;

public partial class RiskDetectView : SukiWindow
{
    private readonly RiskDetectViewModel _viewModel;
    public RiskDetectView()
    {
        _viewModel = new RiskDetectViewModel();
        DataContext = _viewModel;
        _viewModel.DataGridChanged += result =>
        {
            RiskDetectResultsGrid.SelectedItem = result;
            RiskDetectResultsGrid.ScrollIntoView(result, null);

        };
        InitializeComponent();
    }

    private async void OpenFolder(object? sender, RoutedEventArgs e)
    {
        var topLevel = GetTopLevel(this);

        // 启动异步操作以打开对话框。
        var files = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
        {
            AllowMultiple = false
        });
        var res = files.Select(a => a.Path.LocalPath).ToList()[0];
        _viewModel.ImagePath = res;
    }


    private void Onclose(object? sender, WindowClosingEventArgs e)
    {
        _viewModel.AnalyzeRisksCancelCommand?.Execute(null);
    }
}