using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Clipboard = System.Windows.Clipboard;
using ComboBox = System.Windows.Controls.ComboBox;
using Path = System.IO.Path;
using TextBox = System.Windows.Controls.TextBox;
namespace SegmentationTestTool;
/// <summary>
/// 切割测试、异常检测大规模筛选漏检测的工具
/// </summary>
public partial class MainWindow : INotifyPropertyChanged
{
    /// <summary>
    /// 图片的根路径
    /// </summary>
    private const string AbsolutePath = @"D:\";
    /// <summary>
    /// 切割长图的源路径
    /// </summary>
    private const string LongImageFolder = "FTP2";
    /// <summary>
    /// 短图的源路径
    /// </summary>
    private const string ShortImageFolder = "Monitor";
    /// <summary>
    /// 车身左汇总的目标路径
    /// </summary>
    private const string GatheredImageFolder = "long";
    /// <summary>
    /// 所有车头汇总的目标路径
    /// </summary>
    private const string OnlyLocomotiveFolder = "locomotive";
    /// <summary>
    /// 所有车尾汇总的目标路径
    /// </summary>
    private const string OnlyRearFolder = "rear";
    /// <summary>
    /// 顿顿头像的文件名
    /// </summary>
    private const string AvatarJpgName = "avatar.jpg";
    /// <summary>
    /// 硬件错误、软件错误的长图归纳文件夹
    /// </summary>
    private const string LongHardware = "long_hardware", LongSoftware = "long_software";
    /// <summary>
    /// 硬件错误、软件错误的短图归纳文件夹
    /// </summary>
    private const string ShortHardware = "short_hardware", ShortSoftware = "short_software";
    /// <summary>
    /// FTP上传远程路径
    /// </summary>
    private const string FtpRemotePath = "/个人文件夹/张灵顿/segment_test";
    /// <summary>
    /// FTP个人头像下载路径
    /// </summary>
    private const string FtpRemote = "/个人文件夹/张灵顿";
    /// <summary>
    /// 头像文件的修改时间的延后分钟数
    /// </summary>
    private const int AvatarTimeGap = 2;
    /// <summary>
    /// 将Console输出重定向到文本框实例
    /// </summary>
    private readonly Console2Textbox _myTextbox;

    
    public bool ImageGlass { get; set; } = false;
    public bool OneTree { get; set; } = false;
    public bool VsCode { get; set; } = false;
    
    /// <summary>
    /// 无操作计时器
    /// </summary>
    private DispatcherTimer? _inactivityTimer;
    /// <summary>
    /// 写死的站点名列表
    /// </summary>
    public List<string> StationList { get; } = [
        "伍明", "凤台", "包庄", "大许", "宁波", "建国", "新塘边", "杨集", "杭州", "枫泾", "泗安", "淮北北", "湾沚", "炮车", "虞城", "西寺坡", "誓节渡","李庄","姚李庙","杨楼","梓树庄","烔炀河","东孝","白龙桥","汤溪","后溪街","牌头"
    ];
    
    private bool _isInitializing = true, _isInitializing2 = true;
    /// <summary>
    /// 当前提取的类型下拉框
    /// </summary>
    public List<string> TrainTypeList { get; } = ["车身左", "走行左"];
    /// <summary>
    /// 当前提取的类型 
    /// </summary>
    public string PresentTrainType { get; set; } = "cs-z";
    /// <summary>
    /// 短图提取数量的选项
    /// </summary>
    public static List<int> GapList { get; set; } = [8, 14, 20, 50, -30];
    public int PresentGap { get; set; } = 8;
    /// <summary>
    /// 当前站点的中文名
    /// </summary>
    public string PresentStation { get; set; } = new string("null");
    /// <summary>
    /// 当前的本地时间
    /// </summary>
    public static DateTime CurrentDate { get; private set; } = DateTime.Now;
    /// <summary>
    /// 昨天
    /// </summary>
    public static DateTime YesterdayDate { get; private set; } = DateTime.Now.AddDays(-1);
    /// <summary>
    /// 前天
    /// </summary>
    private static readonly DateTime Yesyesterday = DateTime.Now.AddDays(-2);
    /// <summary>
    /// 当前选择的日期
    /// </summary>
    private DateTime _presentDate = YesterdayDate;
    /// <summary>
    /// 日期列表用于选择
    /// </summary>
    public DateTime[] DateList { get; set; } = [CurrentDate, YesterdayDate, Yesyesterday];

