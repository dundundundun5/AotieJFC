using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AlgorithmAcceptanceToolAvalonia.Converters;
using AlgorithmAcceptanceToolAvalonia.Models.Entities;
using AlgorithmAcceptanceToolAvalonia.Models.Enums;
using AlgorithmAcceptanceToolAvalonia.Utils;
using Avalonia.Controls.Notifications;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using SukiUI.Dialogs;


namespace AlgorithmAcceptanceToolAvalonia.ViewModels;

public partial class RiskDetectViewModel : ViewModelBase
{
    // community mvvm
    [ObservableProperty] private bool _isAnalyzing;
    [ObservableProperty] private bool _classifyByStation = false;
    [ObservableProperty] private bool _drawLabel = false;
    [ObservableProperty] private string _imagePath = string.Empty;
    [ObservableProperty] private ObservableCollection<RiskDetectResult> _riskDetectResults = new ObservableCollection<RiskDetectResult>();
    [ObservableProperty] private bool _autoClassifyEnabled = false;
    [ObservableProperty] private EnumTaskName _selectedTaskName = TaskNameList[0];
    [ObservableProperty] private Bitmap? _presentImage;
    [ObservableProperty] private int _jpgIndex = 0;
    [ObservableProperty] private bool _cropImage;
    [ObservableProperty] private string _progress = string.Empty;
    [ObservableProperty] private string _filterText = string.Empty;
    [ObservableProperty] private int _selectedDataGridIndex;
    [ObservableProperty] private string _logText = string.Empty;
    [ObservableProperty] private int _drawLabelScale = 2;
    [ObservableProperty] private float _brightness = 2f; 
    [ObservableProperty] private bool _addBrightness = false;
    [ObservableProperty] private bool _isPlaying = false;
    [ObservableProperty] private string _filterOption = "包含";
    [ObservableProperty] private bool _isClassified;
    partial void OnIsClassifiedChanged(bool value)
    {
        OnPropertyChanged(nameof(ReadyToMarkError));
        OnPropertyChanged(nameof(ReadyToRevokeError));
        MarkErrorCommand.NotifyCanExecuteChanged();
        RevokeErrorCommand.NotifyCanExecuteChanged();
    }

    partial void OnImagePathChanged(string value)
    {
        OnPropertyChanged(nameof(ReadyToAnalyze));
        AnalyzeRisksCommand.NotifyCanExecuteChanged();
    }
     
    partial void OnIsAnalyzingChanged(bool value)
    {
        OnPropertyChanged(nameof(ReadyToAnalyze));
        OnPropertyChanged(nameof(IsNotEnd));
        OnPropertyChanged(nameof(IsNotStart));
        OnPropertyChanged(nameof(ReadyToMarkError));
        OnProgressChanged(nameof(CanPlay));
        
        
        AnalyzeRisksCommand.NotifyCanExecuteChanged();
        NextJpgCommand.NotifyCanExecuteChanged();
        PreviousJpgCommand.NotifyCanExecuteChanged();
        MarkErrorCommand.NotifyCanExecuteChanged();
        RevokeErrorCommand.NotifyCanExecuteChanged();
        InitializePlayTimerCommand.NotifyCanExecuteChanged();
        
    }

    partial void OnIsPlayingChanged(bool value)
    {
        OnProgressChanged(nameof(CanPlay));
        InitializePlayTimerCommand.NotifyCanExecuteChanged();
    }

    partial void OnJpgIndexChanged(int oldValue, int newValue)
    {
        if (newValue < 0 || newValue > RiskDetectResults.Count - 1)
            return;
        
        Dispatcher.UIThread.Invoke(() =>
        {
            PresentImage = ImageUtil.LoadFromLocalPath(RiskDetectResults[newValue].ResultJpgPath);
            DataGridChanged?.Invoke(RiskDetectResults[newValue]);
        });
        IsClassified = _resultClassifiedList[newValue];
        Progress = $" {newValue + 1} / {RiskDetectResults.Count}";
        OnPropertyChanged(nameof(IsNotEnd));
        OnPropertyChanged(nameof(IsNotStart));
        NextJpgCommand.NotifyCanExecuteChanged();
        PreviousJpgCommand.NotifyCanExecuteChanged();
    }
    
