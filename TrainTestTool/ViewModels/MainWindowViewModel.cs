using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using Serilog;
using TrainTestTool.Models;

namespace TrainTestTool.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    
    [ObservableProperty]
    private string _output;
    [ObservableProperty]
    private string _presentStation;
    [ObservableProperty]
    private DateTime _presentDate = DateTime.Now.AddDays(-1);
    [ObservableProperty] 
    private string _presentTrainType;
    [ObservableProperty]
    private int _presentGap;
    [ObservableProperty]
    private bool stationSelectable = true;
    public MainWindowViewModel() : base()
    {
        PresentTrainType = TrainTypeList[0];
        PresentGap = GapList[1];
        PresentDate = DateList[1];
        PresentStation = "未知站点";
        if (StationCheck())
        {
            stationSelectable = false;
        }
        var stationPath = Path.Join(AbsolutePath, PresentStation);
        if (!Directory.Exists(stationPath))
            Directory.CreateDirectory(stationPath);
        WriteLine($"PresentStation: {PresentStation}");
        WriteLine($"Date: {PresentDate:yyyy-MM-dd}");
        WriteLine($"GAP={PresentGap}");
    }
    [RelayCommand]
    private void FtpUploadShortSoftwareImage() 
    {
        SimpleFtpClient ftp;
        string localPath = Path.Join(AbsolutePath, PresentStation, ShortSoftware);
        if (Directory.Exists(Path.Join(localPath, "error")))
            localPath = Path.Join(localPath, "error");//如果有error文件夹说明做过算法
        if (!Directory.Exists(localPath) || Directory.GetFiles(localPath).Length == 0) {
            return;
        }
        
        try {
            ftp = GetFtpClient();
            string[] jpgs = Directory.GetFiles(localPath);
            WriteLine($"================FTP UPLOADING================");
            foreach (var jpg in jpgs) {
                try {
                    string name = jpg.Replace("\\", "/").Split("/")[^1];
                    if (name[^1] != 'g')
                    {
                        WriteLine($"{jpg}不是jpg文件，已跳过");
                    }
                        
                    ftp.UploadFile(jpg, Path.Join(FtpSegmentPath, name));
                    WriteLine($"local.{jpg} -> remote.{FtpSegmentPath}");
                }
                catch (Exception e) {
                    WriteLine($"local.{jpg} upload failed");
                }
            }
            WriteLine($"================FTP DONE================");
        }
        catch (Exception ex) {
            WriteLine(ex.ToString());
        }
    }
    [RelayCommand]
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
            WriteLine(e.ToString());
        }
        
        Directory.CreateDirectory(resultPath);

        string yesterdayPath = Path.Join(AbsolutePath, LongImageFolder, $"{PresentDate:yyyy-MM-dd}");
        if (!Directory.Exists(yesterdayPath))
        {
            WriteLine($"路径不存在 - {yesterdayPath}");
            return;
        }
        int total = 0;
        foreach (var timestampDirectory in Directory.GetDirectories(yesterdayPath)) {
            string cur = "null";
            DateTime curDate = DateTime.MinValue;
            foreach (var trainTypeDirectory in Directory.GetDirectories(timestampDirectory)) {
                if (trainTypeDirectory.Contains('-')) {
                    foreach (var imageFile in Directory.GetFiles(trainTypeDirectory, "*.jpg")) {

                        string newFileName = FileName4Train(imageFile);
                        cur = newFileName;
                        curDate = File.GetLastWriteTime(imageFile);
                        string destinationPath = Path.Join(resultPath, newFileName);
                        File.Copy(imageFile, destinationPath, true);

                        WriteLine($"{newFileName} -> {resultPath} \u2713");
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
            string avatarSourcePath = Path.Join(Directory.GetCurrentDirectory(), SplitImageName);

            File.Copy(avatarSourcePath, avatarTargetPath, true);
            File.SetLastWriteTime(avatarTargetPath, curDate.AddSeconds(2));
            WriteLine($"{avatarSourcePath} -> {avatarTargetPath} \u2713");
      
        }
        WriteLine($"total={total}，长图收集完成，存储在 {resultPath}");
        if (OperatingSystem.IsWindows())
        {
            Process.Start("explorer.exe", resultPath);
        }
    }
    [RelayCommand]
    private void GatherRearImages()
    {
        string resultPath = Path.Join(AbsolutePath, PresentStation, OnlyRearFolder);
        if (Directory.Exists(resultPath))
            Directory.Delete(resultPath, true);
        Directory.CreateDirectory(resultPath);
        string yesterdayPath = Path.Join(AbsolutePath, LongImageFolder, $"{PresentDate:yyyy-MM-dd}");
        if (!Directory.Exists(yesterdayPath))
        {
            WriteLine($"路径不存在 - {yesterdayPath}");
            return;
        }
        int total = 0;
        foreach (var timestampDirectory in Directory.GetDirectories(yesterdayPath))
        {
            foreach (var trainTypeDirectory in Directory.GetDirectories(timestampDirectory))
            {
                if (trainTypeDirectory.Contains(PresentTrainType))
                {
                    var imageFile = Directory.GetFiles(trainTypeDirectory, "*.jpg")[^1];
                    string newFileName = FileName4Train(imageFile);
                    string destinationPath = Path.Join(resultPath, newFileName);
                    File.Copy(imageFile, destinationPath, true);
                    WriteLine($"{newFileName} -> {resultPath} \u2713");
                }
            }
            total += 1;
        }
        WriteLine($"total={total}，车尾长图收集完成，存储在 {resultPath}");
        if (OperatingSystem.IsWindows())
        {
            Process.Start("explorer.exe", resultPath);
        }


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
                string[] parts = jpg.Replace("\\", "/").Split("/")[^1].Split("+");
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
    [RelayCommand]
    private void GatherLongImages() 
    {
        string resultPath = Path.Join(AbsolutePath, PresentStation, GatheredImageFolder);
        if (Directory.Exists(resultPath))
            Directory.Delete(resultPath, true);
        Directory.CreateDirectory(resultPath);

        string yesterdayPath = Path.Join(AbsolutePath, LongImageFolder, $"{PresentDate:yyyy-MM-dd}");
        if (!Directory.Exists(yesterdayPath))
        {
            WriteLine($"路径不存在 - {yesterdayPath}");
            return;
        }
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

                        string newFileName = FileName4Train(imageFile);
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
            string avatarSourcePath = Path.Join(Directory.GetCurrentDirectory(), SplitImageName);
            
            File.Copy(avatarSourcePath, avatarTargetPath, true);
            File.SetLastWriteTime(avatarTargetPath, curDate.AddMilliseconds(2));
            WriteLine($"{avatarSourcePath} -> {avatarTargetPath} \u2713");
        }
        WriteLine($"total={total},sx={sx},xx={xx},长图收集完成，存储在 {resultPath}");
        if (OperatingSystem.IsWindows())
        {
            Process.Start("explorer.exe", resultPath);
        }
        


    }

    public Action OutputUpdated = () => { };
    private SimpleFtpClient GetFtpClient()
    {
        return new SimpleFtpClient(Ip, Port, Username, Password);
    }
    [RelayCommand]
    private void PickShortImages() {
        string longHardwareImagePath = Path.Join(AbsolutePath, PresentStation,  LongHardware);
        string longSoftwareImagePath = Path.Join(AbsolutePath, PresentStation, LongSoftware);
        string shortSoftwarePath = Path.Join(AbsolutePath, PresentStation, ShortSoftware);
        string longImagePath = Path.Join(AbsolutePath, PresentStation, GatheredImageFolder);
        string locomotiveImagePath = Path.Join(AbsolutePath, PresentStation, OnlyLocomotiveFolder);
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
                        catch (Exception ex)
                        {
                            WriteLine(ex.ToString());
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
                    catch (Exception ex)
                    {
                        WriteLine(ex.ToString());
                    }
                }
                else
                {
                    string name = jpg.Replace("\\", "/").Split("/")[^1];
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
            return;
        }
        if (Directory.GetFiles(longImagePath).Length == 0) {
            WriteLine($"切割全对！");
            return;
        }
        string res = GenerateCsv(longImagePath);
        string[] files = Directory.GetFiles(longImagePath);
        for (int j = 0; j < files.Length; j++) {
            string jpg = files[j];
            try {
                string[] parts = jpg.Replace("\\", "/").Split("/")[^1].Split("+");
                string errorDescription = parts[0]; // hxxx or sxx
                if (errorDescription.ToLower().Contains("h")) {
                    File.Move(jpg, Path.Join(longHardwareImagePath, jpg.Replace("\\", "/").Split("/")[^1]), true);
                }
                //software error
                else if (errorDescription.ToLower().Contains("s")) {
                    string shortImagePath = Path.Join(AbsolutePath, ShortImageFolder, parts[1], parts[2]);
                    string name = Path.Join(AbsolutePath, ShortImageFolder, parts[1], parts[2], parts[3]);
                    var imgs = Directory.GetFiles(shortImagePath);
                    int idx = -1;
                    for (int i = 0; i < imgs.Length; i++) {
                        /// 大于等
                        if (imgs[i].Replace("\\", "/").Split("/")[^1].CompareTo(parts[3]) >= 0) {
                            idx = i;
                            WriteLine($"找到 {parts[3]} 在 {imgs[i]} index={idx}");
                            break;
                        }
                    }
                    if (idx == -1) {
                        WriteLine($"{jpg}的短图不存在或已过期，已跳过");
                        File.Move(jpg, Path.Join(longSoftwareImagePath, jpg.Replace("\\", "/").Split("/")[^1]), true);
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
                    File.Move(jpg, Path.Join(longSoftwareImagePath, jpg.Replace("\\", "/").Split("/")[^1]), true);
                }
            }
            catch (Exception) {
                WriteLine($"{jpg}的短图不存在或已过期，已跳过");
                ;
            }
        }
        return;
    }
    private void WriteLine(string content)
    {
        if (string.IsNullOrEmpty(content))
            return;
        // Dispatcher.UIThread.Invoke((() =>
        // {
        //     Output += $"{content}{Environment.NewLine}";
        // }));
        Log.Information("{}", content);
    }

    private static string GetLocalIpAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
            {
                var ipStr = ip.ToString();
                if (ipStr.StartsWith("192.168."))
                {
                    return ipStr;
                }
            }
        }
        return "";
    }
    private bool StationCheck()
    {
        String ip = GetLocalIpAddress();
        for (var i = 0; i < StationList.Length; i++)
        {
            if (!ip.Contains(IpList[i])) 
                continue;
            PresentStation = StationList[i];
            return true;
        }

        return false;


    }
    private string CountLongImages() {
        string yesterdayPath = Path.Join(AbsolutePath, LongImageFolder, $"{PresentDate:yyyy-MM-dd}");
        
        int count = 0;
        if (!Directory.Exists(yesterdayPath))
        {
            WriteLine($"路径不存在 - {yesterdayPath}");
            return count.ToString();
        }
        int total = 0;
        int second = 0;
        int sx = 0, xx = 0;
        
        foreach (var timestampDirectory in Directory.GetDirectories(yesterdayPath)) {
            string cur = "null";
            foreach (var trainTypeDirectory in Directory.GetDirectories(timestampDirectory))
            {
                if (trainTypeDirectory.Contains("cs-z")) {
                    foreach (var imageFile in Directory.GetFiles(trainTypeDirectory, "*.jpg")) {
                        count++;
                    }
                }
            }
            total += 1;
            if (cur == "null")
                continue;
            string[] t = cur.Split("+");
            if (t[0].Contains("SX"))
                sx++;
            else
                xx++;
        }
        string result = $"{PresentDate:yyyy-MM-dd},{total},{count}";
        return result;
    }
    private void GatherWarning(DateTime dateTime)
    {
        string sourceWarningPath = Path.Join(AbsolutePath, LongImageFolder, WarningFolder);
        if (!Directory.Exists(sourceWarningPath))
        {
            WriteLine($"收集告警的路径不存在 - {sourceWarningPath}");
        }
        string resultPath = Path.Join(AbsolutePath, PresentStation, WarningFolder);
        if (Directory.Exists(resultPath))
            Directory.Delete(resultPath, true);
        Directory.CreateDirectory(resultPath);
        string yesterdayFolder = dateTime.ToString("yyyy-MM-dd");
        foreach (var directoryName in Directory.GetDirectories(sourceWarningPath))
        {
            
            var warningLabel = Path.GetFileName(directoryName).ToUpper();
            // WriteLine($"directoryName = {directoryName}, warningLabel = {warningLabel}");
            if (DefectionList.Any(warningLabel.Equals))
            {
                // WriteLine($"warningLabel = {warningLabel} in DefectionList \u2713");
                string warningDirectory = directoryName;

                foreach (var dateDirectory in Directory.GetDirectories(warningDirectory))
                {
                    
                    var d = Path.GetFileName(dateDirectory);
                    
                    // WriteLine($"dateDirectory = {dateDirectory}");
                    // WriteLine($"after -> {d}");
                    // WriteLine($"yesterdayFolder -> {yesterdayFolder}");
                    if (d != yesterdayFolder)
                        continue;
                    foreach (var timestampDirectory in Directory.GetDirectories(dateDirectory))
                    {
                        // WriteLine($" timestampDirectory -> {timestampDirectory}");
                        foreach (var trainTypeDirectory in Directory.GetDirectories(timestampDirectory))
                        {
                            // WriteLine($"trainTypeDirectory -> {trainTypeDirectory}");
                            foreach (var warningJpg in Directory.GetFiles(trainTypeDirectory))
                            {
                                // WriteLine($"WarningJpg -> {warningJpg}");
                                string[] parts = warningJpg.Replace("\\", "/").Split("/");
                                string filename = parts[^1];
                                string trainType = parts[^2];
                                string timestamp = parts[^3];
                                string date = parts[^4];

                                string newName = $"{warningLabel}+{date}_{timestamp}_{trainType}+{filename}";
                                // WriteLine($"newName -> {newName}");
                                File.Copy(warningJpg, Path.Join(resultPath, newName));
                            
                            }
                        }
                    }
                }
            }
        }

        
    }
    private void PickWarningLongImages()
    {
        string sourcePath = Path.Join(AbsolutePath, LongImageFolder);
        string stationPath = Path.Join(AbsolutePath, PresentStation);
        string resultPath = Path.Join(AbsolutePath, PresentStation, WarningFolder);
        string manualPath = Path.Join(stationPath, DetectManulFolder);
        if (!Directory.Exists(manualPath))
            Directory.CreateDirectory(manualPath);
        foreach (var jpg in Directory.GetFiles(resultPath))
        {
            string[] parts = jpg.Replace("\\", "/").Split("/")[^1].Split("+");
            string label = parts[0];
            string orientation = parts[1].Split("_")[2].ToLower();
            string timestamp = parts[1].Split("_")[1];
            string date = parts[1].Split("_")[0];
            string filename = parts[^1];
            // a→algorithm; m→not identified
            try
            {
                string targetImage = Path.Join(sourcePath, date, timestamp, orientation, filename);
                File.Copy(targetImage, Path.Join(manualPath, $"客户端_{PresentStation}_{label}_{filename}"), true);
                
                File.Delete(jpg);
            }
            catch (Exception exception)
            {
                WriteLine(exception.ToString());
            }
        }
        
    }
    private string FtpUploadWarning()
    {
        int count = 0;
        var ftp = GetFtpClient();
        string localPath = Path.Join(AbsolutePath, PresentStation, DetectManulFolder);
        string remotePath = FtpDefectionPath;
        foreach (var warningJpg in Directory.GetFiles(localPath))
        {
            string filename = warningJpg.Replace("\\", "/").Split("/")[^1];
            try
            {
                string targetPath = Path.Join(remotePath, filename);
                ftp.UploadFile(warningJpg, Path.Join(remotePath, filename));
                WriteLine($"ftp {warningJpg} -> {targetPath}");
                count++;
                File.Delete(warningJpg);
            }
            catch (Exception ex)
            {
                WriteLine(ex.ToString());
            }
        }

        return count.ToString();
    }
    private string FileName4Train(string filePath) {
        // 1.
        // 替换\\ 到 /
        // D:/FTP2/2025-07-12/18332-07-22-05/sx-cs-z/2025-07-12-07-23-06-393.jpg
        // 2
        // 按 / 分割路径 兼容linux
        string[] parts = filePath.Replace("\\", "/").Split('/');
        string part1, part2, part3; // 2025-07-12
        int hour = int.Parse(parts[3].Split('-')[1]);
        string amOrPm = "AM";
        if (hour >= 13 && hour <= 24)
            amOrPm = "PM";
        part1 = parts[^2].ToUpper();
        part2 = $"{parts[2]}-{amOrPm}";
        part3 = parts[^1];
        return $"{part1}+{part2}+{part3}";
    }
    [RelayCommand]
    private void GatherWarningNUpload(string auto)
    {
        try
        {
            var dateTime = PresentDate;
            if (auto.Equals("true"))
                dateTime = DateTime.Now.AddDays(-1);
            var result = CountLongImages();
            // 收集一次告警
            string warningImageCnt = "";
            WriteLine($"收集告警 - {dateTime}");
            GatherWarning(dateTime);
            PickWarningLongImages();
            warningImageCnt = FtpUploadWarning();
            string filename = $"{PresentStation}.csv";
            result += $",{warningImageCnt}{Environment.NewLine}";
            File.AppendAllText(filename, result, Encoding.UTF8);
        
            var ftp = GetFtpClient();
        
            ftp.UploadFile(filename, Path.Join(FtpRemotePath, filename));
        }
        catch (Exception exception)
        {
            Log.Error(exception.ToString());
        }
    }
}