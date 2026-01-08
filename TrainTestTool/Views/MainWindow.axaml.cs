using Avalonia.Controls;
using Avalonia.Interactivity;
using TrainTestTool.ViewModels;

namespace TrainTestTool.Views;

public partial class MainWindow : Window
{
    private MainWindowViewModel _mainWindowViewModel;
    public MainWindow()
    {
        InitializeComponent();
        _mainWindowViewModel = new MainWindowViewModel();
        DataContext = _mainWindowViewModel;
    }
    
}