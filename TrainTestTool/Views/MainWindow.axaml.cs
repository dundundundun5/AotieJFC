using System;
using System.IO;
using System.Threading;
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
        _mainWindowViewModel.OutputUpdated += () =>
        {
            LogScrollViewer.ScrollToEnd();
        };
        InitializeDailyTimer();
    }
    
    
    
    private Timer _timer;
    private void InitializeDailyTimer()
    {
        DateTime now = DateTime.Now;
        DateTime firstRun = new DateTime(now.Year, now.Month, now.Day, 6, 0, 0);
        
        if (now > firstRun)
        {
            firstRun = firstRun.AddDays(1);
            _mainWindowViewModel.GatherWarningNUploadCommand.Execute("true");
        }

        TimeSpan timeToGo = firstRun - now;
        
        // 创建定时器
        _timer = new Timer(RunTask, null, timeToGo, TimeSpan.FromDays(1));
        
        
    }
    
    private void RunTask(object? state)
    {
        // 直接在后台线程调用 Command
        _mainWindowViewModel.GatherWarningNUploadCommand.Execute("true");
    }

  
}