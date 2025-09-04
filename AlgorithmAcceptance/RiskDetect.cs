using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using AlgorithmAcceptanceTool.Managers;
using AlgorithmAcceptanceTool.Models;
using AlgorithmAcceptanceTool.Utils;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Image = System.Drawing.Image;
using Path = System.IO.Path;

namespace AlgorithmAcceptanceTool
{
    public partial class RiskDetect : Form
    {
        private List<string> imgArray = [];
        private int imgIndex = 0;
        private Image currentImage = null;
        private static string ServiceEndPoint = ConfigurationManager.AppSettings["RiskDetectServiceEndPoint"];
        
        private BackgroundWorker worker;
        private List<double> DefectScores { get; set; } = [];
        private string PresentTaskName { get; set; } = "LOAD";
        private (string, string) StationNApi { get; set; } 
        private static string ErrorFolder = "error", ResultFolder = "result", ErrorWithDrawingFolder = "errorWithDrawing";
        public RiskDetect()
        {
            InitializeComponent();
            StationNApi = CheckPresentStation.PresentStationAlgorithmApi();
            if (StationNApi.Item1 is not null)
            {
                ServiceEndPoint = ServiceEndPoint.Replace(CheckPresentStation.LocalApi, StationNApi.Item2);
                append_log($"- 检测到当前站点为{StationNApi.Item1}, 已将算法接口更换为{StationNApi.Item2}{Environment.NewLine}");
                this.txtSourcePath.Text = Path.Join(@"D:\", $"{StationNApi.Item1}", "short_software");
            }
            else
            {
                append_log($"- 算法接口默认为18.23{Environment.NewLine}");
            }
            this.StartPosition = FormStartPosition.CenterScreen;
            comboBox1.SelectedIndex = 0;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Right || keyData == Keys.Down)
            {
                btnNext.PerformClick();
                return true;
            }
            if (keyData == Keys.Left || keyData == Keys.Up)
            {
                btnPre.PerformClick();
                return true;
            }
            if (keyData == Keys.Enter)
            {
                btnMarkError.PerformClick();
                return true;
            }
            if (keyData == Keys.Escape)
            {
                this.Close();
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private async void btnStartAnalysis_Click(object sender, EventArgs e)
        {
            var sourcePath = this.txtSourcePath.Text;
            if (string.IsNullOrEmpty(sourcePath))
            {
                MessageBox.Show("请选择原图路径");
                return;
            }

            if (!Directory.Exists(sourcePath))
            {
                MessageBox.Show("原图路径不存在");
                return;
            }

            initial_analysis();

            analysis(sourcePath);

        }

        private void analysis(string sourcePath)
        {
            append_log($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}: 算法分析开始{Environment.NewLine}");
            var destPath = $"{sourcePath}\\{ResultFolder}";
            var errorPath = $"{sourcePath}\\{ErrorFolder}";
            var errorWithLaeblPath = $"{sourcePath}\\{ErrorWithDrawingFolder}";
            if (Directory.Exists(destPath))
            {
                Directory.Delete(destPath, true);
            }
            if (Directory.Exists(errorPath))
            {
                Directory.Delete(errorPath, true);
            }
            if (Directory.Exists(errorWithLaeblPath))
            {
                Directory.Delete(errorWithLaeblPath, true);
            }
            Directory.CreateDirectory(destPath);
            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_completed);
            worker.DoWork += new DoWorkEventHandler(do_worker);
            worker.ProgressChanged += new ProgressChangedEventHandler(worker_progess_changed);
            worker.RunWorkerAsync(new WorkerParam()
            { DestPath = destPath, SourcePath = sourcePath, ErrorPath = errorPath });
        }

        private void do_worker(object sender, DoWorkEventArgs e)
        {
            if (e.Argument is WorkerParam)
            {
                var param = e.Argument as WorkerParam;
                var imgArrayList = ImgUtils.GetImgCollection(param.SourcePath);
                var totalCount = imgArrayList.Count;
                var counter = 1;
                foreach (var imgPath in imgArrayList)
                {
                    var fileName = Path.GetFileName(imgPath);
                    // append_log($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}: 开始处理文件{fileName}{Environment.NewLine}");
                    worker.ReportProgress((int)(Math.Round(Convert.ToDecimal(counter) / totalCount, 2) * 100));
                    counter++;
                    try
                    {
                        if (PresentTaskName == "LEFT")
                        {
                            analysis_image(param.DestPath, imgPath, fileName, "LOAD");
                            analysis_image(param.DestPath, imgPath, fileName, "LEFT");
                        }
                        else
                        {
                            analysis_image(param.DestPath, imgPath, fileName, PresentTaskName);
                        }
                        
                    }
                    catch (System.Exception ex)
                    {
                        append_log($"- {fileName}解析失败，Error: {ex.Message}{Environment.NewLine}");
                        continue;
                    }
                    // 看看获取到的图片是否正常，不正常的都移到error文件夹
                    // var fileFullName = Path.Combine(param.DestPath, fileName);
                    // try
                    // {
                    //     var tmp = Image.FromFile(fileFullName);
                    // }
                    // catch
                    // {
                    //     move_file(fileFullName, param.ErrorPath);
                    // }
                    // append_log($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}: 文件{fileName}处理完成{Environment.NewLine}");
                }
                e.Result = param;
            }
            else
            {
                append_log($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}: Argument不合法");
            }
        }

        public void worker_progess_changed(object sender, ProgressChangedEventArgs e)
        {
            this.pbProcess.Value = e.ProgressPercentage;
        }

        private void worker_completed(object sender, RunWorkerCompletedEventArgs e)
        {
            var result = e.Result as WorkerParam;
            finish_analysis(result.DestPath, result.ErrorPath);
            append_log($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}: 算法分析结束{Environment.NewLine}");
        }

        delegate void delAppendLog(string log);
        private void append_log(string log)
        {
            // string logWithNewLine = log + Environment.NewLine;
            if (this.txtLogs.InvokeRequired)
            {
                delAppendLog dd = new delAppendLog(append_log);
                this.txtLogs.Invoke(dd, new object[] { log });
            }
            else
            {
                this.txtLogs.AppendText(log);
            }
        }

        private void move_file(string source, string destPath)
        {
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }

            FileInfo file = new FileInfo(source);
            file.MoveTo(Path.Combine(destPath, file.Name));
        }

