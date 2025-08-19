using AlgorithmAcceptanceTool.Managers;
using AlgorithmAcceptanceTool.Models;
using AlgorithmAcceptanceTool.Utils;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using System.Windows.Media.Effects;
using System.Windows.Media.Media3D;
using Image = System.Drawing.Image;
using Path = System.IO.Path;
using Point = SixLabors.ImageSharp.Point;


namespace AlgorithmAcceptanceTool
{
    public partial class OCR : Form
    {
        private List<string> imgArray = new List<string>();
        private int imgIndex = 0;
        private Image currentImage = null;
        private static string ServiceEndPoint = ConfigurationManager.AppSettings["OCRServiceEndPoint"];
        private string ErrorFolder = "error", ResultFolder = "result", ErrorWithDrawingFolder = "errorWithDrawing";
        private string AutoDetectFolder = "autoDetect";
        private BackgroundWorker worker;
        private List<string> errorFiles = [];
        private (string, string) StationNApi { get; set; }

        public OCR()
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
            this.Width = 1550;
            this.Height = 950;
            this.Text = "OCR识别检测算法验收";
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

        private void btnStartAnalysis_Click(object sender, EventArgs e)
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
            anylysis(sourcePath);
        }

        private void anylysis(string sourcePath)
        {
            append_log($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}: 算法分析开始{Environment.NewLine}");
            var destPath = $"{sourcePath}\\{ResultFolder}";
            var errorPath = $"{sourcePath}\\{ErrorFolder}";
            var errorWithLaeblPath = $"{sourcePath}\\{ErrorWithDrawingFolder}";
            var autoDetectPath = $"{sourcePath}\\{AutoDetectFolder}";
            if (Directory.Exists(autoDetectPath))
            {
                Directory.Delete(autoDetectPath, true);
            }
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
                var total = imgArrayList.Count;
                var error = 0;
                var AutoDetectPath = Path.Join(txtSourcePath.Text, AutoDetectFolder);
                if (!Directory.Exists(AutoDetectPath))
                {
                    Directory.CreateDirectory(AutoDetectPath);
                }

                bool flag = false;
                foreach (var imgPath in imgArrayList)
                {
                    var fileName = Path.GetFileName(imgPath);
                    string fileTrainType, fileTrainNumber;
                    try
                    {
                        fileTrainType = fileName.Split(".")[0].Split("_")[0];
                        fileTrainNumber = fileName.Split(".")[0].Split("_")[1];
                    }
                    catch (Exception exception)
                    {
                        fileTrainType = "";
                        fileTrainNumber = "";
                    }

                    string trainType, trainNumber;

                    // append_log($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}: 开始处理文件{fileName}{Environment.NewLine}");
                    worker.ReportProgress((int)(Math.Round(Convert.ToDecimal(counter) / totalCount, 2) * 100));
                    counter++;
                    try
                    {
                        (trainType, trainNumber) = analysis_image(param.DestPath, imgPath, fileName);
                        if (fileTrainType == "" && fileTrainNumber == "")
                        {
                            append_log($"- {fileName} -> {trainType}_{trainNumber}.jpg \u2713{Environment.NewLine}");
                            continue;
                        }
                        else
                        {
                            flag = true;
                        }
                        if (trainType == fileTrainType && trainNumber == fileTrainNumber)
                        {
                            append_log($"- {fileName} -> {trainType}_{trainNumber}.jpg \u2713{Environment.NewLine}");

                        }
                        else
                        {
                            // append_log($"- {fileName} -> {trainType}_{trainNumber}.jpg \u2717{Environment.NewLine}");
                            error++;
                            errorFiles.Add(fileName);
                            File.Copy(imgPath, Path.Join(AutoDetectPath, fileName));
                            append_log($"- {fileName} -> {trainType}_{trainNumber}.jpg \u2717{Environment.NewLine}");
                        }
                    }
                    catch (System.Exception ex)
                    {
                        append_log($"- {fileName}解析失败，Error: {ex.Message} \u2717 {Environment.NewLine}");
                    }
                }
                e.Result = param;
                append_log($"- RFID不等于OCR的样本被自动归类在{AutoDetectPath}{Environment.NewLine}" +
                           $"");
                if (flag)
                    append_log($"- OCR识别准确率 = 100- {error}/{total}*100 = {100 - (1.0 * error / total * 100):F2}%{Environment.NewLine}");
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
            if (this.txtLogs.InvokeRequired)
            {
                delAppendLog dd = new delAppendLog(append_log);
                this.txtLogs.Invoke(dd, new object[] { log });

            }
            else
            {
                this.txtLogs.AppendText(log);
                this.txtLogs.ScrollToCaret();
            }
        }