    partial void OnAutoClassifyEnabledChanged(bool value)
    {
        if (value)
            Task.Run(AutoClassify);

    }
   
    partial void OnSelectedDataGridIndexChanged(int value)
    {
        JpgIndex = value;
    }
    // public list
    public List<string> FilterOptionList { get; set; }= ["包含", "不包含"];
    public float[] BrightnessList { get; set; }= [ 2f, 3f, 5f];
    public int[] DrawLabelScaleList { get; set; } = [2, 3, 4];
    public static List<EnumTaskName> TaskNameList => Enum.GetValues<EnumTaskName>().ToList();
    // public bool
    public bool ReadyToAnalyze => !string.IsNullOrEmpty(ImagePath) && !IsAnalyzing;
    public bool CanPlay =>  !IsAnalyzing && !IsPlaying;
    public bool ReadyToMarkError => !IsAnalyzing && !IsClassified ;
    public bool ReadyToRevokeError => !IsAnalyzing && IsClassified;
    public bool IsNotStart => JpgIndex != 0 && !IsAnalyzing;
    public bool IsNotEnd => (JpgIndex + 1) != RiskDetectResults.Count && !IsAnalyzing;
    // public object
    public ISukiDialogManager DialogManager { get; } = new SukiDialogManager();
    // private
    private List<bool> _resultClassifiedList = [];
    private Dictionary<string, string> _jpgPathPairs = new();
    private DispatcherTimer? _playTimer = null;
    private DispatcherTimer _timer;
    private string
        _resultPath = string.Empty,
        _truePositivePath = string.Empty,
        _trueNegativePath = string.Empty,
        _noLabelPath = string.Empty,
        _hasLabelPath = string.Empty;
   
    
    
    // public event
    public event Action<RiskDetectResult> DataGridChanged = (result) =>
    {

    };
    // private method - relaycommand
    [RelayCommand]
    private void InitializeDailyTimer()
    {
        
        // 创建定时器
        _timer = new DispatcherTimer();
        _timer.Interval = TimeSpan.FromDays(1);
        _timer.Tick += (sender, args) =>
        {
            if (!AutoClassifyEnabled)
                return;
            AutoClassify();
        };
        _timer.Start();
        
    }
    
