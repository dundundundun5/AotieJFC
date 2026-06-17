using System.Linq;
using System.Threading.Tasks;
using AlgorithmAcceptanceToolAvalonia.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using SukiUI.Controls;

namespace AlgorithmAcceptanceToolAvalonia.Views;

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
        this.Loaded += (sender, args) =>
        {
            _viewModel.InitializeDailyTimerCommand.Execute(null);
        };
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
    

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        // if (e.Key == Key.Z)
        // {
        //     var fullScreen = new FullScreenImageView(_viewModel.PresentImage);
        //     fullScreen.Show(this);
        // }
    }
}