using CommunityToolkit.Mvvm.ComponentModel;

namespace TrainTestTool.ViewModels;

public class ViewModelBase : ObservableObject
{
    protected const string 
        LongImageFolder = "FTP2",
        ShortImageFolder = "Monitor",
        GatheredImageFolder = "long",
        OnlyLocomotiveFolder = "locomotive",
        OnlyRearFolder = "rear",
        SplitJpgName = "avatar.jpg",
        LongHardware = "硬件错误长图",
        LongSoftware = "软件错误长图",
        ShortHardware = "硬件错误短图",
        ShortSoftware = "软件错误短图",
        WarningFolder = "warning";
    protected const string 
        DetectCsFolder = "车身误检测", 
        DetectZxFolder = "走行误检测", 
        DetectManulFolder = "manual_error", 
        ScoreFolder = "score";
}