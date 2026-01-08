using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AlgoritmAcceptanceToolAvalonia.Converters;
using AlgoritmAcceptanceToolAvalonia.Models;
using AlgoritmAcceptanceToolAvalonia.Models.Enums;
using AlgoritmAcceptanceToolAvalonia.Utils;
using Avalonia.Controls.Notifications;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Flurl.Util;
using Serilog;
using SukiUI.Dialogs;


namespace AlgoritmAcceptanceToolAvalonia.ViewModels;

public partial class RiskDetectViewModel : ViewModelBase
{
    public ISukiDialogManager DialogManager { get; } = new SukiDialogManager();
    public static List<EnumTaskName> TaskNameList => Enum.GetValues<EnumTaskName>().ToList();

    [ObservableProperty]
    private bool _isAnalyzing;

    partial void OnIsAnalyzingChanged(bool value)
    {
        OnPropertyChanged(nameof(ReadyToAnalyze));
        OnPropertyChanged(nameof(IsNotEnd));
        OnPropertyChanged(nameof(IsNotStart));
        OnPropertyChanged(nameof(ReadyToMarkError));
        AnalyzeRisksCommand.NotifyCanExecuteChanged();
        NextJpgCommand.NotifyCanExecuteChanged();
        PreviousJpgCommand.NotifyCanExecuteChanged();
        MarkErrorCommand.NotifyCanExecuteChanged();

    }

    [ObservableProperty] 
    private string _imagePath = string.Empty;

    [ObservableProperty] 
    private ObservableCollection<RiskDetectResult> _riskDetectResults = new ObservableCollection<RiskDetectResult>();

    private List<Bitmap> _resultJpgList = [];
    private List<string> _resultPathList = [];
    partial void OnImagePathChanged(string value)
    {
        OnPropertyChanged(nameof(ReadyToAnalyze));
        AnalyzeRisksCommand.NotifyCanExecuteChanged();
    }
    

    [ObservableProperty]
    private EnumTaskName _selectedTaskName = TaskNameList[0];

    
    [ObservableProperty] private Bitmap _presentImage;

    [ObservableProperty] private int _jpgIndex = -1;

    public event Action<RiskDetectResult> DataGridChanged = (result) =>
    {

    };

    partial void OnJpgIndexChanged(int oldValue, int newValue)
    {
       
        Dispatcher.UIThread.Invoke(() =>
        {
            PresentImage = _resultJpgList[newValue];
            DataGridChanged?.Invoke(RiskDetectResults[newValue]);
        });
        
        OnPropertyChanged(nameof(IsNotEnd));
        OnPropertyChanged(nameof(IsNotStart));
        NextJpgCommand.NotifyCanExecuteChanged();
        PreviousJpgCommand.NotifyCanExecuteChanged();
    }

    [ObservableProperty]
    private int _selectedDataGridIndex;
    

   

    partial void OnSelectedDataGridIndexChanged(int value)
    {
        JpgIndex = value;
    }
    [ObservableProperty]
    private string _logText = string.Empty;

    public bool ReadyToAnalyze => !string.IsNullOrEmpty(ImagePath) && !IsAnalyzing;

    private Dictionary<string, string> _jpgPathPairs = new();

