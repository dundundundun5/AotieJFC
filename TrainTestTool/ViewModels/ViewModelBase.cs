using System;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Configuration;

namespace TrainTestTool.ViewModels;

public partial class ViewModelBase : ObservableObject
{
    protected const string
        LongImageFolder = "Ftp2",
        ShortImageFolder = "Monitor",
        GatheredImageFolder = "long",
        OnlyLocomotiveFolder = "locomotive",
        OnlyRearFolder = "rear",
        SplitImageName = "train.png",
        LongHardware = "硬件错误长图",
        LongSoftware = "软件错误长图",
        ShortHardware = "硬件错误短图",
        ShortSoftware = "软件错误短图",
        WarningFolder = "warning",
        SegmentTestFtpFolder = "segment_test",
        DetectManulFolder = "manual_error",
        Ip = "210.22.86.114",
        Username = "suanfa2",
        Password = "Sfkf2024@";

    protected const int Port = 22222;

    /**
     * 根目录盘符号 AbsolutePath
     * 切割远程路径 FtpSegmentPath
     * 异常远程路径 FtpDefectionPath
     * 根远程路径 FtpRemotePath
     */
    protected readonly string 
        AbsolutePath, 
        FtpSegmentPath,
        FtpDefectionPath,
        FtpRemotePath;
    protected readonly string[] DefectionList;
    [ObservableProperty]
    private int[] _gapList;
    [ObservableProperty]
    private string[] _trainTypeList;
    [ObservableProperty]
    private string[] _stationList;
    [ObservableProperty]
    private DateTime[] _dateList;
    [ObservableProperty]
    private string[] _ipList;
    protected ViewModelBase()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        var winRoot = configuration.GetSection("WinRootPath").Value;
        var linuxRootPath = configuration.GetSection("LinuxRootPath").Value;
        if (OperatingSystem.IsWindows())
            AbsolutePath = winRoot;
        else
            AbsolutePath = linuxRootPath;
        FtpRemotePath = configuration.GetSection("FtpPath").Value;
        FtpSegmentPath = Path.Join(FtpRemotePath, "segment_test");
        FtpDefectionPath = Path.Join(FtpRemotePath, DetectManulFolder);
        TrainTypeList =
        [
            "cs-z",
            "zx-z"
        ];
        GapList =
        [
            1,
            8,
            50,
            -30
        ];
        DateList = [DateTime.Now.AddDays(-2).Date, DateTime.Now.AddDays(-1).Date, DateTime.Now.Date];
        DefectionList = configuration.GetSection("DefectionList").GetChildren().Select(a => a.Value).ToArray();
        StationList = configuration.GetSection("StationList").GetChildren().Select(a => a.Value).ToArray();
        IpList = configuration.GetSection("IpList").GetChildren().Select(a => a.Value).ToArray();

    }
    
}