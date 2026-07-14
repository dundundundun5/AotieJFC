using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using TrainTestTool.Models;
using TrainTestTool.ViewModels;
using TrainTestTool.Views;

namespace TrainTestTool;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger();
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            var ex = e.ExceptionObject as Exception;
            Log.Error(ex, "AppDomain 未处理异常: {ErrorMessage}", ex?.Message);
            // 注意：e.IsTerminating 表示应用即将崩溃
        };

        // 2. Task 未观察到的异常
        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            Log.Error(e.Exception, "Task 未观察异常: {ErrorMessage}", e.Exception.Message);
            e.SetObserved();  // 标记为已处理，避免应用崩溃
        };

        // 3. UI 线程异常（你已经有）
        Dispatcher.UIThread.UnhandledException += (s, args) =>
        {
            Log.Error(args.Exception, "UI 线程异常: {ErrorMessage}", args.Exception.Message);
            args.Handled = true;
        };
    }
    
    public override void OnFrameworkInitializationCompleted()
    {
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MainWindow
            {
                
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
    
}