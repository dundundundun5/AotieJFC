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

namespace AlgorithmAcceptanceToolAvalonia.Utils;

public static class ImageUtil
{
    public static Bitmap LoadFromLocalPath(string localPath)
    {
        using var fileStream = File.OpenRead(localPath);
        return new Bitmap(fileStream);
    }

    public static List<string> GetAllJpgPath(string rootPath)
    {
        return Directory.GetFiles(rootPath, "*.jpg", SearchOption.TopDirectoryOnly).ToList();
        
    }

    public static string TryGetTimestamp(string fileName)
    {
        
        var idx = fileName.IndexOf("20", StringComparison.Ordinal);
        if (idx >= 0)
        {
            var l = "2025-xx-xx-xx-xx-xx-xxx".Length;
            return fileName.Substring(idx, l);
        }
        
        return DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff");
    }
    public static async Task Drawing(FileStream imageSteam, string resultJpgPath,AlgorithmResponse response, bool cropImage, string? cropPath=null, bool classify = false)
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
        foreach (var defect in response.DefectList)
        {
            var x1 = (int)defect.TopLeft.X;
            var x2 = (int)defect.BottomRight.X;
            var y1 = (int)defect.TopLeft.Y;
            var y2 = (int)defect.BottomRight.Y;
            var rect = new RectangularPolygon(x1, y1, x2 - x1, y2 - y1);
            var l = defect.DefectType;
            var s = defect.DefectScore;
            var a = defect.DefectArea;
            var v = defect.DefectValue;
            var fileName = Path.GetFileName(resultJpgPath);
            if (l.Equals("Z"))
            {
                flag = true;
            }
            if (cropImage)
            {
                if (l.Equals("Z") || l.Equals("BT"))
                    continue;
                var newImage = image.Clone();
                newImage.Mutate(img => img.Crop(new Rectangle(x1, y1, x2-x1, y2-y1)));
                var timestamp = TryGetTimestamp(fileName);
                var newCropName = $"{timestamp}-{l.ToUpper()}-{x1}-{y1}-{x2}-{y2}-{s}-{a}-{v}.jpg";
                if (classify)
                {
                    cropPath = Path.Join(cropPath, fileName.Split("_")[1], "误检");
                }
                cropPath = Path.Join(cropPath, l.ToUpper());
                if (!Directory.Exists(cropPath))
                    Directory.CreateDirectory(cropPath);
                var targetPath = Path.Join(cropPath, newCropName);
                Console.WriteLine($" -> {targetPath}");
                await newImage.SaveAsync(targetPath);
                break;
            }
            
            
            var redPen = Pens.Solid(SixLabors.ImageSharp.Color.Red, 4); // 5px stroke width
            image.Mutate(x => x.Draw(redPen, rect));
            
            
        }
        image.Mutate(x => x.Brightness(1.5f));
        await image.SaveAsync(resultJpgPath);
        if (!flag)
        {
            await rawImage.SaveAsync(resultJpgPath.Replace(nameof(EnumFolder.Result).ToLower(),
                nameof(EnumFolder.NoLabel).ToLower()));
        }
    }
    
}