        private void btnPre_Click(object sender, EventArgs e)
        {
            currentImage.Dispose();
            if (imgIndex != 0)
            {
                switch_image(imgIndex - 1);
            }
            this.btnPre.Enabled = imgIndex != 0;
            this.btnNext.Enabled = imgIndex != imgArray.Count - 1;
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            currentImage.Dispose();
            if (imgIndex != imgArray.Count - 1)
            {
                switch_image(imgIndex + 1);
            }
            this.btnPre.Enabled = imgIndex != 0;
            this.btnNext.Enabled = imgIndex != imgArray.Count - 1;
        }

        private void btnMarkError_Click(object sender, EventArgs e)
        {
            // if ((MessageBox.Show("确定要将图片移入错误结果目录？", "提示", MessageBoxButtons.OKCancel) == DialogResult.OK))
            // {
            var img = imgArray[imgIndex];
            var fileName = Path.GetFileName(img);

            if (!Directory.Exists(this.txtErrorDirectory.Text))
            {
                Directory.CreateDirectory(this.txtErrorDirectory.Text);
            }
            if (!Directory.Exists(Path.Combine(txtSourcePath.Text, ErrorWithDrawingFolder)))
            {
                Directory.CreateDirectory(Path.Combine(txtSourcePath.Text, ErrorWithDrawingFolder));
            }
            FileInfo file = new FileInfo(Path.Combine(this.txtSourcePath.Text, fileName));
            FileInfo fileWithLabel = new FileInfo(Path.Combine(this.txtSourcePath.Text, ResultFolder, fileName));
            if (file.Exists)
            {
                file.CopyTo(Path.Combine(this.txtErrorDirectory.Text, fileName), true);
                fileWithLabel.CopyTo(Path.Combine(txtSourcePath.Text, ErrorWithDrawingFolder, fileName), true);
                append_log($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}: 文件{fileName}已完成标记结果错误{Environment.NewLine}");
            }
            // }
        }

        private void initial_analysis()
        {
            this.pbProcess.Value = 0;
            this.txtLogs.Text = string.Empty;
            this.pbImgBox.Image = null;
            this.btnStartAnalysis.Enabled = false;
            this.btnNext.Enabled = false;
            this.btnPre.Enabled = false;
            this.btnMarkError.Enabled = false;
            this.txtAnalysisResultDirectory.Text = string.Empty;
            this.txtErrorDirectory.Text = string.Empty;

            this.label3.Visible = false;
            this.label7.Visible = false;
            this.label8.Visible = false;
            this.label9.Visible = false;
            this.label10.Visible = false;
            this.label11.Visible = false;
            this.comboBox1.Enabled = false;

            imgArray = new List<string>();
            imgIndex = -1;
            if (currentImage != null) currentImage.Dispose();
        }

        private void finish_analysis(string destPath, string errorPath)
        {
            this.btnStartAnalysis.Enabled = true;
            this.txtAnalysisResultDirectory.Text = destPath;
            this.txtErrorDirectory.Text = errorPath;
            imgArray = ImgUtils.GetImgCollection(destPath);
            this.label7.Text = (imgArray?.Count ?? 0).ToString();
            if (imgArray.Count > 0)
            {
                this.btnMarkError.Enabled = true;
                switch_image(0);
            }

            this.label3.Visible = true;
            this.label7.Visible = true;
            this.label8.Visible = true;
            this.label9.Visible = true;
            this.label10.Visible = true;
            this.label11.Visible = true;

            this.btnPre.Enabled = imgIndex > 0;
            this.btnNext.Enabled = imgIndex != imgArray.Count - 1;
            //TODO 展示识别出来的分数，识别总数
            try
            {
                double min = DefectScores.Min(), max = DefectScores.Max(), mean = DefectScores.Average();
                append_log($"分数最小值={min*100:F}, 分数最大值={max*100:F}, 分数平均值={mean*100:F1}, 检出率={DefectScores.Count}/{imgArray.Count}*100%={1.0 * DefectScores.Count/imgArray.Count*100:F2}%{Environment.NewLine}");
            }
            catch (Exception e)
            {
                append_log($"均未检出 检出数=0{Environment.NewLine}");
            }

            

        }

