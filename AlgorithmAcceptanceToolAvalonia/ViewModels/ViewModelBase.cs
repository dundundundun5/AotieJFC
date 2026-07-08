using System;
using AlgorithmAcceptanceToolAvalonia.Utils;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Configuration;
using SukiUI.Dialogs;

namespace AlgorithmAcceptanceToolAvalonia.ViewModels;

public class ViewModelBase : ObservableObject
{
    protected string SegmentApi;
    protected string OcrApi;
    protected string RiskDetectApi;
    protected int ThreadNumber;
    public ISukiDialogManager DialogManager { get; } = new SukiDialogManager();
    protected ViewModelBase()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
      
        string baseUrl = configuration.GetSection("BaseUrl").Value;
        var (station, api) = StationApiUtil.PresentStationAlgorithmApi();
        if (!string.IsNullOrEmpty(station))
        {
            baseUrl = $"http://192.168.{api}";
            Dispatcher.UIThread.Invoke(() =>
            {
                DialogManager
                    .CreateDialog()
                    .WithTitle("算法Api更换")
                    .WithContent($"检测到生产环境: {station} 站, 算法改为 {baseUrl}")
                    .Dismiss()
                    .ByClickingBackground()
                    .OfType(NotificationType.Information)
                    .TryShow();

            });
        }
            
        
        
        string segmentApi = configuration.GetSection("SegmentApi").Value;
        string ocrApi = configuration.GetSection("OcrApi").Value;
        string riskDetectApi = configuration.GetSection("RiskDetectApi").Value;
        string devUrl = configuration.GetSection("DevUrl").Value;
        ThreadNumber = int.Parse(configuration.GetSection("Thread").Value);
        SegmentApi = baseUrl + segmentApi;
        OcrApi = baseUrl + ocrApi;
        RiskDetectApi = baseUrl + riskDetectApi;
    }
}