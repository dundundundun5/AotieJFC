using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using AlgorithmAcceptanceToolAvalonia.Models.Responses;
using AlgorithmAcceptanceToolAvalonia.Models.Enums;
using Avalonia.Media.Imaging;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Path = System.IO.Path;
using Point = SixLabors.ImageSharp.Point;

namespace AlgorithmAcceptanceToolAvalonia.Utils;

public static class ImageUtil
{
    public static Bitmap LoadFromLocalPath(string localPath)
    {
        using var fileStream = File.OpenRead(localPath);
        return new Bitmap(fileStream);
    }
    // todo: 文件名筛选
    // todo: 进度挪上去
    public static List<string> GetAllJpgPath(string rootPath, string filterText, string filterOption)
    {
        var paths = Directory
            .GetFiles(rootPath, "*.jpg", SearchOption.TopDirectoryOnly)
            .ToList();
        var filterTexts = filterText.Split(" ");
        foreach (var text in filterTexts)
        {
            if (!string.IsNullOrEmpty(text))
            {
                if (filterOption.Equals("包含"))
                    paths =  paths
                        .Where(p => p.Contains(text))
                        .ToList();
                if (filterOption.Equals("不包含"))
                    paths =  paths
                        .Where(p => !p.Contains(text))
                        .ToList();

            }   
        }
        
            
        return paths;


    }

    private static string TryGetTimestamp(string fileName)
    {
        
        var idx = fileName.IndexOf("20", StringComparison.Ordinal);
        if (idx >= 0)
        {
            var l = "2025-xx-xx-xx-xx-xx-xxx".Length;
            return fileName.Substring(idx, l);
        }
        
        return DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff");
    }
    public static async Task Drawing(FileStream imageSteam, string resultJpgPath,AlgorithmResponse response, bool cropImage, string? cropPath=null, bool classify = false, bool addBrightness = false, float brightness = 1.0f, bool drawLabel = false, int drawLabelScale = 1)
    {
        // 将 FileStream 转换为字节数组
        byte[] bytes;
        using (var memoryStream = new MemoryStream())
        {
            // 确保流在起始位置
            imageSteam.Position = 0;
            await imageSteam.CopyToAsync(memoryStream);
            bytes = memoryStream.ToArray();
        }

        // 使用字节数组加载图像，指定像素格式
        var image = Image.Load<Rgba32>(bytes);
        var rawImage = image.Clone();
        bool flag = false;
        int startX = 0;
        foreach (var defect in response.DefectList)
        {
            var x1 = (int)defect.TopLeft.X;
            var x2 = (int)defect.BottomRight.X;
            var y1 = (int)defect.TopLeft.Y;
            var y2 = (int)defect.BottomRight.Y;
            var rectPoly = new RectangularPolygon(x1, y1, x2 - x1, y2 - y1);
            var rect = new Rectangle(x1, y1, x2 - x1, y2 - y1);
            var l = defect.DefectType;
            var s = defect.DefectScore;
            var a = defect.DefectArea;
            var v = defect.DefectValue;
            var fileName = Path.GetFileName(resultJpgPath);
            flag = true;
            if (cropImage)
            {
                var newImage = image.Clone();
                newImage.Mutate(img => img.Crop(new Rectangle(x1, y1, x2-x1, y2-y1)));
                var timestamp = TryGetTimestamp(fileName);
                var newCropName = $"{timestamp}-{l.ToUpper()}-{x1}-{y1}-{x2}-{y2}-{s}-{a}-{v}.jpg";
                var labelPath = Path.Join(cropPath, l.ToUpper());
                if (classify)
                {
                    labelPath = Path.Join(labelPath, fileName.Split("_")[1], "误检");
                }
                if (!Directory.Exists(labelPath))
                    Directory.CreateDirectory(labelPath);
                var targetPath = Path.Join(labelPath, newCropName);
                await newImage.SaveAsync(targetPath);
            }
            
            
            var redPen = Pens.Solid(SixLabors.ImageSharp.Color.Red, 4); // 5px stroke width
            image.Mutate(x => x.Draw(redPen, rectPoly));
            if (drawLabel)
            {
                Image<Rgba32> croppedLabelImage = image.Clone(x => x.Crop(rect));
                Image<Rgba32> labelScaled = croppedLabelImage.Clone(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(croppedLabelImage.Width * drawLabelScale, croppedLabelImage.Height * drawLabelScale),
                    Mode = ResizeMode.Stretch // 或 ResizeMode.BoxPad 等，根据需要选择
                }));
                image.Mutate(ctx =>
                {
                    ctx.DrawImage(
                        labelScaled,
                        new Point(startX, 0),
                        1f); // 1f 表示不透明度为100%
                });
                startX += labelScaled.Width + 10;
                
            }
            


        }
        if (addBrightness)
            image.Mutate(x => x.Brightness(brightness));
        await image.SaveAsync(resultJpgPath);
        // 如果一张图没有任何标签，才是nolabel
        if (!flag)
        {
            await rawImage.SaveAsync(resultJpgPath.Replace(nameof(EnumFolder.Result).ToLower(),
                nameof(EnumFolder.NoLabel).ToLower()));
        }
        else
        {
            await rawImage.SaveAsync(resultJpgPath.Replace(nameof(EnumFolder.Result).ToLower(),
                nameof(EnumFolder.HasLabel).ToLower()));
        }
    }
    
}