        private void btnPre_Click(object sender, EventArgs e)
        {
            // currentImage.Dispose();
            if (imgIndex != 0)
            {
                switch_image(imgIndex - 1);
            }
            this.btnPre.Enabled = imgIndex != 0;
            this.btnNext.Enabled = imgIndex != imgArray.Count - 1;
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            // currentImage.Dispose();
            if (imgIndex != imgArray.Count - 1)
            {
                switch_image(imgIndex + 1);
            }
            this.btnPre.Enabled = imgIndex != 0;
            this.btnNext.Enabled = imgIndex != imgArray.Count - 1;
        }

        private void btnMarkError_Click(object sender, EventArgs e)
        {
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

            //this.label3.Visible = false;
            //this.label5.Visible = false;
            //this.label7.Visible = false;
            //this.label8.Visible = false;
            //this.label9.Visible = false;
            //this.label10.Visible = false;
            //this.label11.Visible = false;

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
            this.label5.Visible = true;
            this.label7.Visible = true;
            this.label8.Visible = true;
            this.label9.Visible = true;
            this.label10.Visible = true;
            this.label11.Visible = true;

            this.btnPre.Enabled = imgIndex > 0;
            this.btnNext.Enabled = imgIndex != imgArray.Count - 1;
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

        private (string, string) analysis_image(string destPath, string imgPath, string fileName)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ServiceEndPoint);
                MsMultiPartFormData form = new MsMultiPartFormData();
                FileStream fs = new FileStream(imgPath, FileMode.Open, FileAccess.Read);
                Byte[] bytes = new Byte[fs.Length];
                fs.Read(bytes, 0, (int)fs.Length);
                fs.Close();

