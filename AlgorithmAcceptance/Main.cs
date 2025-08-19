using System;
using System.Windows.Forms;
using AlgorithmAcceptance;

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
}