    [RelayCommand(CanExecute = nameof(CanPlay))]
    private void InitializePlayTimer()
    {
        if (_playTimer is null)
        {
            _playTimer = new DispatcherTimer();
            _playTimer.Interval = TimeSpan.FromSeconds(1.25);
            _playTimer.Tick += (sender, args) =>
            {
                if (IsNotEnd)
                    JpgIndex += 1;
                else
                    _playTimer.Stop();
                
                
                    
            };
        }
        _playTimer.Start();
        IsPlaying = true;

    }
    [RelayCommand(CanExecute = nameof(ReadyToAnalyze), AllowConcurrentExecutions = true, IncludeCancelCommand = true)]
    private async Task AnalyzeRisks(CancellationToken token)
    {
        int total = 0;
    
        try
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                IsAnalyzing = true;
                RiskDetectResults.Clear();
            });
           
            _jpgPathPairs.Clear();
            RiskDetectResults.Clear();
            _resultClassifiedList.Clear();
            
            
            var jpgs = ImageUtil.GetAllJpgPath(ImagePath, FilterText.Trim(), FilterOption);
            if (jpgs.Count == 0)
            {
                await Dispatcher.UIThread.InvokeAsync( () =>
                {
                    DialogManager
                        .CreateDialog()
                        .WithTitle("提示")
                        .WithContent("符合条件的图片数量为0")
                        .Dismiss()
                        .ByClickingBackground()
                        .OfType(NotificationType.Warning)
                        .TryShow();
                });
                
                return;
            }

            total = jpgs.Count;
            CreateDirectories(ImagePath);
            var paralleOptions = new ParallelOptions()
            {
                MaxDegreeOfParallelism = 8,
                CancellationToken = token
            };
            int b = 1;
            string taskName = TaskNameConverter.FromEnum(SelectedTaskName);
            await Parallel.ForEachAsync(jpgs, paralleOptions, async (jpg, cancellationToken) =>
            {
                try
                {
                    var tempResult = await GetDefectLabelByTaskName(jpg, _resultPath, taskName, CropImage);
                    
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        PresentImage = ImageUtil.LoadFromLocalPath(jpg);
                        RiskDetectResults.Add(tempResult);
                        OnPropertyChanged(nameof(RiskDetectResults));
                        Progress = $"{b} / {total}";
                    });
                    b += 1;
                }
                catch (Exception ex2)
                {
                    
                    Log.Error("{ErrorMessage}",  $"{jpg}-{taskName}-{ex2.ToString()}");
                }
                

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
            Dispatcher.UIThread.Invoke(() =>
            {
                IsAnalyzing = false;
                JpgIndex = 0;
                OnPropertyChanged(nameof(JpgIndex));
            });
            
            if (CropImage)
            {
                if (ClassifyByStation)
                    AutoClean();
            }
        }
            
            
    }
    
    [RelayCommand(CanExecute = nameof(ReadyToMarkError))]
    private void MarkError(string isTruePositive)
    {
        var jpgPath = RiskDetectResults[JpgIndex].ResultJpgPath;
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
    
    [RelayCommand(CanExecute = nameof(ReadyToRevokeError))]
    private void RevokeError()
    {
        var jpgPath = RiskDetectResults[JpgIndex].ResultJpgPath;
        var sourcePath = _jpgPathPairs[jpgPath];
        var fileName = Path.GetFileName(sourcePath);
        string targetPath1, targetPath2;
        targetPath1 = Path.Join(_truePositivePath,fileName);
        targetPath2 = Path.Join(_trueNegativePath, fileName);
        if (File.Exists(targetPath1))
            File.Delete(targetPath1);
        if (File.Exists(targetPath2))
            File.Delete(targetPath2);
        _resultClassifiedList[JpgIndex] = false;
        IsClassified = _resultClassifiedList[JpgIndex];
    }
    
    [RelayCommand(CanExecute = nameof(IsNotEnd))]
    private void NextJpg()
    {
        if (_playTimer is not null)
        {
            _playTimer.Stop();
            IsPlaying = false;
        }
            
        JpgIndex += 1;
        
    }

    [RelayCommand(CanExecute = nameof(IsNotStart))]
    private void PreviousJpg()
    {
        if (_playTimer is not null)
        {
            _playTimer.Stop();
            IsPlaying = false;
        }
            
            
        
        JpgIndex -= 1;
    }
    // private method
    private async Task<RiskDetectResult> GetDefectLabelByTaskName(string jpg, string resultPath, string taskName, bool cropImage)
    {
        var fileName = Path.GetFileName(jpg);
        await using var stream = File.OpenRead(jpg);
        var guessedLabel = "";
        if (taskName == nameof(EnumTaskName.自动))
        {
            guessedLabel = await GuessUtil.GuessLabelFromLabelFile(jpg);
            if (guessedLabel != null)
                taskName = GuessUtil.TryGetTaskName(guessedLabel);
        }
        
        // Get Api for Response
        
        var httpResponse = await RequestUtil.GetDefectiveLabel(RiskDetectApi, stream, fileName, taskName);
        var response = httpResponse.Data;
        // Drawing if exists
        var resultJpgPath = Path.Join(resultPath, $"{fileName}");
        await ImageUtil.Drawing(stream, resultJpgPath, response, cropImage, ImagePath, ClassifyByStation, AddBrightness, Brightness, DrawLabel, DrawLabelScale);
        // Path Pairs
        _jpgPathPairs[resultJpgPath] = jpg;
        _resultClassifiedList.Add(false);
            
        // UI dispatcher
        var tempResult = ResponseConverter.FromResponse(httpResponse, jpg, guessedLabel, taskName, resultJpgPath);
        return tempResult;
    
        
    }
    
    private async void AutoClassify()
    {
        
        try
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                ImagePath = @"Z:\个人文件夹\张灵顿\manual_error";
                CropImage = true;
                ClassifyByStation = true;
                SelectedTaskName = EnumTaskName.自动;
            });
            
            string archivePath = Path.Join(ImagePath, "archive");
            try
            {
                if (!Directory.Exists(archivePath))
                    Directory.CreateDirectory(archivePath);
            }
            catch (Exception e)
            {
                
            }
            
            foreach (var jpg in Directory.GetFiles(ImagePath))
            {
                try
                {
                    var filename = Path.GetFileName(jpg);
                    File.Copy(jpg, Path.Join(archivePath, filename), true);
                }
                catch (Exception ex)
                {
                    
                }
               
            }
            
            string targetPath = @"D:\新标注文件";
            DateTime d = DateTime.Now.AddDays(-1);
            targetPath = Path.Join(targetPath, $"裁剪{d:MMdd}");
            if (Directory.Exists(targetPath))
                return;
           
            
            await AnalyzeRisks(new CancellationToken(false));
            foreach (var dir in Directory.GetDirectories(ImagePath))
            {
                var name = Path.GetFileName(dir);
                if (name.Contains("a"))
                    continue;
                CopyUtil.CopyDirectory(dir, Path.Join(targetPath, name), true);
                Directory.Delete(dir, true);
            }
        }
        catch (Exception e)
        {
            Log.Error("{ErrorMessage}", e.ToString());
        }
        
    }
    
    private void CreateDirectories(string imagePath)
    {
        _resultPath = Path.Join(imagePath, nameof(EnumFolder.Result).ToLower());
        _truePositivePath = Path.Join(imagePath, nameof(EnumFolder.异常));
        _trueNegativePath = Path.Join(imagePath, nameof(EnumFolder.误检));
        _noLabelPath = Path.Join(imagePath, nameof(EnumFolder.NoLabel));
        _hasLabelPath = Path.Join(imagePath, nameof(EnumFolder.HasLabel));
        if (Directory.Exists(_resultPath))
            Directory.Delete(_resultPath, true);
        Directory.CreateDirectory(_resultPath);
        if (Directory.Exists(_trueNegativePath))
            Directory.Delete(_trueNegativePath, true);
        Directory.CreateDirectory(_trueNegativePath);
        if (Directory.Exists(_truePositivePath))
            Directory.Delete(_truePositivePath, true);
        Directory.CreateDirectory(_truePositivePath);
        if (Directory.Exists(_noLabelPath))
            Directory.Delete(_noLabelPath, true);
        Directory.CreateDirectory(_noLabelPath);
        if (Directory.Exists(_hasLabelPath))
            Directory.Delete(_hasLabelPath, true);
        Directory.CreateDirectory(_hasLabelPath);
    }
    

    private void AutoClean()
    {

        if (Directory.Exists(_resultPath))
            Directory.Delete(_resultPath, true);
        if (Directory.Exists(_trueNegativePath))
            Directory.Delete(_trueNegativePath, true);
        if (Directory.Exists(_truePositivePath))
            Directory.Delete(_truePositivePath, true);
        foreach (var jpg in _jpgPathPairs.Values.ToList())
        {
            try
            {
                Console.WriteLine($"Delete {jpg}");
                File.Delete(jpg);
            }
            catch (Exception e)
            {
                Log.Error("{ErrorMessage}", e.ToString());
            }
        }
    }
}