                // 添加文件
                form.AddStreamFile("image_file", fileName, bytes);
                form.AddFormField("task_name", "BODY");
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
                OcrDetectiveResponse result = JsonConvert.DeserializeObject<OcrDetectiveResponse>(content);
                var image = SixLabors.ImageSharp.Image.Load<Rgba32>(bytes);
                string trainType = "无", trainNumber = "无";
                SixLabors.ImageSharp.Image<Rgba32> cz, ch;
                (int, int, int, int) czTuple = (0, 0, 0, 0), chTuple = (0, 0, 0, 0);
                if (result != null && result.Data.DefectList.Any())
                {
                    
                    int count = 1;
                    
                    foreach (var defect in result.Data.DefectList.OrderBy(d => d.TopLeft.X))
                    {
                        var x1 = (int)defect.TopLeft.X;
                        var x2 = (int)defect.BottomRight.X;
                        var y1 = (int)defect.TopLeft.Y;
                        var y2 = (int)defect.BottomRight.Y;
                        var rect = new RectangularPolygon(x1, y1, x2 - x1, y2 - y1);
                        var redPen = SixLabors.ImageSharp.Drawing.Processing.Pens.Solid(SixLabors.ImageSharp.Color.Red, 5); // 5px stroke width
                        image.Mutate(x => x.Draw(redPen, rect));

                        

                        // Draw rectangle and text
                        if (!string.IsNullOrEmpty(defect.DefectContent))
                        {
                           
                            try
                            {
                                if (defect.DefectType == "CZ")
                                {
                                    trainType = defect.DefectContent;
                                    czTuple = (x1, y1, x2 - x1, y2 - y1);
                                }
                                    
                            }
                            catch
                            {

                            }
                            try
                            {
                                if (defect.DefectType == "CH")
                                {
                                    trainNumber = defect.DefectContent;
                                    chTuple = (x1, y1, x2 - x1, y2 - y1);
                                }
                                    
                            }
                            catch
                            {

                            }
                        }
                        count++;
                    }

                    int scale = 4;
                    int fontSize = 450; 
                    int yAxisPixelBias = fontSize + 700, xAxisPixelBias = fontSize + 1600;
                    var fontColor = Color.YellowGreen;
                    float brightness = 1.00f;
                    SixLabors.Fonts.FontFamily fontFamily = SixLabors.Fonts.SystemFonts.Families.FirstOrDefault();
                    SixLabors.Fonts.Font font = fontFamily.CreateFont(fontSize, SixLabors.Fonts.FontStyle.Bold);
                    Image<Rgba32> resizedTrainTypeImage = null;
                    Image<Rgba32> resizedTrainNumberImage = null;
                    if (trainType != "无")
                    {
                        int left = czTuple.Item1, top = czTuple.Item2, width = czTuple.Item3, height = czTuple.Item4;
                        var cropRectangle = new Rectangle(left, top, width, height);
                        Image<Rgba32> croppedImage = image.Clone(img => img.Crop(cropRectangle));
                        // 2. 放大
                        resizedTrainTypeImage = croppedImage.Clone(img =>
                        {
                            img.Resize(new ResizeOptions
                            {
                                Size = new Size(croppedImage.Width * scale, croppedImage.Height * scale),
                                Mode = ResizeMode.Stretch // 或 ResizeMode.BoxPad 等，根据需要选择
                            }).Brightness(brightness);
                        });
                    }
                    if (trainNumber != "无")
                    {
                        int left = chTuple.Item1,
                            top = chTuple.Item2,
                            width = chTuple.Item3,
                            height = chTuple.Item4;
                        var cropRectangle = new Rectangle(left, top, width, height);
                        Image<Rgba32> croppedImage = image.Clone(img => img.Crop(cropRectangle));
                        // 2. 放大
                        resizedTrainNumberImage = croppedImage.Clone(img =>
                        {
                            img.Resize(new ResizeOptions
                            {
                                Size = new Size(croppedImage.Width * scale, croppedImage.Height * scale),
                                Mode = ResizeMode.Stretch // 或 ResizeMode.BoxPad 等，根据需要选择
                            }).Brightness(brightness);
                        });
                    }

                    if (resizedTrainTypeImage is not null)
                    {
                        image.Mutate(ctx =>
                        {
                            ctx.DrawImage(
                                resizedTrainTypeImage,
                                new Point(0, 0),
                                1f); // 1f 表示不透明度为100%
                        });
                        image.Mutate(x => x.DrawText($"{trainType}", font, fontColor, new SixLabors.ImageSharp.PointF(0, yAxisPixelBias)));
                    }

                    if (resizedTrainNumberImage is not null)
                    {
                        image.Mutate(ctx =>
                        {
                            ctx.DrawImage(
                                resizedTrainNumberImage,
                                new Point(xAxisPixelBias, 0),
                                1f); // 1f 表示不透明度为100%
                        });
                        image.Mutate(x => x.DrawText($"{trainNumber}", font, fontColor, new SixLabors.ImageSharp.PointF(xAxisPixelBias, yAxisPixelBias)));
                    }
                }
                streamReader.Close();
                response.Close();
                image.Save(Path.Combine(destPath, fileName));
                return (trainType, trainNumber);
            }
            catch (System.Exception ex)
            {
                append_log($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}: 文件{fileName}解析失败，Error: {ex.Message}{Environment.NewLine}");
                return ("无", "无");
            }
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
            }
        }

        private void OCR_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void txtLogs_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