    public event PropertyChangedEventHandler? PropertyChanged;
    /// <summary>
    /// 主窗口构造函数
    /// </summary>
    public MainWindow() {
        InitializeComponent();
        _myTextbox = new Console2Textbox(myConsole);
        CheckPresentStation();
        CheckInstalledSoftware();
        Console.WriteLine($"当前日期: {CurrentDate:yyyy-MM-dd}");
        Console.WriteLine($"切割测试日期: {YesterdayDate:yyyy-MM-dd}");
        Console.WriteLine($"短图收集：GAP={PresentGap}");
        StartInactivityTimer(1800);
        if (PresentStation != "null")
            DownloadAvatarIfNotExist();
    }

    
    protected virtual void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
    /// <summary>
    /// 如果头像不存在则从个人文件夹下载头像
    /// </summary>
    private async void DownloadAvatarIfNotExist()
    {
        try
        {
            var localAvatarPath = Path.Join(AbsolutePath, PresentStation, AvatarJpgName);
            if (!File.Exists(localAvatarPath))
            {
                await Task.Run((() =>
                {
                    var ftp = Dundun.Ftp();
                    try
                    {
                        ftp.DownloadFile(Path.Join(FtpRemote, AvatarJpgName), localAvatarPath);
                        WriteAsync(_myTextbox.box, $"{AvatarJpgName} 下载成功！");
                    }
                    catch (Exception e)
                    {
                        WriteAsync(_myTextbox.box, e.ToString());
                    }
                }));
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    /// <summary>
    /// 在线程内输出文本到主窗口的文本框
    /// </summary>
    /// <param name="box">TextBox类实例</param>
    /// <param name="text">要输出的文本</param>
    private void WriteAsync(TextBox box, string text) {
        void Write(System.Windows.Controls.TextBox box, string text) {
            box.AppendText(text);
            box.ScrollToEnd();
        }
        Action<TextBox, String> updateAction = new Action<TextBox, string>(Write);
        box.Dispatcher.BeginInvoke(updateAction, box, text);
    }
    /// <summary>
    /// 查询是否存在D:\站点名的文件夹，有则锁定选项
    /// </summary>
    private void CheckPresentStation() {
        for (int i = 0; i < StationList.Count(); i++) {
            if (Directory.Exists(Path.Join(AbsolutePath, StationList[i]))) {
                stations.SelectedIndex = i;
                stations.IsEnabled = false;
                break;
            }
        }
    }

    private void CheckInstalledSoftware()
    {
        string a = Path.Join(AbsolutePath,"ImageGlass_9.3.2.520_x64");
        string b = Path.Join(AbsolutePath, "1tree");
        string c = Path.Join(AbsolutePath, "Microsoft VS Code");
        if (Directory.Exists(a))
        {
            if (Directory.GetFiles(a).Length > 0)
            {
                ImageGlass = true;
                OnPropertyChanged(nameof(ImageGlass));
            }
        }
        if (Directory.Exists(b))
        {
            if (Directory.GetFiles(b).Length > 0)
            {
                OneTree = true;
                OnPropertyChanged(nameof(OneTree));
            }
        }
        if (Directory.Exists(c))
        {
            if (Directory.GetFiles(c).Length > 0)
            {
                VsCode = true;
                OnPropertyChanged(nameof(VsCode));
            }
        }
    }
    
    /// <summary>
    /// 汇总昨日所有车身左的长图到long
    /// </summary>
    private void GatherLongImages() {

        string resultPath = Path.Join(AbsolutePath, PresentStation, GatheredImageFolder);
        if (Directory.Exists(resultPath))
            Directory.Delete(resultPath, true);
        Directory.CreateDirectory(resultPath);

        string yesterdayPath = Path.Join(AbsolutePath, LongImageFolder, $"{_presentDate:yyyy-MM-dd}");
        //string testPath = new string(@"D:\xmind2025");
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
            string avatarSourcePath = Path.Join(AbsolutePath, PresentStation, AvatarJpgName);

            File.Copy(avatarSourcePath, avatarTargetPath, true);
            File.SetLastWriteTime(avatarTargetPath, curDate.AddMilliseconds(AvatarTimeGap));
            WriteAsync(_myTextbox.box, $"{avatarSourcePath} -> {avatarTargetPath} \u2713\n");
        }
        WriteAsync(_myTextbox.box, $"total={total},sx={sx},xx={xx},长图收集完成，存储在 {resultPath}\n");
        //WriteAsync(MyTextbox.box, $"如果站点选择错误，则将{resultPath}的{resultPath.Split('\\')[1]}手动改成实际站点名称\n");
        Process.Start("explorer.exe", resultPath);


    }
    /// <summary>
    /// 汇总所有相机的车头长图到locomotive
    /// </summary>
    private void GatherLongImageOnlyLocomotive() {
        string resultPath = Path.Join(AbsolutePath, PresentStation, OnlyLocomotiveFolder);
        if (Directory.Exists(resultPath))
            Directory.Delete(resultPath, true);
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

                        // WriteAsync(MyTextbox.box, $"{newFileName} -> {resultPath} \u2713\n");
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
            string avatarSourcePath = Path.Join(AbsolutePath, PresentStation, AvatarJpgName);

            File.Copy(avatarSourcePath, avatarTargetPath, true);
            File.SetLastWriteTime(avatarTargetPath, curDate.AddSeconds(AvatarTimeGap));
            WriteAsync(_myTextbox.box, $"{avatarSourcePath} -> {avatarTargetPath} \u2713\n");
      
        }
        WriteAsync(_myTextbox.box, $"total={total}，长图收集完成，存储在 {resultPath}\n");
        // WriteAsync(MyTextbox.box, $"如果站点选择错误，则将{resultPath}的{resultPath.Split('\\')[1]}手动改成实际站点名称\n");
        Process.Start("explorer.exe", resultPath);
    }
    /// <summary>
    /// 选择指定测试日期
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void DatetimeChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing2)
        {
            _isInitializing2 = false;
            return;
        }
        var t = ((ComboBox)sender).SelectedItem;
        _presentDate = (DateTime)t;
        Console.WriteLine($"测试日期 -> {_presentDate:yyyy-MM-dd}");
        GatherRearButton.IsEnabled = true;
        gatherLongButton.IsEnabled = true;
        gatherLocomotiveButton.IsEnabled = true;
        pickShortButton.IsEnabled = false;
        gaps.IsEnabled = false;
    }
    /// <summary>
    /// 通过选项框选择站点
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void StationChanged(object sender, SelectionChangedEventArgs e) {
        var box = (ComboBox)sender;
        string? selected = (string)box.SelectedItem;
        if (selected != null) {
            Console.WriteLine($"选择站点： {selected}");
            PresentStation = selected.ToString();
            gatherLongButton.IsEnabled = true;
        }
    }
    /// <summary>
    /// 根据./long和./locomotive里留下的软件错误的长图找对应短图
    /// </summary>
    /// <returns>csv文件表示当前站点的测试明细</returns>
    private string PickShortImages() {
        string longHardwareImagePath = Path.Join(AbsolutePath, PresentStation, LongHardware);
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
            WriteAsync(_myTextbox.box, $"{longImagePath}不存在，跳过\n");
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
                    WriteAsync(_myTextbox.box, $"{jpg} -> {newJpg}\n");
                }
            }
            WriteAsync(_myTextbox.box, $"删除完毕！\n");
        }
        catch (DirectoryNotFoundException e)
        {
            WriteAsync(_myTextbox.box, $"{locomotiveImagePath}不存在，跳过\n");
        }
        if (Directory.GetFiles(longImagePath).Length == 0) {
            WriteAsync(_myTextbox.box, $"切割全对！\n");
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
                            WriteAsync(_myTextbox.box, $"找到 {parts[3]} 在 {imgs[i]} index={idx}\n");
                            break;
                        }
                    }
                    if (idx == -1) {
                        WriteAsync(_myTextbox.box, $"{jpg}的短图不存在或已过期，已跳过\n");
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
                                WriteAsync(_myTextbox.box, $"copy {imgs[i]} -> {dest}\u2713\n");
                            }
                    }
                    File.Move(jpg, Path.Join(longSoftwareImagePath, jpg.Split("\\")[^1]), true);
                }
            }
            catch (Exception) {
                WriteAsync(_myTextbox.box, $"{jpg}的短图不存在或已过期，已跳过\n");
                continue;
            }
        }
        return res;
    }
    /// <summary>
    /// 生成切割测试明细csv
    /// </summary>
    /// <param name="longImagePath">留下长图的绝对路径</param>
    /// <returns>csv字符串</returns>
    private string GenerateCsv(string longImagePath) {
        string res = "";
        string csv = $"{PresentStation}CS{Dundun.Two(YesterdayDate.Month)}{Dundun.Two(YesterdayDate.Day)}.csv";
        using (StreamWriter writer = new StreamWriter(Path.Join(AbsolutePath, PresentStation, csv), append: false, encoding: System.Text.Encoding.UTF8)) {
            writer.WriteLine("时间,朝向,错误描述,文件名");
            res += $"时间,朝向,错误描述,文件名\n";
            foreach (string jpg in Directory.GetFiles(longImagePath)) {
                string[] parts = jpg.Split("\\")[^1].Split("+");
                string errorDescription = parts[0]; // hxxx or sxx
                string orientation = parts[1][..2]; // SX-CS-Z -> SX
                string[] errorTimes = parts[^1].Split("-")[3..6];
                string errorTime = $"{errorTimes[0]}{errorTimes[1]}{errorTimes[2]}";
                string filename = parts[^1].Split(".")[0];
                writer.WriteLine($"{errorTime},{orientation},{errorDescription},{filename}");
                res += $"{errorTime},{orientation},{errorDescription},{filename}\n";
            }
        }
        return res;
    }
    /// <summary>
    /// 提取长图的按钮
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void GatherLongClick(object sender, RoutedEventArgs e) {
        gatherLongButton.IsEnabled = false;
        TrainTypeComboBox.IsEnabled = false;
        DatetimeComboBox.IsEnabled = false;
        stations.IsEnabled = false;
        await Task.Run(() => {
            // 这里执行长时间运行的操作
            //print();
            try {
                // 可能会抛出异常的代码
                GatherLongImages();
            }
            catch (Exception ex) {
                WriteAsync(_myTextbox.box, $"{ex.ToString()}\n");
            }
        });
        if (PresentTrainType == "cs-z")
        {
            gaps.IsEnabled = true;
            pickShortButton.IsEnabled = true;
        }
        TrainTypeComboBox.IsEnabled = true;
        DatetimeComboBox.IsEnabled = true;
    }
    /// <summary>
    /// 提取车尾长图的按钮
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void GatherRearClick(object sender, RoutedEventArgs e)
    {
        GatherRearButton.IsEnabled = false;
        TrainTypeComboBox.IsEnabled = false;
        DatetimeComboBox.IsEnabled = false;
        stations.IsEnabled = false;
        await Task.Run(() => {
            // 这里执行长时间运行的操作
            //print();
            try
            {
                // 可能会抛出异常的代码
                GatherRearImages();
            }
            catch (Exception ex)
            {
                WriteAsync(_myTextbox.box, $"{ex.ToString()}\n");
                return;
            }
        });
        TrainTypeComboBox.IsEnabled = true;
        DatetimeComboBox.IsEnabled = true;
    }
    /// <summary>
    /// 提取车头或走行的车尾长图到rear
    /// </summary>
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
                    WriteAsync(_myTextbox.box, $"{newFileName} -> {resultPath} \u2713\n");
                }
            }
            total += 1;
        }
        WriteAsync(_myTextbox.box, $"total={total}，车尾长图收集完成，存储在 {resultPath}\n");
        //WriteAsync(MyTextbox.box, $"如果站点选择错误，则将{resultPath}的{resultPath.Split('\\')[1]}手动改成实际站点名称\n");
        Process.Start("explorer.exe", resultPath);


    }
    /// <summary>
    /// 根据留下标记好的长图找对应短图的按钮
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void PickShortClick(object sender, RoutedEventArgs e) {
        pickShortButton.IsEnabled = false;
        ftpUploadButton.IsEnabled = true;
        string res = "";
        await Task.Run(() => {
            // 这里执行长时间运行的操作
            try {
                // 可能会抛出异常的代码
                res = PickShortImages();
            }
            catch (Exception ex) {
                // 显示异常信息
                WriteAsync(_myTextbox.box, ex.ToString());
                return;
            }
        });
        // TODO 复制到剪贴板 Clipboard.SetDataObject(res); 但是Clipboard.SetText(res)会导致卡死
        try
        {
            Clipboard.SetDataObject(res);
        }
        catch (Exception ex) {
            Console.WriteLine($"复制到剪贴板出错:{ex.ToString()}");
        }
        Console.WriteLine($"信息已复制到剪贴板！");
    }
    /// <summary>
    /// 收集车头按钮，包括单行道的所有相机
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void GatherLocomotiveClick(object sender, RoutedEventArgs e)
    {
        gatherLocomotiveButton.IsEnabled = false;
        DatetimeComboBox.IsEnabled = false;
        TrainTypeComboBox.IsEnabled = false;
        await Task.Run(() => {
            // 这里执行长时间运行的操作
            //print();
            try {
                // 可能会抛出异常的代码

                GatherLongImageOnlyLocomotive();
            }
            catch (Exception ex) {
                // 显示异常信息
                WriteAsync(_myTextbox.box, $"{ex.ToString()}\n");
            }
        });
        if (PresentTrainType == "cs-z")
        {
            gaps.IsEnabled = true;
            pickShortButton.IsEnabled = true;
        }
        DatetimeComboBox.IsEnabled = true;
        TrainTypeComboBox.IsEnabled = true;
    }
    /// <summary>
    /// ftp 上传按钮
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void FtpUploadClick(object sender, RoutedEventArgs e) {
        ftpUploadButton.IsEnabled = false;
        pickShortButton.IsEnabled = false;
        await Task.Run(() => {
            // 这里执行长时间运行的操作
            //print();
            try {
                // 可能会抛出异常的代码
                FtpUploadShortSoftwareImage();
            }
            catch (Exception ex) {
                // 显示异常信息
                WriteAsync(_myTextbox.box, $"{ex.ToString()}");
                return;
            }
        });
        StartInactivityTimer(2);
    }
    /// <summary>
    /// ftp上传短图到个人文件夹
    /// </summary>
    private void FtpUploadShortSoftwareImage() {
        SimpleFtpClient ftp;
        string localPath = Path.Join(AbsolutePath, PresentStation, ShortSoftware);
        if (Directory.Exists(Path.Join(localPath, "error")))
            localPath = Path.Join(localPath, "error");//如果有error文件夹说明做过算法
        string testPath = Path.Join(AbsolutePath, PresentStation, AvatarJpgName);
        if (!Directory.Exists(localPath) || Directory.GetFiles(localPath).Length == 0) {
            return;
        }
        
        try {
            ftp = Dundun.Ftp();
            string[] jpgs = Directory.GetFiles(localPath);
            WriteAsync(_myTextbox.box, $"================FTP UPLOADING================\n");
            foreach (var jpg in jpgs) {
                try {
                    string name = jpg.Split("\\")[^1];
                    if (name[^1] != 'g')
                    {
                        WriteAsync(_myTextbox.box, $"{jpg}不是jpg文件，已跳过\n");
                    }
                        
                    ftp.UploadFile(jpg, Path.Join(FtpRemotePath, name));
                    WriteAsync(_myTextbox.box, $"local.{jpg} -> remote.{FtpRemotePath}\n");
                }
                catch (Exception e) {
                    WriteAsync(_myTextbox.box, $"local.{jpg} upload failed\n");
                    continue;
                }
            }
            WriteAsync(_myTextbox.box, $"================FTP DONE================\n");
        }
        catch (Exception ex) {
            WriteAsync(_myTextbox.box, ex.ToString());
            return;
        }
    }
    /// <summary>
    /// 无操作计时器启动
    /// </summary>
    /// <param name="seconds"></param>
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
    /// <summary>
    /// 以当前文件夹为原点，去短图，前面2张，后面x-2张
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void GapChanged(object sender, SelectionChangedEventArgs e)
    {
        var box = (ComboBox)sender;
        int selected = (int)box.SelectedItem;
        if (selected != null)
        {
            Console.WriteLine($"短图收集：GAP -> {selected}");
            PresentGap = selected;
        }
    }
    /// <summary>
    /// 决定拉取图片是走行还是车身，只支持左
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TrainTypeChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing)
        {
            _isInitializing = false;
            return;
        }
        var box = (ComboBox)sender;
        string selected = (string)box.SelectedItem;
        PresentTrainType = selected == "车身左" ? "cs-z" : "zx-z";
        Console.WriteLine($"汇总长图 -> {selected}");
        GatherRearButton.IsEnabled = true;
        gatherLocomotiveButton.IsEnabled = true;
        gatherLongButton.IsEnabled = true;
        pickShortButton.IsEnabled = false;
        gaps.IsEnabled = false;
    }
    /// <summary>
    /// 清理D:\站点下面的无关文件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void CleanClick(object sender, RoutedEventArgs e)
    {
        MessageBoxResult result = MessageBox.Show(
            $"确定要删除目录 {Path.Join(AbsolutePath, PresentStation)} 下的所有无关内容吗？此操作不可恢复！",
            "确认删除",
            MessageBoxButton.OKCancel,
            MessageBoxImage.Warning);
        if (result == MessageBoxResult.OK)
        {
            // CleanButton.IsEnabled = false;
            await Task.Run((() =>
            {
                try
                {
                    foreach (var f in Directory.GetFiles(Path.Join(AbsolutePath, PresentStation)))
                    {
                        if (Path.GetFileName(f) != AvatarJpgName)
                            File.Delete(f);
                    }

                    foreach (var d in Directory.GetDirectories(Path.Join(AbsolutePath, PresentStation)))
                    {
                        if (d.Split("\\")[^1].Contains("long_") || d.Split("\\")[^1].Contains("short_"))
                            continue;
                        Directory.Delete(d, true);
                    }
                    WriteAsync(_myTextbox.box, $"===============垃圾清理完成！===============\n");
                }
                catch
                {
                    WriteAsync(_myTextbox.box, e.ToString()+'\n');
                }
            }));
        }
        
    }
    // 用户有操作时重置计时器
    /// <summary>
    /// 计算两个数的和
    /// </summary>
    /// <param name="a">第一个数</param>
    /// <param name="b">第二个数</param>
   /// <returns>返回两个数的和</returns>
    private void ResetTimerOnActivity(object sender, EventArgs e) {
        _inactivityTimer.Stop();
        _inactivityTimer.Start();
    }
    /// <summary>
    /// 关闭应用程序
    /// </summary>
    private void CloseApplication() {
        _inactivityTimer.Stop();
        Application.Current.Shutdown();
    }

    private void OpenTrainMonitorLogButton_OnClick(object sender, RoutedEventArgs e)
    {
        Process.Start("explorer.exe", Path.Join(AbsolutePath, "deploy", "TrainMonitorService", "App_Data", "logs"));
    }

    private void OpenImageHandlerLogButton_OnClick(object sender, RoutedEventArgs e)
    {
        Process.Start("explorer.exe", Path.Join(AbsolutePath, "deploy", "TrainMonitorService.ImageHandler", "App_Data", "logs"));
    }
}

