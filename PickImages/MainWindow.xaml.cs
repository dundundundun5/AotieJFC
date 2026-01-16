using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Path = System.IO.Path;
namespace PickImagesTool;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly Console2Textbox c2t;
    private const string FtpRemotePath = "/个人文件夹/张灵顿";
    private const string SourcePath = @"D:\FTP2";
    private const string WarningFolder = "warning";
    private string?  _presentStation;
    private const string DetectCsFolder = "车身误检测", DetectZxFolder = "走行误检测", DetectManulFolder = "manual_error", ScoreFolder = "score";
    private DispatcherTimer? _inactivityTimer;
    private DispatcherTimer? _dailyTimer;
    private DateTime? _lastExecutionDate;
    private static DateTime _today = DateTime.Now;
    private static bool initializing = true;
    private static DateTime _yesterday = DateTime.Now.AddDays(-1);
    private static DateTime __yyesterday = DateTime.Now.AddDays(-2);
    private static DateTime __yyyesterday = DateTime.Now.AddDays(-3);
    private static DateTime __yyyyesterday = DateTime.Now.AddDays(-4);
    private DateTime _presentDate = _yesterday;
    public DateTime[] DateList { get; set; } = [_today, _yesterday, __yyesterday, __yyyesterday, __yyyyesterday];
    private int cnt = 0;
    private readonly string[] _warningLabels =
    [
        "JGQ",
        "ZL",
        "ZJ",
        "RG",
        "HX",
        "PB",
        "P",
        "SL",
        "YW",
        "M",
        "bt",
        "Z"
    ];
    private readonly string[] _stations = [
        "伍明",
        "凤台",
        "包庄",
        "大许",
        "宁波",
        "建国",
        "新塘边",
        "杨集",
        "杭州",
        "枫泾",
        "泗安",
        "淮北北",
        "湾沚",
        "炮车",
        "虞城",
        "西寺坡",
        "誓节渡",
        "李庄",
        "姚李庙",
        "杨楼",
        "梓树庄",
        "烔炀河",
        "东孝",
        "白龙桥",
        "汤溪",
        "后溪街",
        "牌头"
    ];
    private readonly string[] _stationsPingYing = [
        "wm",
        "ft",
        "bz",
        "dx",
        "nb",
        "jg",
        "xtb",
        "yj",
        "hz",
        "fj",
        "sa",
        "hbb",
        "wz",
        "pc",
        "yc",
        "xsp",
        "sjd",
        "lz",
        "ylm",
        "yl",
        "zsz",
        "tyh",
        "dx",
        "blq",
        "tx",
        "hxj",
        "pt"
    ];
    public MainWindow()
    {
        InitializeComponent();
        c2t = new Console2Textbox(myConsole);
        Console.WriteLine($"误检测检查日期：{_presentDate:yyyy-MM-dd}");
        CheckIfExists();
        InitializeDailyTimer();
    }

    private void InitializeDailyTimer()
    {
        _dailyTimer = new DispatcherTimer();
        // 从程序启动时间开始计算1天采集一次昨天的告警
        _dailyTimer.Interval = TimeSpan.FromDays(1);
        _dailyTimer.Tick += ((sender, args) =>
        {
            if (cnt == 20)
                return;
            _presentDate = DateTime.Now.AddDays(-1);
            // 收集一次告警
            GatherWarningClick(null, null);
            cnt++;
        });
        _dailyTimer.Start();
    }
    private void CheckIfExists() {
        string desktopPath;
        string pingYing = "";
        string yesterday = DateTime.Today.AddDays(-1).ToString("yyyyMMdd");
        // yesterday = $"{yesterday:yyyy-MM-dd}";
        for (int i = 0; i < _stations.Length; i++) {
            string path = @$"D:\{_stations[i]}";
            if (!Directory.Exists(path)) continue;
            pingYing = _stationsPingYing[i];
            _presentStation = path.Split("\\")[^1];
            break;
        }
        try {
            desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }
        catch (Exception ex) {
            string rawUser = "57292";
            if (Directory.Exists(@"D:\建国"))
                rawUser = "TEMP";
            desktopPath = $@"C:\Users\{rawUser}";
            Console.WriteLine($"用户名更改过，无法获取桌面路径, 修改为{desktopPath}");
        }
        // foreach (string desktopFile in Directory.GetFiles(desktopPath, searchPattern: "*.csv")) {
        //     if (string.Compare(desktopFile.Split("\\")[^1], $"{pingYing}_{yesterday}_OCR.csv", StringComparison.Ordinal) == 0) {
        //         OCRFile1.Text = desktopFile;
        //         OCRButton1.Content = "寻找原图";
        //         break;
        //     }
        //    
        //
        // }
       

    }
    private void AsyncWrite(TextBox box, string text) {
        void Write(System.Windows.Controls.TextBox textBox, string t) {
            textBox.AppendText(t);
            textBox.ScrollToEnd();
        }
        Action<TextBox, string> updateAction = new Action<TextBox, string>(Write);
        box.Dispatcher.BeginInvoke(updateAction, box, text);
    }
    // private void StartInactivityTimer(int seconds) {
    //     _inactivityTimer = new DispatcherTimer {
    //         Interval = TimeSpan.FromSeconds(seconds) // 15秒无操作后触发
    //     };
    //     _inactivityTimer.Tick += (s, e) => CloseApplication();
    //     _inactivityTimer.Start();
    //
    //     // 监听所有可能的用户输入事件
    //     PreviewMouseMove += ResetTimerOnActivity;
    //     PreviewKeyDown += ResetTimerOnActivity;
    //     PreviewTouchDown += ResetTimerOnActivity;
    // }
    // 用户有操作时重置计时器
    private void ResetTimerOnActivity(object sender, EventArgs e) {
        _inactivityTimer.Stop();
        _inactivityTimer.Start();
    }
    // 关闭应用程序
    private void CloseApplication() {
        _inactivityTimer.Stop();
        Application.Current.Shutdown();
    }
    
    
    private void PickLongImage(string[] filenames, string finalPath, string trainType="cs-z") {
        AsyncWrite(c2t.box, $"===========寻找类型：{trainType.ToUpper()}===========\n");
        foreach (string f in filenames) {
            string filename = f.Split("/")[^1]; // ubuntu路径处理为文件名
            bool flag = false;
            
            string yearMonthDay = filename.Split("\\")[^1][..10];
            string yearMonthDayPath = Path.Join(SourcePath, yearMonthDay);
            string hour = filename.Split("-")[3];
            if (!Directory.Exists(yearMonthDayPath)) {
                AsyncWrite(c2t.box, $"✗ {filename}的原图已过期或不存在\n");
                continue;
            }
            foreach (string timestamp in Directory.GetDirectories(yearMonthDayPath)) {
                // xxxx-hour-minute-second -> hour
                if (timestamp.Split("\\")[^1].Contains(hour)) {
                    foreach (string orientation in Directory.GetDirectories(timestamp)) {
                        if (!orientation.Split("\\")[^1].Contains(trainType))
                            continue;
                        foreach (string jpg in Directory.GetFiles(orientation)) {
                            string jpgName = jpg.Split("\\")[^1];
                    
                            if (jpgName.CompareTo(filename.Split("\\")[^1]) == 0) {
                                File.Copy(jpg, Path.Join(finalPath, jpgName), true);
                                
                                flag = true;
                                break;
                            }
                        }
                    }
                }
            }
            if (flag)
                AsyncWrite(c2t.box, $"✓ {filename}\n");
            else
                AsyncWrite(c2t.box, $"✗ {filename}的原图已过期或不存在\n");


        }
        // Process.Start("explorer.exe", finalPath);
    }
    private async void GatherWarningClick(object sender, RoutedEventArgs e)
    {
        
        await Task.Run(() =>
        {
            try
            {
                GatherWarning();
                PickWarningLongImages();
                PickImagesByScore();
                FtpUploadWarning();
            }
            catch (Exception exception)
            {
                AsyncWrite(c2t.box, $"{exception.ToString()}\n");
            }
        });
      

    }
    private void GatherWarning()
    {
        string sourceWarningPath = Path.Join(SourcePath, WarningFolder);
        string resultPath = Path.Join(@"D:\", _presentStation, WarningFolder);
        if (Directory.Exists(resultPath))
            Directory.Delete(resultPath, true);
        Directory.CreateDirectory(resultPath);
        string yesterdayFolder = _presentDate.ToString("yyyy-MM-dd");
        foreach (var warningLabel in _warningLabels)
        {
            string warningDirectory = Path.Join(sourceWarningPath, warningLabel);
            if (!Directory.Exists(warningDirectory))
            {
                AsyncWrite(c2t.box, $"{_presentStation}不存在异常检测标签{warningLabel}, 已跳过\n");
                continue;
            }

            foreach (var dateDirectory in Directory.GetDirectories(warningDirectory))
            {
                if (dateDirectory.Split("\\")[^1] != yesterdayFolder)
                    continue;
                foreach (var timestampDirectory in Directory.GetDirectories(dateDirectory))
                {
                    foreach (var trainTypeDirectory in Directory.GetDirectories(timestampDirectory))
                    {
                        foreach (var warningJpg in Directory.GetFiles(trainTypeDirectory))
                        {

                            string[] parts = warningJpg.Split("\\");
                            string filename = parts[^1];
                            string trainType = parts[^2];
                            string timestamp = parts[^3];
                            string date = parts[^4];
                            int hour = int.Parse(timestamp.Split("-")[1]);
                            String amOrPm = hour <= 13 ? "AM" : "PM";
                            string shortTimestamp = $"{date}-{amOrPm}";
                            string shortTrainType = trainType.ToUpper();

                            string newName = $"{warningLabel}+{date}_{timestamp}_{trainType}+{filename}";
                            File.Copy(warningJpg, Path.Join(resultPath, newName));
                            AsyncWrite(c2t.box, $"{newName} \u2713\n");
                        }
                    }
                }
            }
        }

        AsyncWrite(c2t.box, $"Warning长图收集完成，存储在 {resultPath}\n");
    }
    private void PickWarningLongImages()
    {
        string stationPath = Path.Join(@"D:\", _presentStation);
        string resultPath = Path.Join(@"D:\", _presentStation, WarningFolder);
        string manualPath = Path.Join(stationPath, DetectManulFolder);
        if (!Directory.Exists(manualPath))
            Directory.CreateDirectory(manualPath);
        foreach (var jpg in Directory.GetFiles(resultPath))
        {
            string[] parts = jpg.Split("\\")[^1].Split("+");
            string label = parts[0];
            string orientation = parts[1].Split("_")[2].ToLower();
            string timestamp = parts[1].Split("_")[1];
            string date = parts[1].Split("_")[0];
            string filename = parts[^1];
            // a→algorithm; m→not identified
            try
            {
                string targetImage = Path.Join(SourcePath, date, timestamp, orientation, filename);
                File.Copy(targetImage, Path.Join(manualPath, $"客户端_{_presentStation}_{label}_{filename}"), true);
                AsyncWrite(c2t.box, $"{Path.Join(manualPath, $"{_presentStation}_{label}_{filename}")} \u2713\n");
                File.Delete(jpg);
            }
            catch (Exception exception)
            {
                AsyncWrite(c2t.box, exception.ToString() + "\n");
            }
        }
        
    }
    private async void FtpClick(object sender, RoutedEventArgs e)
    {
        
        await Task.Run(() =>
        {
            try
            {
                FtpUploadWarning();

            }
            catch (Exception exception)
            {
                AsyncWrite(c2t.box, exception.ToString());
            }
        });
       
    }
    private void FtpUploadWarning()
    {
        var ftp = Dundun.Ftp();
        (string, string)[] folderPairs = [(DetectManulFolder, "manual_error"), (ScoreFolder , "manual_error")];
        foreach (var (localFolder,remoteFolder) in folderPairs)
        {
            string localPath = Path.Join(@"D:\", _presentStation, localFolder);
            string remotePath = $"{FtpRemotePath}/{remoteFolder}";
            foreach (var warningJpg in Directory.GetFiles(localPath))
            {
                string filename = warningJpg.Split("\\")[^1];
                try
                {
                    ftp.UploadFile(warningJpg, Path.Join(remotePath, filename));
                    AsyncWrite(c2t.box, $"{filename} -> {remotePath}\u2713\n");
                    File.Delete(warningJpg);
                }
                catch 
                {
                    AsyncWrite(c2t.box, $"ftp传输出错，检查文件是否存在\n");
                }
            }
        }
    }
    private void PickImagesByScore()
    {
        try
        {
            File.Copy(
                @"D:\deploy\TrainMonitorService.ImageHandler\App_Data\logs\warning\score\warning.txt", 
                @"D:\warning.txt",
                true);
        }
        catch (Exception e)
        {
            AsyncWrite(c2t.box, e.ToString() + "\n");
        }
        
        string resultPath = $@"D:\{_presentStation}\{ScoreFolder}";
        if (Directory.Exists(resultPath))
            Directory.Delete(resultPath, true);
        Directory.CreateDirectory(resultPath);
        
        string filePath = @"D:\warning.txt";
        if (!File.Exists(filePath))
        {
            AsyncWrite(c2t.box, $"{filePath}不存在\n");
            return;
        }
            
        string[] lines = File.ReadLines(filePath).ToArray();
        
        foreach (string line in lines)
        {
            try
            {
                string[] parts = line.Split(' ');
                string imagePath = parts[3].Split("e:")[1];
                string date = imagePath.Split("/")[3];
                if (date != _presentDate.ToString("yyyy-MM-dd"))
                    continue;
                string type = parts[4].Split(":")[1];
                string defectScore = parts[5].Split(":")[1].Substring(2, 2);
                string configScore = parts[6].Split(":")[1];
                if (configScore.Length == 3)
                    configScore = configScore.Substring(2, 1) + "0";
                else
                    configScore = configScore.Substring(2, 2);
                string newName = $"二级分数_{_presentStation}_{type}_{defectScore}_{configScore}_{imagePath.Split("\\")[^1].Split("/")[^1]}";
                File.Copy(imagePath, Path.Join(resultPath, newName), true);
                AsyncWrite(c2t.box,$"{newName} \u2713\n");
            }
            catch (Exception e)
            {
                AsyncWrite(c2t.box, $"{e} \n");
            }
        }
        
    }
    private void DatetimeChanged(object sender, SelectionChangedEventArgs e)
    {
        if (initializing)
        {
            initializing = false;
            return;
        }
        var t = ((ComboBox)sender).SelectedItem;
        _presentDate = (DateTime)t;
        Console.WriteLine($"测试日期 -> {_presentDate:yyyy-MM-dd}");
        GatherWarningButton.IsEnabled = true;
        
    }

    private async void PickWarningButton_OnClick(object sender, RoutedEventArgs e)
    {
        
        await Task.Run(() =>
        {
            try
            {
                PickWarningLongImages();

            }
            catch (Exception exception)
            {
                AsyncWrite(c2t.box, $"{exception.ToString()}");
                return;
            }
        });
        GatherWarningButton.IsEnabled = true;


    }
    private void OcrPickLongImage(string[] csvRows, string finalPath, string trainType="cs-z") {
        AsyncWrite(c2t.box, $"===========寻找类型：{trainType.ToUpper()}===========\n");
        foreach (string row in csvRows) {
            string[] cols = row.Replace("\"", "").Split(',');
            string filename = cols[^8].Split("/")[^1];
            string number1 = cols[^3].Replace("-", "").Replace("F", "");
            string number2 = cols[^2];
            string newName = $"{number1}_{number2}_{filename}.jpg";
            //filename 获取文件名
            //获取车号1-车号2
            bool flag = false;
            string yearMonthDay = filename.Split("\\")[^1][..10];
            string yearMonthDayPath = Path.Join(SourcePath, yearMonthDay);
            string hour = filename.Split("-")[3];
            if (!Directory.Exists(yearMonthDayPath)) {
                AsyncWrite(c2t.box, $"✗ {filename}的原图已过期或不存在\n");
                continue;
            }
            foreach (string timestamp in Directory.GetDirectories(yearMonthDayPath)) {
                // xxxx-hour-minute-second -> hour
                if (timestamp.Split("\\")[^1].Contains(hour)) {
                    foreach (string orientation in Directory.GetDirectories(timestamp)) {
                        if (!orientation.Split("\\")[^1].Contains(trainType))
                            continue;
                        foreach (string jpg in Directory.GetFiles(orientation)) 
                        {
                            string jpgName = jpg.Split("\\")[^1];
                            if (jpgName.CompareTo(filename.Split("\\")[^1]) == 0) {
                                File.Copy(jpg, Path.Join(finalPath, newName), true);
                                flag = true;
                                break;
                            }
                        }
                    }
                }
            }
            if (flag)
                AsyncWrite(c2t.box, $"✓ {filename}\n");
            else
                AsyncWrite(c2t.box, $"✗ {filename}的原图已过期或不存在\n");


        }
        // Process.Start("explorer.exe", finalPath);
    }


    
}