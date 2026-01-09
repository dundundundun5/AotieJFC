using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Configuration;

namespace AlgoritmAcceptanceToolAvalonia.ViewModels;

public class ViewModelBase : ObservableObject
{
    protected string SegmentApi;
    protected string OcrApi;
    protected string RiskDetectApi;
    protected ViewModelBase()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        string baseUrl = configuration.GetSection("BaseUrl").Value;
        string segmentApi = configuration.GetSection("SegmentApi").Value;
        string ocrApi = configuration.GetSection("OcrApi").Value;
        string riskDetectApi = configuration.GetSection("RiskDetectApi").Value;
        SegmentApi = baseUrl + segmentApi;
        OcrApi = baseUrl + ocrApi;
        RiskDetectApi = baseUrl + riskDetectApi;
    }
}