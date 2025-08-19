namespace AlgorithmAcceptanceTool;

partial class Main
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
        button1 = new System.Windows.Forms.Button();
        button2 = new System.Windows.Forms.Button();
        button3 = new System.Windows.Forms.Button();
        SuspendLayout();
        // 
        // button1
        // 
        button1.Font = new System.Drawing.Font("Microsoft YaHei UI", 16F);
        button1.Location = new System.Drawing.Point(21, 3);
        button1.Name = "button1";
        button1.Size = new System.Drawing.Size(250, 80);
        button1.TabIndex = 0;
        button1.Text = "车身识别检测算法验收";
        button1.UseVisualStyleBackColor = true;
        button1.Click += button1_Click;
        // 
        // button2
        // 
        button2.Font = new System.Drawing.Font("Microsoft YaHei UI", 16F);
        button2.Location = new System.Drawing.Point(21, 89);
        button2.Name = "button2";
        button2.Size = new System.Drawing.Size(250, 80);
        button2.TabIndex = 1;
        button2.Text = "异常检测算法验收";
        button2.UseVisualStyleBackColor = true;
        button2.Click += button2_Click;
        // 
        // button3
        // 
        button3.Font = new System.Drawing.Font("Microsoft YaHei UI", 16F);
        button3.Location = new System.Drawing.Point(21, 175);
        button3.Name = "button3";
        button3.Size = new System.Drawing.Size(250, 80);
        button3.TabIndex = 2;
        button3.Text = "车号识别检测算法验收";
        button3.UseVisualStyleBackColor = true;
        button3.Click += button3_Click;
        // 
        // Main
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(297, 257);
        Controls.Add(button3);
        Controls.Add(button2);
        Controls.Add(button1);
        Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
        Name = "Main";
        Text = "算法验收三合一";
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.Button button2;
    private System.Windows.Forms.Button button3;
}