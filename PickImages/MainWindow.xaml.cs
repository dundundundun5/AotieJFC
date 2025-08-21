using Microsoft.Win32;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.Formats.Tar;
using System.IO;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using PickImagesTool;
using Path = System.IO.Path;
namespace PickImages;

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
    private const string DetectCsFolder = "车身误检测", DetectZxFolder = "走行误检测", DetectManulFolder = "manual_error";
    private DispatcherTimer? _inactivityTimer;
    private static DateTime _today = DateTime.Now;
    private static bool initializing = true;
    private static DateTime _yesterday = DateTime.Now.AddDays(-1);
    private static DateTime __yyesterday = DateTime.Now.AddDays(-2);
    private static DateTime __yyyesterday = DateTime.Now.AddDays(-3);
    private static DateTime __yyyyesterday = DateTime.Now.AddDays(-4);
    private DateTime _presentDate = _yesterday;
    public DateTime[] DateList { get; set; } = [_today, _yesterday, __yyesterday, __yyyesterday, __yyyyesterday];
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
        StartInactivityTimer(seconds: 60);
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
        foreach (string desktopFile in Directory.GetDirectories(desktopPath)) {
            if (String.Compare(desktopFile.Split("\\")[^1], DetectCsFolder, StringComparison.Ordinal) == 0) {
                DetectCSFolder.Text = desktopFile;
                DetectCSButton.Content = "寻找原图";
            }
            if (String.Compare(desktopFile.Split("\\")[^1], DetectZxFolder, StringComparison.Ordinal) == 0) {
                DetectZXFolder.Text = desktopFile;
                DetectZXButton.Content = "寻找原图";
            }
        }

    }
    private void AsyncWrite(TextBox box, string text) {
        void Write(System.Windows.Controls.TextBox textBox, string t) {
            textBox.AppendText(t);
            textBox.ScrollToEnd();
        }
        Action<TextBox, string> updateAction = new Action<TextBox, string>(Write);
        box.Dispatcher.BeginInvoke(updateAction, box, text);
    }
    private void StartInactivityTimer(int seconds) {
        _inactivityTimer = new DispatcherTimer {
            Interval = TimeSpan.FromSeconds(seconds) // 15秒无操作后触发
        };
        _inactivityTimer.Tick += (s, e) => CloseApplication();
        _inactivityTimer.Start();

        // 监听所有可能的用户输入事件
        PreviewMouseMove += ResetTimerOnActivity;
        PreviewKeyDown += ResetTimerOnActivity;
        PreviewTouchDown += ResetTimerOnActivity;
    }
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
    private async void OCRButton1_Click(object sender, RoutedEventArgs e) {
        if (OCRFile1.Text.Contains($"选择站点名_日期XXX.csv")) {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "逗号分隔文件 (*.csv)|*.csv";
            if (openFileDialog.ShowDialog() == true) {
                OCRFile1.Text = openFileDialog.FileName;
                AsyncWrite(c2t.box, $"目标文件 -> {OCRFile1.Text}\n");
            }
        }
        string txtFile = OCRFile1.Text;
        string finalFolder = Path.GetDirectoryName(txtFile);
        string name = txtFile.Split("\\")[^1].Split(".")[0];
        string ocrPath = Path.Join(finalFolder, name);

        if (!Directory.Exists(ocrPath))
            Directory.CreateDirectory(ocrPath);
        if (OCRFile1.Text.Contains($"选择站点名_日期XXX.csv")) {
            Console.WriteLine("您未选择任何文件，必须选择一个文件");
            return; 
        }
        string[] ocrFiles = File.ReadAllLines(txtFile)[1..];
        OCRButton1.IsEnabled = false;
        await Task.Run(() => {
            // 这里执行长时间运行的操作
            //print();
            try {
                // 可能会抛出异常的代码
                OcrPickLongImage(csvRows: ocrFiles, finalPath: ocrPath);
            }
            catch (Exception ex) {
                // 显示异常信息
                AsyncWrite(c2t.box, $"{ex.ToString()}");
                return;
            }
        });
        StartInactivityTimer(3);
    }
    private async void DetectCSButton_Click(object sender, RoutedEventArgs e) {
        if (DetectCSFolder.Text.Contains($"选择{DetectCsFolder}")) {
            OpenFolderDialog openFolderDialog = new OpenFolderDialog();
            if (openFolderDialog.ShowDialog() == true) {
                DetectCSFolder.Text = openFolderDialog.FolderName;
                AsyncWrite(c2t.box, $"目标文件夹 -> {DetectCSFolder.Text}\n");
            }
        }
        string path = DetectCSFolder.Text; // 获取路径
        string[] filenames = Directory.GetFiles(path, searchPattern: "*.jpg");
        string detectPath = Path.Join(path, "原图");
        if (!Directory.Exists(detectPath)) 
            Directory.CreateDirectory(detectPath);
        DetectCSButton.IsEnabled = false;
        await Task.Run(() => {
            // 这里执行长时间运行的操作
            //print();
            try {
                // 可能会抛出异常的代码
                PickLongImage(filenames: filenames, finalPath: detectPath);
                //PickLongImage(filenames: filenames, finalPath: detectPath, trainType:"cs-y");
            }
            catch (Exception ex) {
                // 显示异常信息

                AsyncWrite(c2t.box, $"{ex.ToString()}");
                return;
            }
        });

    }
    private async void DetectZXButton_Click(object sender, RoutedEventArgs e) {
        if (DetectZXFolder.Text.Contains($"选择{DetectZxFolder}")) {
            OpenFolderDialog openFolderDialog = new OpenFolderDialog();
            if (openFolderDialog.ShowDialog() == true) {
                DetectZXFolder.Text = openFolderDialog.FolderName;
                AsyncWrite(c2t.box, $"目标文件夹 -> {DetectZXFolder.Text}\n");
            }
        }

        string path = DetectZXFolder.Text; // 获取路径
        string[] filenames = Directory.GetFiles(path, searchPattern: "*.jpg");
        string detectPath = Path.Join(path, "原图");
        if (!Directory.Exists(detectPath))
            Directory.CreateDirectory(detectPath);
        DetectZXButton.IsEnabled = false;
        await Task.Run(() => {
            // 这里执行长时间运行的操作
            //print();
            try {
                // 可能会抛出异常的代码
                PickLongImage(filenames: filenames, finalPath: detectPath, trainType:"zx-z");
                //PickLongImage(filenames: filenames, finalPath: detectPath, trainType: "zx-y");
            }
            catch (Exception ex) {
                // 显示异常信息

                AsyncWrite(c2t.box, $"{ex.ToString()}");
                return;
            }
        });
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
        // FtpButton.IsEnabled = true;
        GatherWarningButton.IsEnabled = false;
        await Task.Run(() =>
        {
            try
            {
                GatherWarning();
                PickWarningLongImages();
                FtpUploadWarning();
            }
            catch (Exception exception)
            {
                AsyncWrite(c2t.box, $"{exception.ToString()}\n");
                return;
            }
        });
        StartInactivityTimer(seconds:2);
        // PickWarningButton.IsEnabled = true;

    }
    private void GatherWarning()
    {
        string sourceWarningPath = Path.Join(SourcePath, WarningFolder);
        string resultPath = Path.Join(@"D:\", _presentStation, WarningFolder);
        if (Directory.Exists(resultPath))
            Directory.Delete(resultPath, true);
        Directory.CreateDirectory(resultPath);
        //result -> D:\\station\\warnings
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
    

        // Process.Start("explorer.exe", resultPath);
    }
    private void PickWarningLongImages()
    {
        string stationPath = Path.Join(@"D:\", _presentStation);
        string resultPath = Path.Join(@"D:\", _presentStation, WarningFolder);
        // string csPath = Path.Join(stationPath, DetectCsFolder), zxPath = Path.Join(stationPath, DetectZxFolder);
        string manualPath = Path.Join(stationPath, DetectManulFolder);
        int zxAlgorithmError = 0, csAlgorithmError = 0;
        // if (!Directory.Exists(csPath))
        //     Directory.CreateDirectory(csPath);
        // if (!Directory.Exists(zxPath))
        //     Directory.CreateDirectory(zxPath);
        if (!Directory.Exists(manualPath))
            Directory.CreateDirectory(manualPath);
        foreach (var jpg in Directory.GetFiles(resultPath))
        {
            ////ZJ+2025-07-23_242008-15-54-20_xx-zx-z+XX-ZX-Z_2025-07-23-PM+2025-07-23-15-54-21-078.jpg
            string[] parts = jpg.Split("\\")[^1].Split("+");
            // string errorType = parts[0];
            // if (errorType == "a")
            //     parts = parts[1..];
            string label = parts[0];
            string orientation = parts[1].Split("_")[2].ToLower();
            string timestamp = parts[1].Split("_")[1];
            string date = parts[1].Split("_")[0];
            string filename = parts[^1];
            // a→algorithm; m→not identified
            try
            {
                string targetImage = Path.Join(SourcePath, date, timestamp, orientation, filename);
                File.Copy(targetImage, Path.Join(manualPath, $"{_presentStation}_{label}_{filename}"), true);
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
        FtpButton.IsEnabled = false;
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
        StartInactivityTimer(seconds:2);
    }
    private void FtpUploadWarning()
    {
        var ftp = Dundun.Ftp();
        (string, string)[] folderPairs = [(DetectManulFolder, "manual_error")];
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
        PickWarningButton.IsEnabled = false;
    }

    private async void PickWarningButton_OnClick(object sender, RoutedEventArgs e)
    {
        
        PickWarningButton.IsEnabled = false;
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