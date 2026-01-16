using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AlgorithmAcceptanceToolAvalonia.Converters;
using AlgorithmAcceptanceToolAvalonia.Models;
using AlgorithmAcceptanceToolAvalonia.Models.Entities;
using AlgorithmAcceptanceToolAvalonia.Models.Enums;
using AlgorithmAcceptanceToolAvalonia.Utils;
using Avalonia.Controls.Notifications;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Flurl.Util;
using Serilog;
using SukiUI.Dialogs;


namespace AlgorithmAcceptanceToolAvalonia.ViewModels;

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
        RevokeErrorCommand.NotifyCanExecuteChanged();
    }

    [ObservableProperty] 
    private string _imagePath = string.Empty;

    [ObservableProperty] 
    private ObservableCollection<RiskDetectResult> _riskDetectResults = new ObservableCollection<RiskDetectResult>();

    private List<Bitmap> _resultJpgList = [];
    private List<string> _resultPathList = [];
    private List<bool> _resultClassifiedList = [];
    partial void OnImagePathChanged(string value)
    {
        OnPropertyChanged(nameof(ReadyToAnalyze));
        AnalyzeRisksCommand.NotifyCanExecuteChanged();
    }
    

    [ObservableProperty]
    private EnumTaskName _selectedTaskName = TaskNameList[0];

    
    [ObservableProperty] private Bitmap _presentImage;

    [ObservableProperty] private int _jpgIndex = -1;

    [ObservableProperty]
    private bool _cropImage;

    
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
        IsClassified = _resultClassifiedList[newValue];
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

    private async Task<RiskDetectResult> GetDefectLabelByTaskName(string jpg, string resultPath, string taskName, bool cropImage)
    {
        var fileName = Path.GetFileName(jpg);
        await using var stream = File.OpenRead(jpg);
        var guessedLabel = await GuessUtil.TryGetLabel(jpg);
        if (guessedLabel != null)
            taskName = GuessUtil.TryGetTaskName(guessedLabel);
        // Get Api for Response
        var httpResponse = await RequestUtil.GetDefectiveLabel(RiskDetectApi, stream, fileName, taskName);
        var response = httpResponse.Data;
        // Drawing if exists
        var resultJpgPath = Path.Join(resultPath, $"任务={taskName}_真实标签={guessedLabel ?? "无标签文件"}_预测标签={ResponseConverter.GetPredictLabel(response)}_文件名={fileName}");
        await ImageUtil.Drawing(stream, resultJpgPath, response, cropImage, ImagePath);
        // Path Pairs
        _jpgPathPairs[resultJpgPath] = jpg;
        _resultJpgList.Add(ImageUtil.LoadFromLocalPath(resultJpgPath));
        _resultPathList.Add(resultJpgPath);
        _resultClassifiedList.Add(false);
            
        // UI dispatcher
        var tempResult = ResponseConverter.FromResponse(httpResponse, jpg, guessedLabel, taskName);
        return tempResult;
    
        
    }

    private string
        _resultPath = string.Empty,
        _truePositivePath = string.Empty,
        _trueNegativePath = string.Empty,
        _cropPath = string.Empty;

    private void CreateDirectories(string imagePath)
    {
        _cropPath = Path.Join(imagePath, nameof(EnumFolder.Crop).ToLower());
        if (Directory.Exists(_cropPath))
            Directory.Delete(_cropPath, true);
        if (CropImage)
        {
            Directory.CreateDirectory(_cropPath);
        }
        _resultPath = Path.Join(imagePath, nameof(EnumFolder.Result).ToLower());
        _truePositivePath = Path.Join(imagePath, nameof(EnumFolder.异常));
        _trueNegativePath = Path.Join(imagePath, nameof(EnumFolder.误检));
        if (Directory.Exists(_resultPath))
            Directory.Delete(_resultPath, true);
        Directory.CreateDirectory(_resultPath);
        if (Directory.Exists(_trueNegativePath))
            Directory.Delete(_trueNegativePath, true);
        Directory.CreateDirectory(_trueNegativePath);
        if (Directory.Exists(_truePositivePath))
            Directory.Delete(_truePositivePath, true);
        Directory.CreateDirectory(_truePositivePath);
    }
    [RelayCommand(CanExecute = nameof(ReadyToAnalyze), AllowConcurrentExecutions = true, IncludeCancelCommand = true)]
    private async Task AnalyzeRisks(CancellationToken token)
    {
        try
        {
            IsAnalyzing = true;
            RiskDetectResults.Clear();
            _jpgPathPairs.Clear();
            _resultJpgList.Clear();
            _resultPathList.Clear();
            _resultClassifiedList.Clear();
            
            var jpgs = ImageUtil.GetAllJpgPath(ImagePath);
            if (jpgs.Count == 0)
            {
                DialogManager.CreateDialog()
                    .WithTitle("分析中止")
                    .WithContent($"路径{ImagePath}找不到图片")
                    .Dismiss().ByClickingBackground()
                    .OfType(NotificationType.Information)
                    .TryShow();
                return;
            }

            CreateDirectories(ImagePath);
            var paralleOptions = new ParallelOptions()
            {
                MaxDegreeOfParallelism = 4,
                CancellationToken = token
            };

            string taskName = TaskNameConverter.FromEnum(SelectedTaskName);
            await Parallel.ForEachAsync(jpgs, paralleOptions, async (jpg, cancellationToken) =>
            {
                var tempResult = await GetDefectLabelByTaskName(jpg, _resultPath, taskName, CropImage);
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    PresentImage = ImageUtil.LoadFromLocalPath(jpg);
                    RiskDetectResults.Add(tempResult);
                    OnPropertyChanged(nameof(RiskDetectResults));

                });

            });
        }
        catch (OperationCanceledException canceledException)
        {
            Console.WriteLine(canceledException.ToString());
            Log.Information("{Info}", canceledException.ToString());
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

    [ObservableProperty] private bool _isClassified;

    partial void OnIsClassifiedChanged(bool value)
    {
        OnPropertyChanged(nameof(ReadyToMarkError));
        OnPropertyChanged(nameof(ReadyToRevokeError));
        MarkErrorCommand.NotifyCanExecuteChanged();
        RevokeErrorCommand.NotifyCanExecuteChanged();
    }
    

    public bool ReadyToMarkError => !IsAnalyzing && !IsClassified ;
    [RelayCommand(CanExecute = nameof(ReadyToMarkError))]
    private void MarkError(string isTruePositive)
    {
        var jpgPath = _resultPathList[JpgIndex];
        var sourcePath = _jpgPathPairs[jpgPath];
        var fileName = Path.GetFileName(sourcePath);
        string targetPath;
        if (string.Equals(isTruePositive, "true"))
            targetPath = Path.Join(_truePositivePath,fileName);
        else
            targetPath = Path.Join(_trueNegativePath, fileName);    
        File.Copy(sourcePath, targetPath, true);
        _resultClassifiedList[JpgIndex] = true;
        IsClassified = _resultClassifiedList[JpgIndex];
    }
    public bool ReadyToRevokeError => !IsAnalyzing && IsClassified ;

    [RelayCommand(CanExecute = nameof(ReadyToRevokeError))]
    private void RevokeError()
    {
        var jpgPath = _resultPathList[JpgIndex];
        var sourcePath = _jpgPathPairs[jpgPath];
        var fileName = Path.GetFileName(sourcePath);
        string targetPath1, targetPath2;
        targetPath1 = Path.Join(_truePositivePath,fileName);
        targetPath2 = Path.Join(_trueNegativePath, fileName);
        if (File.Exists(targetPath1))
            File.Delete(targetPath1);
        if (File.Exists(targetPath2))
            File.Delete(targetPath2);
        DialogManager.CreateDialog()
            .WithTitle("撤销成功")
            .Dismiss().ByClickingBackground()
            .OfType(NotificationType.Success)
            .TryShow();
        _resultClassifiedList[JpgIndex] = false;
        IsClassified = _resultClassifiedList[JpgIndex];
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