        private void switch_image(int index)
        {
            try
            {
                currentImage = Image.FromFile(imgArray[index]);
                this.label11.Text = Path.GetFileName(imgArray[index]);
            }
            catch (OutOfMemoryException ex)
            {
                append_log($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}: 获取图片失败，图片路径：{imgArray[index]}{Environment.NewLine}");
                currentImage = Image.FromFile($"{Environment.CurrentDirectory}/error.png");
                this.label11.Text = "error.png";
            }

            this.pbImgBox.Image = currentImage;
            imgIndex = index;
            this.label9.Text = (imgIndex + 1).ToString();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            RemoteManager.Instance.Init();
        }

        private void analysis_image(string destPath, string imgPath, string fileName, string taskName)
        {

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ServiceEndPoint);
            MsMultiPartFormData form = new MsMultiPartFormData();
            FileStream fs = new FileStream(imgPath, FileMode.Open, FileAccess.Read);
            Byte[] bytes = new Byte[fs.Length];
            fs.Read(bytes, 0, (int)fs.Length);
            fs.Close();

            // 添加文件
            form.AddStreamFile("image_file", fileName, bytes);
            form.AddFormField("task_name", taskName);
            form.PrepareFormData();

            request.Method = "POST";
            request.ContentType = "multipart/form-data; boundary=" + form.Boundary;

            Stream dataStream = request.GetRequestStream();
            foreach (var b in form.GetFormData())
            {
                dataStream.WriteByte(b);
            }
            dataStream.Close();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            var streamReader = new StreamReader(response.GetResponseStream());
            var content = streamReader.ReadToEnd();
            WarningDetectiveResponse result = JsonConvert.DeserializeObject<WarningDetectiveResponse>(content);
            var image = SixLabors.ImageSharp.Image.Load<Rgba32>(bytes);
            if (result != null && result.Data.DefectList.Any())
            {
                foreach (var defect in result.Data.DefectList)
                {
                    var x1 = (int)defect.TopLeft.X;
                    var x2 = (int)defect.BottomRight.X;
                    var y1 = (int)defect.TopLeft.Y;
                    var y2 = (int)defect.BottomRight.Y;
                    var rect = new RectangularPolygon(x1, y1, x2 - x1, y2 - y1);
                    var redPen = SixLabors.ImageSharp.Drawing.Processing.Pens.Solid(SixLabors.ImageSharp.Color.Red, 3); // 5px stroke width
                    image.Mutate(x => x.Draw(redPen, rect));
                }
            }

            double defectScore = 0;
            double defectValue = 0;
            string label = "没检测到";
            
            try
            {
                
                foreach (var l in result.Data.DefectList)
                {
                    if (l is null)
                    {
                        continue;
                    }

                    defectScore = l.DefectScore;
                    defectValue = l.DefectValue;
                    label = l.DefectType;
                    continue;
                }

                if (label == "BT")
                {
                    append_log($"- {taskName} -> {fileName} -> 标签={label}, 亮度={defectValue}, 分数={defectScore*100:F2}{Environment.NewLine}");
                    DefectScores.Add(defectScore);
                }
                    
                else if (label == "没检测到")
                {
                    append_log($"- {taskName} -> {fileName} -> {content}{Environment.NewLine}");
                } 
                else {
                    DefectScores.Add(defectScore);
                    append_log($"- {taskName} -> {fileName} -> 标签={label}, 分数={defectScore*100:F2}{Environment.NewLine}");
                }
                
            }
            catch (Exception e)
            {
                // append_log($"{fileName} -> {content}{Environment.NewLine}");
                append_log($"- {fileName} -> {content}{Environment.NewLine}");
            }


            image.Save(Path.Combine(destPath, $"{taskName}_{label}_{fileName}"));

            streamReader.Close();
            response.Close();

        }

        private void btnSelectPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fb = new FolderBrowserDialog();
            fb.RootFolder = Environment.SpecialFolder.Desktop;
            //设置默认根目录是桌面
            fb.Description = "请选择算法分析原图目录:";
            //设置对话框说明
            if (fb.ShowDialog(this) == DialogResult.OK)
            {
                this.txtSourcePath.Text = fb.SelectedPath;
                this.comboBox1.Enabled = true;
            }
        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string taskName = ((ComboBox)sender).SelectedItem.ToString();
            switch (taskName)
            {
                case "车身": PresentTaskName = "LOAD"; break;
                case "走行": PresentTaskName = "BODY"; break;
                case "左侧标志灯": PresentTaskName = "LEFT"; break;

            }
            append_log($"异常检测类型 -> {taskName}{Environment.NewLine}");
        }

        private void txtLogs_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtSourcePath_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
