using AlgorithmAcceptance.Managers;
using AlgorithmAcceptance.Models;
using AlgorithmAcceptance.Utils;
using Newtonsoft.Json;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using Path = System.IO.Path;
using Image = System.Drawing.Image;

namespace AlgorithmAcceptance
{
    public partial class OCR : Form
    {
        private static List<string> imgArray = new List<string>();
        private static int imgIndex = 0;
        private static Image currentImage = null;
        private static string OCRServiceEndPoint = ConfigurationManager.AppSettings["OCRServiceEndPoint"];
        private BackgroundWorker worker;

        public OCR()
        {
			InitializeComponent();
			this.Width = 1350;
			this.Height = 850;
			this.Text = "OCR识别检测验收";
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
            var destPath = $"{sourcePath}\\result";
            var errorPath = $"{sourcePath}\\error";
            if (Directory.Exists(destPath))
            {
                Directory.Delete(destPath, true);
            }
            if (Directory.Exists(errorPath))
            {
                Directory.Delete(errorPath, true);
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
                    append_log($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}: 开始处理文件{fileName}{Environment.NewLine}");
                    worker.ReportProgress((int)(Math.Round(Convert.ToDecimal(counter) / totalCount, 2) * 100));
                    counter++;
                    analysis_image(param.DestPath, imgPath, fileName);
                    // 看看获取到的图片是否正常，不正常的都移到error文件夹
                    var fileFullName = Path.Combine(param.DestPath, fileName);
                    try
                    {
                        var tmp = Image.FromFile(fileFullName);
                    }
                    catch 
                    {
                        move_file(fileFullName, param.ErrorPath);
                    }
                    
                    append_log($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}: 文件{fileName}处理完成{Environment.NewLine}");
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
            if ((MessageBox.Show("确定要将图片移入错误结果目录？", "提示", MessageBoxButtons.OKCancel) == DialogResult.OK))
            {
                var img = imgArray[imgIndex];
                var fileName = Path.GetFileName(img);
                if (!Directory.Exists(this.txtErrorDirectory.Text))
                {
                    Directory.CreateDirectory(this.txtErrorDirectory.Text);
                }
                FileInfo file = new FileInfo(Path.Combine(this.txtSourcePath.Text, fileName));
                if (file.Exists)
                {
                    file.CopyTo(Path.Combine(this.txtErrorDirectory.Text, fileName), true);
                    append_log($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}: 文件{fileName}已完成标记结果错误{Environment.NewLine}");
                }
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

            this.label3.Visible = false;
            this.label7.Visible = false;
            this.label8.Visible = false;
            this.label9.Visible = false;
            this.label10.Visible = false;
            this.label11.Visible = false;

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

		private void analysis_image(string destPath, string imgPath, string fileName)
		{
			try
			{
				HttpWebRequest request = (HttpWebRequest)WebRequest.Create(OCRServiceEndPoint);
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
				if (result != null && result.Data.DefectList.Any())
				{
					SixLabors.Fonts.FontFamily fontFamily = SixLabors.Fonts.SystemFonts.Families.FirstOrDefault();
					SixLabors.Fonts.Font font = fontFamily.CreateFont(100, SixLabors.Fonts.FontStyle.Bold);
                    int count = 1;
					foreach (var defect in result.Data.DefectList.OrderBy(d=> d.TopLeft.X))
					{
						var x1 = (int)defect.TopLeft.X;
						var x2 = (int)defect.BottomRight.X;
						var y1 = (int)defect.TopLeft.Y;
						var y2 = (int)defect.BottomRight.Y;
						var rect = new RectangularPolygon(x1, y1, x2 - x1, y2 - y1);
						var redPen = SixLabors.ImageSharp.Drawing.Processing.Pens.Solid(SixLabors.ImageSharp.Color.Red, 5); // 5px stroke width
						image.Mutate(x => x.Draw(redPen, rect));

						//绘制识别出的矩形框bottomRight的Y坐标
						if (!string.IsNullOrEmpty(defect.DefectContent))
						{
                            float xx = count == 1 ? (float)defect.TopLeft.X - 100 : (float)defect.BottomRight.X + 100;
							image.Mutate(x => x.DrawText(defect.DefectContent, font, SixLabors.ImageSharp.Color.Blue, new SixLabors.ImageSharp.PointF(xx, (float)defect.TopLeft.Y - 150)));
						}
                        count++;
					}
				}

				image.Save(Path.Combine(destPath, fileName));

				streamReader.Close();
				response.Close();
			}
			catch (System.Exception ex)
			{
				append_log($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}: 文件{fileName}解析失败，Error: {ex.Message}{Environment.NewLine}");
			}
		}

		private void btnSelectPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fb = new FolderBrowserDialog();
            fb.RootFolder = Environment.SpecialFolder.Desktop;
            //设置默认根目录是桌面
            fb.Description = "请选择算法分析原图目录:";
            //设置对话框说明
            if (fb.ShowDialog() == DialogResult.OK)
            {
                this.txtSourcePath.Text = fb.SelectedPath;
            }
        }
    }
}
