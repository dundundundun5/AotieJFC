using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Configuration;
using TrainTestTool.Models;

namespace TrainTestTool.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private IConfiguration Configuration;
    private string AbsolutePath;
    private string SegmentTestPath;
    private string FtpRemotePath,
        SegmentTestFTPFolder;


    
    [ObservableProperty]
    private string _output;

    [ObservableProperty]
    private string[] _trainTypeList;
    [ObservableProperty]
    private string[] _stationList;
    [ObservableProperty]
    private int[] _gapList;
    [ObservableProperty]
    private DateTime[] _dateList;
    [ObservableProperty]
    private string[] _defectionList;
    
    [ObservableProperty]
    private string _presentStation;
    [ObservableProperty]
    private DateTime _presentDate;
    [ObservableProperty] 
    private string _presentTrainType;
    [ObservableProperty]
    private int _presentGap;
    public MainWindowViewModel()
    {
        
        Configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        AbsolutePath = Configuration.GetSection("根路径").Value;
        FtpRemotePath = Configuration.GetSection("FTP路径").Value;
      
        SegmentTestFTPFolder = "segment_test";
        SegmentTestPath = Path.Join(FtpRemotePath, SegmentTestFTPFolder);
        DateList = [DateTime.Now.AddDays(-2).Date, DateTime.Now.AddDays(-1).Date, DateTime.Now.Date];
        _defectionList = Configuration.GetSection("异常标签").GetChildren().Select(a => a.Value).ToArray();
        StationList = Configuration.GetSection("站点").GetChildren().Select(a => a.Value).ToArray();
        GapList =
        [
            1,
            8,
            14,
            20,
            50,
            -30
        ];
        TrainTypeList = Configuration.GetSection("长图类型").GetChildren().Select(a => a.Value).ToArray();
       
        
        Console.WriteLine($"切割测试日期: {DateTime.Now.AddDays(-1):yyyy-MM-dd}");
        Console.WriteLine($"短图收集：GAP={PresentGap}");
    }
    private void FtpUploadShortSoftwareImage() 
    {
        SimpleFtpClient ftp;
        string localPath = Path.Join(AbsolutePath, PresentStation, ShortSoftware);
        if (Directory.Exists(Path.Join(localPath, "error")))
            localPath = Path.Join(localPath, "error");//如果有error文件夹说明做过算法
        string testPath = Path.Join(AbsolutePath, PresentStation, SplitJpgName);
        if (!Directory.Exists(localPath) || Directory.GetFiles(localPath).Length == 0) {
            return;
        }
        
        try {
            ftp = Dundun.Ftp();
            string[] jpgs = Directory.GetFiles(localPath);
            WriteLine($"================FTP UPLOADING================");
            foreach (var jpg in jpgs) {
                try {
                    string name = jpg.Split("\\")[^1];
                    if (name[^1] != 'g')
                    {
                        WriteLine($"{jpg}不是jpg文件，已跳过");
                    }
                        
                    ftp.UploadFile(jpg, Path.Join(SegmentTestPath, name));
                    WriteLine($"local.{jpg} -> remote.{SegmentTestPath}");
                }
                catch (Exception e) {
                    WriteLine($"local.{jpg} upload failed");
                    continue;
                }
            }
            WriteLine($"================FTP DONE================");
        }
        catch (Exception ex) {
            WriteLine(ex.ToString());
            return;
        }
    }
    private void GatherLongImageOnlyLocomotive() 
     {
        string resultPath = Path.Join(AbsolutePath, PresentStation, OnlyLocomotiveFolder);
        // bug: 建国总是照片在后台打开，系统问题
        try
        {
            if (Directory.Exists(resultPath))
                Directory.Delete(resultPath, true);
        }
        catch (Exception e)
        {
            
        }
        
        Directory.CreateDirectory(resultPath);

        string yesterdayPath = Path.Join(AbsolutePath, LongImageFolder, $"{_presentDate:yyyy-MM-dd}");
        int total = 0;
        foreach (var timestampDirectory in Directory.GetDirectories(yesterdayPath)) {
            string cur = "null";
            DateTime curDate = DateTime.MinValue;
            foreach (var trainTypeDirectory in Directory.GetDirectories(timestampDirectory)) {
                if (trainTypeDirectory.Contains("-")) {
                    foreach (var imageFile in Directory.GetFiles(trainTypeDirectory, "*.jpg")) {

                        string newFileName = Dundun.FileName4Train(imageFile);
                        cur = newFileName;
                        curDate = File.GetLastWriteTime(imageFile);
                        string destinationPath = Path.Join(resultPath, newFileName);
                        File.Copy(imageFile, destinationPath, true);

                        // WriteAsync(MyTextbox.box, $"{newFileName} -> {resultPath} \u2713");
                        break; //每个文件夹一张车头的图
                    }
                }
            }
            total += 1;
            if (cur == "null")
                continue;
            string[] t = cur.Split("+");
            string ms = t[^1].Split(".")[0].Split('-')[^1];
            string newMs = (int.Parse(ms) + 1).ToString();

            string newName = $"{t[0]}+{t[1]}+{t[^1][..^7]}{newMs}.jpg";
            string avatarTargetPath = Path.Join(resultPath, newName);
            string avatarSourcePath = Path.Join(AbsolutePath, PresentStation, SplitJpgName);

            File.Copy(avatarSourcePath, avatarTargetPath, true);
            File.SetLastWriteTime(avatarTargetPath, curDate.AddSeconds(2));
            WriteLine($"{avatarSourcePath} -> {avatarTargetPath} \u2713");
      
        }
        WriteLine($"total={total}，长图收集完成，存储在 {resultPath}");
        Process.Start("explorer.exe", resultPath);
    }
    private void GatherRearImages()
    {
        string resultPath = Path.Join(AbsolutePath, PresentStation, OnlyRearFolder);
        if (Directory.Exists(resultPath))
            Directory.Delete(resultPath, true);
        Directory.CreateDirectory(resultPath);
        string yesterdayPath = Path.Join(AbsolutePath, LongImageFolder, $"{_presentDate:yyyy-MM-dd}");
        int total = 0;
        foreach (var timestampDirectory in Directory.GetDirectories(yesterdayPath))
        {
            foreach (var trainTypeDirectory in Directory.GetDirectories(timestampDirectory))
            {
                if (trainTypeDirectory.Contains(PresentTrainType))
                {
                    var imageFile = Directory.GetFiles(trainTypeDirectory, "*.jpg")[^1];
                    string newFileName = Dundun.FileName4Train(imageFile);
                    string destinationPath = Path.Join(resultPath, newFileName);
                    File.Copy(imageFile, destinationPath, true);
                    WriteLine($"{newFileName} -> {resultPath} \u2713");
                }
            }
            total += 1;
        }
        WriteLine($"total={total}，车尾长图收集完成，存储在 {resultPath}");
        Process.Start("explorer.exe", resultPath);


    }
    
    private string GenerateCsv(string longImagePath)
    {
        DateTime yesterdayDate = DateTime.Now.AddDays(-1);
        string res = "";
        string csv = $"{PresentStation}CS{yesterdayDate:MMdd}.csv";
        using (StreamWriter writer = new StreamWriter(Path.Join(AbsolutePath, PresentStation, csv), append: false, encoding: System.Text.Encoding.UTF8)) {
            writer.WriteLine("时间,朝向,错误描述,文件名");
            res += $"时间,朝向,错误描述,文件名";
            foreach (string jpg in Directory.GetFiles(longImagePath)) {
                string[] parts = jpg.Split("\\")[^1].Split("+");
                string errorDescription = parts[0]; // hxxx or sxx
                string orientation = parts[1][..2]; // SX-CS-Z -> SX
                string[] errorTimes = parts[^1].Split("-")[3..6];
                string errorTime = $"{errorTimes[0]}{errorTimes[1]}{errorTimes[2]}";
                string filename = parts[^1].Split(".")[0];
                writer.WriteLine($"{errorTime},{orientation},{errorDescription},{filename}");
                res += $"{errorTime},{orientation},{errorDescription},{filename}";
            }
        }
        return res;
    }
    private string? CheckPresentStation(string absolutePath, string[] stationList) 
    {
        for (int i = 0; i < StationList.Length; i++) 
        {
            if (Directory.Exists(Path.Join(AbsolutePath, StationList[i])))
            {
                return StationList[i];
            }
        }
        return null;
    }
    private void GatherLongImages() 
    {
        string resultPath = Path.Join(AbsolutePath, PresentStation, GatheredImageFolder);
        if (Directory.Exists(resultPath))
            Directory.Delete(resultPath, true);
        Directory.CreateDirectory(resultPath);

        string yesterdayPath = Path.Join(AbsolutePath, LongImageFolder, $"{PresentDate:yyyy-MM-dd}");
        
        int total = 0;
        int second = 0;
        int sx = 0, xx = 0;
        
        foreach (var timestampDirectory in Directory.GetDirectories(yesterdayPath)) {
            string cur = "null";
            DateTime curDate = DateTime.Now;
            foreach (var trainTypeDirectory in Directory.GetDirectories(timestampDirectory))
            {
                if (trainTypeDirectory.Contains(PresentTrainType)) {
                    foreach (var imageFile in Directory.GetFiles(trainTypeDirectory, "*.jpg")) {

                        string newFileName = Dundun.FileName4Train(imageFile);
                        cur = newFileName;
                        curDate = File.GetLastWriteTime(imageFile);
                        string destinationPath = Path.Join(resultPath, newFileName);
                        File.Copy(imageFile, destinationPath, true);
                    }
                }
            }
            total += 1;
            if (cur == "null")
                continue;
            string[] t = cur.Split("+");
            string ms = t[^1].Split(".")[0].Split('-')[^1];
            string newMs = (int.Parse(ms) + 1).ToString();

            string newName = $"{t[0]}+{t[1]}+{t[^1][..^7]}{newMs}.jpg";
            if (t[0].Contains("SX"))
                sx++;
            else
                xx++;
            string avatarTargetPath = Path.Join(resultPath, newName);
            string avatarSourcePath = Path.Join(AbsolutePath, PresentStation, SplitJpgName);

            File.Copy(avatarSourcePath, avatarTargetPath, true);
            File.SetLastWriteTime(avatarTargetPath, curDate.AddMilliseconds(2));
            WriteLine($"{avatarSourcePath} -> {avatarTargetPath} \u2713");
        }
        WriteLine($"total={total},sx={sx},xx={xx},长图收集完成，存储在 {resultPath}");
        //WriteLine(MyTextbox.box, $"如果站点选择错误，则将{resultPath}的{resultPath.Split('\\')[1]}手动改成实际站点名称");
        Process.Start("explorer.exe", resultPath);


    }
    private string PickShortImages() {
        string longHardwareImagePath = Path.Join(AbsolutePath, PresentStation,  LongHardware);
        string longSoftwareImagePath = Path.Join(AbsolutePath, PresentStation, LongSoftware);
        string shortSoftwarePath = Path.Join(AbsolutePath, PresentStation, ShortSoftware);
        string longImagePath = Path.Join(AbsolutePath, PresentStation, GatheredImageFolder);
        string locomotiveImagePath = Path.Join(AbsolutePath, PresentStation, OnlyLocomotiveFolder);
        // if (Directory.Exists(longSoftwareImagePath))
        //     Directory.Delete(longSoftwareImagePath, true);
        if (!Directory.Exists(longHardwareImagePath))
            Directory.CreateDirectory(longHardwareImagePath);
        if (Directory.Exists(shortSoftwarePath))
            Directory.Delete(shortSoftwarePath, true);
        Directory.CreateDirectory(longSoftwareImagePath);
        Directory.CreateDirectory(shortSoftwarePath);
        try
        {
            if (Directory.Exists(longImagePath))
            {
                foreach (string jpg in Directory.GetFiles(longImagePath))
                {
                    string[] parts = jpg.Split("+");
                    string errorDescription = parts[0]; // hxxx or sxx
                    if (parts.Length == 3)
                    {
                        try
                        {
                            File.Delete(jpg);
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
            }
        }
        catch (DirectoryNotFoundException e)
        {
            WriteLine($"{longImagePath}不存在，跳过");
        }
        try
        {
            foreach (string jpg in Directory.GetFiles(locomotiveImagePath))
            {
                string[] parts = jpg.Split("+");
                string errorDescription = parts[0]; // hxxx or sxx
                if (parts.Length == 3)
                {
                    try
                    {
                        File.Delete(jpg);
                    }
                    catch
                    {
                        continue;
                    }
                }
                else
                {
                    string name = jpg.Split("\\")[^1];
                    string newJpg = Path.Join(AbsolutePath, PresentStation, GatheredImageFolder, name);
                    File.Move(jpg, newJpg, true);
                    WriteLine($"{jpg} -> {newJpg}");
                }
            }
            WriteLine($"删除完毕！");
        }
        catch (DirectoryNotFoundException e)
        {
            WriteLine($"{locomotiveImagePath}不存在，跳过");
        }

        if (!Path.Exists(longImagePath))
        {
            WriteLine($"{longImagePath}不存在 ");
            return "";
        }
        if (Directory.GetFiles(longImagePath).Length == 0) {
            WriteLine($"切割全对！");
            return "";
        }
        string res = GenerateCsv(longImagePath);
        string[] files = Directory.GetFiles(longImagePath);
        for (int j = 0; j < files.Length; j++) {
            string jpg = files[j];
            try {
                string[] parts = jpg.Split("\\")[^1].Split("+");
                string errorDescription = parts[0]; // hxxx or sxx
                if (errorDescription.ToLower().Contains("h")) {
                    File.Move(jpg, Path.Join(longHardwareImagePath, jpg.Split("\\")[^1]), true);
                }
                //software error
                else if (errorDescription.ToLower().Contains("s")) {
                    string shortImagePath = Path.Join(AbsolutePath, ShortImageFolder, parts[1], parts[2]);
                    string name = Path.Join(AbsolutePath, ShortImageFolder, parts[1], parts[2], parts[3]);
                    var imgs = Directory.GetFiles(shortImagePath);
                    int idx = -1;
                    for (int i = 0; i < imgs.Length; i++) {
                        /// 大于等
                        if (imgs[i].Split("\\")[^1].CompareTo(parts[3]) >= 0) {
                            idx = i;
                            WriteLine($"找到 {parts[3]} 在 {imgs[i]} index={idx}");
                            break;
                        }
                    }
                    if (idx == -1) {
                        WriteLine($"{jpg}的短图不存在或已过期，已跳过");
                        File.Move(jpg, Path.Join(longSoftwareImagePath, jpg.Split("\\")[^1]), true);
                        continue;
                    }
                    if (idx >= 0) {
                        int start, end;
                        int bias = -2;
                        //TODO 配置文件化
                        
                        if (PresentGap < 0)
                        {
                            start = Math.Max(0, idx + PresentGap);
                            end = Math.Min(idx - bias, imgs.Length - 1);
                        }
                        else
                        {
                            start = Math.Max(0, idx + bias);
                            end = Math.Min(idx + PresentGap, imgs.Length - 1);
                        }
                            for (int i = start; i <= end; i++)
                            {
                                string temp = $"{PresentStation}_{parts[0]}_{parts[1]}_{Path.GetFileName(imgs[i])}";
                                string dest = Path.Join(shortSoftwarePath, temp);
                                File.Copy(imgs[i], dest, true);
                                WriteLine($"copy {imgs[i]} -> {dest}\u2713");
                            }
                    }
                    File.Move(jpg, Path.Join(longSoftwareImagePath, jpg.Split("\\")[^1]), true);
                }
            }
            catch (Exception) {
                WriteLine($"{jpg}的短图不存在或已过期，已跳过");
                continue;
            }
        }
        return res;
    }
    private void GatherWarning()
    {
        string sourceWarningPath = Path.Join(AbsolutePath, LongImageFolder, WarningFolder);
        string resultPath = Path.Join(@"D:\", _presentStation, WarningFolder);
        if (Directory.Exists(resultPath))
            Directory.Delete(resultPath, true);
        Directory.CreateDirectory(resultPath);
        string yesterdayFolder = _presentDate.ToString("yyyy-MM-dd");
        foreach (var warningLabel in _defectionList)
        {
            string warningDirectory = Path.Join(sourceWarningPath, warningLabel);
            if (!Directory.Exists(warningDirectory))
            {
                WriteLine($"{_presentStation}不存在异常检测标签{warningLabel}, 已跳过");
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
                            WriteLine($"{newName} \u2713");
                        }
                    }
                }
            }
        }

        WriteLine($"Warning长图收集完成，存储在 {resultPath}");
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
                string targetImage = Path.Join(AbsolutePath, LongImageFolder, date, timestamp, orientation, filename);
                File.Copy(targetImage, Path.Join(manualPath, $"客户端_{PresentStation}_{label}_{filename}"), true);
                WriteLine($"{Path.Join(manualPath, $"{PresentStation}_{label}_{filename}")} \u2713");
                File.Delete(jpg);
            }
            catch (Exception exception)
            {
                WriteLine(exception.ToString() + "");
            }
        }
        
    }
    private void FtpUploadWarning()
    {
        var ftp = Dundun.Ftp();
        (string, string)[] folderPairs = [(DetectManulFolder, "manual_error"), (ScoreFolder , "manual_error")];
        foreach (var (localFolder,remoteFolder) in folderPairs)
        {
            string localPath = Path.Join(@"D:\", PresentStation, localFolder);
            string remotePath = $"{FtpRemotePath}/{remoteFolder}";
            foreach (var warningJpg in Directory.GetFiles(localPath))
            {
                string filename = warningJpg.Split("\\")[^1];
                try
                {
                    ftp.UploadFile(warningJpg, Path.Join(remotePath, filename));
                    WriteLine($"{filename} -> {remotePath}\u2713");
                    File.Delete(warningJpg);
                }
                catch 
                {
                    WriteLine($"ftp传输出错，检查文件是否存在");
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
            WriteLine(e.ToString() + "");
        }
        
        string resultPath = $@"D:\{PresentStation}\{ScoreFolder}";
        if (Directory.Exists(resultPath))
            Directory.Delete(resultPath, true);
        Directory.CreateDirectory(resultPath);
        string filePath = @"D:\warning.txt";
        if (!File.Exists(filePath))
        {
            WriteLine($"{filePath}不存在");
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
                WriteLine($"{newName} \u2713");
            }
            catch (Exception e)
            {
                WriteLine($"{e} ");
            }
        }
        
    }
    private void WriteLine(string content)
    {
        Dispatcher.UIThread.Invoke((() =>
        {
            Output += $"{content}{Environment.NewLine}";
        }));
    }
}