    private async Task<RiskDetectResult> GetDefectLabelByTaskName(string jpg, string resultPath, List<string> taskNames)
    {
        var fileName = Path.GetFileName(jpg);
        await using var stream = File.OpenRead(jpg);
        var label = await GuessUtil.TryGetLabel(jpg);
        if (taskNames.Count == 1)
        {
            var taskName = taskNames[0];
            // Get Api for Response
            var httpResponse = await RequestUtil.GetDefectiveLabel(RiskDetectApi, stream, fileName, taskName);
            var response = httpResponse.Data;
            // Drawing if exists
            var resultJpgPath = Path.Join(resultPath, $"任务={taskName}_真实标签={label ?? "无标签文件"}_预测标签={ResponseConverter.GetPredictLabel(response)}_文件名={fileName}");
            await ImageUtil.Drawing(stream, resultJpgPath, response);
            // Path Pairs
            _jpgPathPairs[resultJpgPath] = jpg;
            _resultJpgList.Add(ImageUtil.LoadFromLocalPath(resultJpgPath));
            _resultPathList.Add(resultJpgPath);
                
            // UI dispatcher
            var tempResult = ResponseConverter.FromResponse(httpResponse, jpg, label);
            return tempResult;
        }
        else 
        {
            for (int i = 0; i < taskNames.Count; i++)
            {
                if (i == 3)
                {
                    // 转成bytes读取成image再转成stream
                    // 裁剪掉右侧一个正方形 1024x1024
                }

                if (i == 4)
                {
                    // 基于裁剪的再加亮度
                    
                }
                var taskName = taskNames[i];
                var httpResponse = await RequestUtil.GetDefectiveLabel(RiskDetectApi, stream, fileName, taskName);
                var response = httpResponse.Data;
                if (response.DefectList.Count > 0)
                {
                    var predictLabel = response.DefectList[0].DefectType;
                    if (string.Equals(predictLabel.ToUpper(), "BT"))
                    {
                        var resultJpgPath = Path.Join(resultPath, $"任务={taskName}_真实标签={label ?? "无标签文件"}_预测标签={ResponseConverter.GetPredictLabel(response)}_文件名={fileName}");
                        await ImageUtil.Drawing(stream, resultJpgPath, response);
                        // Path Pairs
                        _jpgPathPairs[resultJpgPath] = jpg;
                        _resultJpgList.Add(ImageUtil.LoadFromLocalPath(resultJpgPath));
                        _resultPathList.Add(resultJpgPath);
                        var tempResult = ResponseConverter.FromResponse(httpResponse, jpg, label);
                        return tempResult;
                    }
                }
            }

            return ResponseConverter.FromResponse(null, jpg, label);;
        }
        
    }
    [RelayCommand(CanExecute = nameof(ReadyToAnalyze), AllowConcurrentExecutions = true)]
    private async Task AnalyzeRisks()
    {
        try
        {
            IsAnalyzing = true;
            RiskDetectResults.Clear();
            var resultPath = Path.Join(ImagePath, nameof(EnumFolder.Result).ToLower());
            var errorPath = Path.Join(ImagePath, nameof(EnumFolder.Error).ToLower());
            var paralleOptions = new ParallelOptions()
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };
            if (Directory.Exists(resultPath))
                Directory.Delete(resultPath, true);
            Directory.CreateDirectory(resultPath);
            if (Directory.Exists(errorPath))
                Directory.Delete(errorPath, true);
            Directory.CreateDirectory(errorPath);
            
            var jpgs = ImageUtil.GetAllJpgPath(ImagePath);
            List<string> taskNames = TaskNameConverter.FromEnum(SelectedTaskName);
            await Parallel.ForEachAsync(jpgs, paralleOptions, async (jpg, cancellationToken) =>
            {
                
                var tempResult = await GetDefectLabelByTaskName(jpg, resultPath, taskNames);
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    PresentImage = ImageUtil.LoadFromLocalPath(jpg);
                    RiskDetectResults.Add(tempResult);
                    OnPropertyChanged(nameof(RiskDetectResults));
                    
                });
                
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            Log.Error("{ErrorMessage}", ex.ToString());
        }
        finally
        {
            IsAnalyzing = false;
            JpgIndex = 0;
            OnPropertyChanged(nameof(JpgIndex));
        }
            
            
    }
    public bool ReadyToMarkError => !IsAnalyzing;
    [RelayCommand(CanExecute = nameof(ReadyToMarkError))]
    private void MarkError()
    {
        var jpgPath = _resultPathList[JpgIndex];
        var sourcePath = _jpgPathPairs[jpgPath];
        var fileName = Path.GetFileName(sourcePath);
        var targetPath = Path.Join(ImagePath, nameof(EnumFolder.Error).ToLower(),fileName);
        File.Copy(sourcePath, targetPath, true);
        DialogManager.CreateDialog()
            .WithTitle("操作成功")
            .WithContent($"文件路径 -> {targetPath}")
            .Dismiss().ByClickingBackground()
            .OfType(NotificationType.Success)
            .TryShow();
        
    }

    public bool IsNotStart => JpgIndex != 0 && !IsAnalyzing;
    public bool IsNotEnd => (JpgIndex + 1) != _resultJpgList.Count && !IsAnalyzing;
    
    [RelayCommand(CanExecute = nameof(IsNotEnd))]
    private void NextJpg()
    {
        JpgIndex += 1;
    }

    [RelayCommand(CanExecute = nameof(IsNotStart))]
    private void PreviousJpg()
    {
        JpgIndex -= 1;
    }
}