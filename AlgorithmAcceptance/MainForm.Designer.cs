namespace AlgorithmAcceptance
{
    partial class MainForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
			this.pnlMessages = new System.Windows.Forms.Panel();
			this.panel2 = new System.Windows.Forms.Panel();
			this.txtLogs = new System.Windows.Forms.TextBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.label5 = new System.Windows.Forms.Label();
			this.pnlAlgorithmAnalysis = new System.Windows.Forms.Panel();
			this.btnSelectPath = new System.Windows.Forms.Button();
			this.txtErrorDirectory = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.btnMarkError = new System.Windows.Forms.Button();
			this.txtAnalysisResultDirectory = new System.Windows.Forms.TextBox();
			this.btnNext = new System.Windows.Forms.Button();
			this.btnPre = new System.Windows.Forms.Button();
			this.btnStartAnalysis = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.pbProcess = new System.Windows.Forms.ProgressBar();
			this.label4 = new System.Windows.Forms.Label();
			this.txtSourcePath = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.pbImgBox = new System.Windows.Forms.PictureBox();
			this.pnlAnalysisResult = new System.Windows.Forms.Panel();
			this.label3 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.pnlMessages.SuspendLayout();
			this.panel2.SuspendLayout();
			this.panel1.SuspendLayout();
			this.pnlAlgorithmAnalysis.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pbImgBox)).BeginInit();
			this.pnlAnalysisResult.SuspendLayout();
			this.SuspendLayout();
			// 
			// pnlMessages
			// 
			this.pnlMessages.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.pnlMessages.Controls.Add(this.panel2);
			this.pnlMessages.Controls.Add(this.panel1);
			this.pnlMessages.Dock = System.Windows.Forms.DockStyle.Right;
			this.pnlMessages.Location = new System.Drawing.Point(992, 0);
			this.pnlMessages.Margin = new System.Windows.Forms.Padding(10, 11, 10, 11);
			this.pnlMessages.Name = "pnlMessages";
			this.pnlMessages.Padding = new System.Windows.Forms.Padding(3);
			this.pnlMessages.Size = new System.Drawing.Size(263, 816);
			this.pnlMessages.TabIndex = 0;
			// 
			// panel2
			// 
			this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panel2.Controls.Add(this.txtLogs);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel2.Location = new System.Drawing.Point(3, 32);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(253, 777);
			this.panel2.TabIndex = 3;
			// 
			// txtLogs
			// 
			this.txtLogs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtLogs.Location = new System.Drawing.Point(0, 0);
			this.txtLogs.Multiline = true;
			this.txtLogs.Name = "txtLogs";
			this.txtLogs.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtLogs.Size = new System.Drawing.Size(249, 773);
			this.txtLogs.TabIndex = 0;
			// 
			// panel1
			// 
			this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panel1.Controls.Add(this.label5);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel1.Location = new System.Drawing.Point(3, 3);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(253, 29);
			this.panel1.TabIndex = 2;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(3, 5);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(67, 13);
			this.label5.TabIndex = 1;
			this.label5.Text = "实时日志：";
			// 
			// pnlAlgorithmAnalysis
			// 
			this.pnlAlgorithmAnalysis.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.pnlAlgorithmAnalysis.Controls.Add(this.label11);
			this.pnlAlgorithmAnalysis.Controls.Add(this.label10);
			this.pnlAlgorithmAnalysis.Controls.Add(this.label9);
			this.pnlAlgorithmAnalysis.Controls.Add(this.label8);
			this.pnlAlgorithmAnalysis.Controls.Add(this.label7);
			this.pnlAlgorithmAnalysis.Controls.Add(this.label3);
			this.pnlAlgorithmAnalysis.Controls.Add(this.btnSelectPath);
			this.pnlAlgorithmAnalysis.Controls.Add(this.txtErrorDirectory);
			this.pnlAlgorithmAnalysis.Controls.Add(this.label6);
			this.pnlAlgorithmAnalysis.Controls.Add(this.btnMarkError);
			this.pnlAlgorithmAnalysis.Controls.Add(this.txtAnalysisResultDirectory);
			this.pnlAlgorithmAnalysis.Controls.Add(this.btnNext);
			this.pnlAlgorithmAnalysis.Controls.Add(this.btnPre);
			this.pnlAlgorithmAnalysis.Controls.Add(this.btnStartAnalysis);
			this.pnlAlgorithmAnalysis.Controls.Add(this.label2);
			this.pnlAlgorithmAnalysis.Controls.Add(this.pbProcess);
			this.pnlAlgorithmAnalysis.Controls.Add(this.label4);
			this.pnlAlgorithmAnalysis.Controls.Add(this.txtSourcePath);
			this.pnlAlgorithmAnalysis.Controls.Add(this.label1);
			this.pnlAlgorithmAnalysis.Dock = System.Windows.Forms.DockStyle.Top;
			this.pnlAlgorithmAnalysis.Location = new System.Drawing.Point(0, 0);
			this.pnlAlgorithmAnalysis.Name = "pnlAlgorithmAnalysis";
			this.pnlAlgorithmAnalysis.Padding = new System.Windows.Forms.Padding(0, 0, 10, 11);
			this.pnlAlgorithmAnalysis.Size = new System.Drawing.Size(992, 242);
			this.pnlAlgorithmAnalysis.TabIndex = 1;
			// 
			// btnSelectPath
			// 
			this.btnSelectPath.Location = new System.Drawing.Point(478, 25);
			this.btnSelectPath.Name = "btnSelectPath";
			this.btnSelectPath.Size = new System.Drawing.Size(75, 25);
			this.btnSelectPath.TabIndex = 10;
			this.btnSelectPath.Text = "浏览";
			this.btnSelectPath.UseVisualStyleBackColor = true;
			this.btnSelectPath.Click += new System.EventHandler(this.btnSelectPath_Click);
			// 
			// txtErrorDirectory
			// 
			this.txtErrorDirectory.Location = new System.Drawing.Point(112, 125);
			this.txtErrorDirectory.Name = "txtErrorDirectory";
			this.txtErrorDirectory.ReadOnly = true;
			this.txtErrorDirectory.Size = new System.Drawing.Size(600, 20);
			this.txtErrorDirectory.TabIndex = 9;
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(16, 128);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(91, 13);
			this.label6.TabIndex = 8;
			this.label6.Text = "错误结果目录：";
			// 
			// btnMarkError
			// 
			this.btnMarkError.Location = new System.Drawing.Point(858, 122);
			this.btnMarkError.Name = "btnMarkError";
			this.btnMarkError.Size = new System.Drawing.Size(104, 25);
			this.btnMarkError.TabIndex = 5;
			this.btnMarkError.Text = "标记结果错误";
			this.btnMarkError.UseVisualStyleBackColor = true;
			this.btnMarkError.Click += new System.EventHandler(this.btnMarkError_Click);
			// 
			// txtAnalysisResultDirectory
			// 
			this.txtAnalysisResultDirectory.Location = new System.Drawing.Point(113, 168);
			this.txtAnalysisResultDirectory.Name = "txtAnalysisResultDirectory";
			this.txtAnalysisResultDirectory.ReadOnly = true;
			this.txtAnalysisResultDirectory.Size = new System.Drawing.Size(599, 20);
			this.txtAnalysisResultDirectory.TabIndex = 6;
			// 
			// btnNext
			// 
			this.btnNext.Location = new System.Drawing.Point(858, 168);
			this.btnNext.Name = "btnNext";
			this.btnNext.Size = new System.Drawing.Size(104, 25);
			this.btnNext.TabIndex = 4;
			this.btnNext.TabStop = false;
			this.btnNext.Text = "下一张";
			this.btnNext.UseVisualStyleBackColor = true;
			this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
			// 
			// btnPre
			// 
			this.btnPre.Location = new System.Drawing.Point(740, 168);
			this.btnPre.Name = "btnPre";
			this.btnPre.Size = new System.Drawing.Size(94, 25);
			this.btnPre.TabIndex = 3;
			this.btnPre.Text = "上一张";
			this.btnPre.UseVisualStyleBackColor = true;
			this.btnPre.Click += new System.EventHandler(this.btnPre_Click);
			// 
			// btnStartAnalysis
			// 
			this.btnStartAnalysis.Location = new System.Drawing.Point(858, 27);
			this.btnStartAnalysis.Name = "btnStartAnalysis";
			this.btnStartAnalysis.Size = new System.Drawing.Size(104, 76);
			this.btnStartAnalysis.TabIndex = 6;
			this.btnStartAnalysis.Text = "开始分析";
			this.btnStartAnalysis.UseVisualStyleBackColor = true;
			this.btnStartAnalysis.Click += new System.EventHandler(this.btnStartAnalysis_Click);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(41, 85);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(67, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "处理进度：";
			// 
			// pbProcess
			// 
			this.pbProcess.Location = new System.Drawing.Point(113, 76);
			this.pbProcess.Name = "pbProcess";
			this.pbProcess.Size = new System.Drawing.Size(599, 25);
			this.pbProcess.TabIndex = 2;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(17, 172);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(91, 13);
			this.label4.TabIndex = 1;
			this.label4.Text = "分析结果目录：";
			// 
			// txtSourcePath
			// 
			this.txtSourcePath.Location = new System.Drawing.Point(113, 26);
			this.txtSourcePath.Name = "txtSourcePath";
			this.txtSourcePath.ReadOnly = true;
			this.txtSourcePath.Size = new System.Drawing.Size(345, 20);
			this.txtSourcePath.TabIndex = 1;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(41, 33);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(67, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "原图路径：";
			// 
			// pbImgBox
			// 
			this.pbImgBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pbImgBox.Location = new System.Drawing.Point(0, 0);
			this.pbImgBox.Name = "pbImgBox";
			this.pbImgBox.Size = new System.Drawing.Size(988, 570);
			this.pbImgBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pbImgBox.TabIndex = 2;
			this.pbImgBox.TabStop = false;
			// 
			// pnlAnalysisResult
			// 
			this.pnlAnalysisResult.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.pnlAnalysisResult.Controls.Add(this.pbImgBox);
			this.pnlAnalysisResult.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlAnalysisResult.Location = new System.Drawing.Point(0, 242);
			this.pnlAnalysisResult.Name = "pnlAnalysisResult";
			this.pnlAnalysisResult.Size = new System.Drawing.Size(992, 574);
			this.pnlAnalysisResult.TabIndex = 2;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(111, 206);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(79, 13);
			this.label3.TabIndex = 11;
			this.label3.Text = "结果目录中共";
			this.label3.Visible = false;
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.label7.ForeColor = System.Drawing.Color.Red;
			this.label7.Location = new System.Drawing.Point(186, 202);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(39, 20);
			this.label7.TabIndex = 12;
			this.label7.Text = "100";
			this.label7.Visible = false;
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(227, 206);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(127, 13);
			this.label8.TabIndex = 13;
			this.label8.Text = "张图片，当前浏览至第";
			this.label8.Visible = false;
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.label9.ForeColor = System.Drawing.Color.Red;
			this.label9.Location = new System.Drawing.Point(351, 202);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(19, 20);
			this.label9.TabIndex = 14;
			this.label9.Text = "0";
			this.label9.Visible = false;
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(392, 206);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(127, 13);
			this.label10.TabIndex = 15;
			this.label10.Text = "张，当前文件文件名：";
			this.label10.Visible = false;
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.label11.ForeColor = System.Drawing.Color.Red;
			this.label11.Location = new System.Drawing.Point(520, 203);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(63, 20);
			this.label11.TabIndex = 16;
			this.label11.Text = "文件名";
			this.label11.Visible = false;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1255, 816);
			this.Controls.Add(this.pnlAnalysisResult);
			this.Controls.Add(this.pnlAlgorithmAnalysis);
			this.Controls.Add(this.pnlMessages);
			this.Name = "MainForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "算法验收";
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.pnlMessages.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.pnlAlgorithmAnalysis.ResumeLayout(false);
			this.pnlAlgorithmAnalysis.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pbImgBox)).EndInit();
			this.pnlAnalysisResult.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlMessages;
        private System.Windows.Forms.Panel pnlAlgorithmAnalysis;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnStartAnalysis;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ProgressBar pbProcess;
        private System.Windows.Forms.TextBox txtSourcePath;
        private System.Windows.Forms.TextBox txtLogs;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnMarkError;
        private System.Windows.Forms.TextBox txtAnalysisResultDirectory;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.Button btnPre;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.PictureBox pbImgBox;
        private System.Windows.Forms.Panel pnlAnalysisResult;
        private System.Windows.Forms.TextBox txtErrorDirectory;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnSelectPath;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label3;
	}
}

