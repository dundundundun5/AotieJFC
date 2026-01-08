using System;
using AlgoritmAcceptanceToolAvalonia.ViewModels;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using SukiUI.Controls;

namespace AlgoritmAcceptanceToolAvalonia.Views;

public partial class MainWindow : SukiWindow
{
    public MainWindow()
    {
        InitializeComponent();
    }
    

    private void OpenSubWindow(object? sender, RoutedEventArgs e)
    {
        var btn = (Button)sender;
        if ((string)btn.Tag == "1")
        {
            var r = new RiskDetectView();
            r.Show(this);
        }
    }
}