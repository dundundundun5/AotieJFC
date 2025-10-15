using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using AlgorithmAcceptance;
using SixLabors.ImageSharp.Drawing;
using Path = System.IO.Path;

namespace AlgorithmAcceptanceTool;
public partial class Main : Form
{
    private Timer inactivityTimer;
    private const int InactivityTimeout = 120 * 60 * 1000; // 30 minutes in milliseconds

    public Main()
    {
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        InitializeComponent();

        // Initialize and configure the inactivity timer
        inactivityTimer = new Timer();
        inactivityTimer.Interval = InactivityTimeout;
        inactivityTimer.Tick += InactivityTimer_Tick;
        inactivityTimer.Start();

        // Track mouse and keyboard activity
        this.MouseMove += ResetInactivityTimer;
        this.KeyPress += ResetInactivityTimer;
    }

    private void ResetInactivityTimer(object sender, EventArgs e)
    {
        // Reset the timer on any activity
        inactivityTimer.Stop();
        inactivityTimer.Start();
    }

    private void InactivityTimer_Tick(object sender, EventArgs e)
    {
        // Close the application after inactivity timeout
        inactivityTimer.Stop();
        this.Close();
    }

    private void button1_Click(object sender, EventArgs e)
    {
        OpenChildForm<Segment>();
    }

    private void button2_Click(object sender, EventArgs e)
    {
        OpenChildForm<RiskDetect>();
    }

    private void button3_Click(object sender, EventArgs e)
    {
        OpenChildForm<OCR>();
    }

    private void OpenChildForm<T>() where T : Form, new()
    {
        // 创建新窗口
        T form = new T();
        form.Show(this);
    }

    private void Main_Load(object sender, EventArgs e)
    {
        throw new System.NotImplementedException();
    }

    private async void button4_Click(object sender, EventArgs e)
    {
        FolderBrowserDialog fb = new FolderBrowserDialog();
        fb.RootFolder = Environment.SpecialFolder.Desktop;
        //设置默认根目录是桌面
        fb.Description = "请选择批量修改文件名的目录";
        //设置对话框说明
        if (fb.ShowDialog(this) == DialogResult.OK)
        {
            await Task.Run((() =>
            {
                string path = fb.SelectedPath;
                string resultPath = Path.Join(path, "文件名仅日期");
                if (!Directory.Exists(resultPath))
                    Directory.CreateDirectory(resultPath);
                foreach (var oldFile in Directory.GetFiles(path))
                {
                    string oldName = Path.GetFileName(oldFile);
                    string newName = oldName.Split("_")[^1];
                    File.Copy(oldFile, Path.Join(resultPath, newName), true);
                }

                Process.Start("explorer.exe", resultPath);
            }));
            
        }
    }
}