using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using AlgoritmAcceptanceToolAvalonia.Models.Enums;
using AlgoritmAcceptanceToolAvalonia.Models.Responses;
using Avalonia.Media.Imaging;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace AlgoritmAcceptanceToolAvalonia.Utils;

public static class ImageUtil
{
    public static Bitmap LoadFromLocalPath(string localPath)
    {
        using var fileStream = File.OpenRead(localPath);
        return new Bitmap(fileStream);
    }

    public static List<string> GetAllJpgPath(string rootPath)
    {
        return Directory.GetFiles(rootPath, "*.jpg", SearchOption.AllDirectories).ToList();
        
    }

    public static async Task Drawing(FileStream imageSteam, string resultPath,AlgorithmResponse response)
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
        foreach (var defect in response.DefectList)
        {
            var x1 = (int)defect.TopLeft.X;
            var x2 = (int)defect.BottomRight.X;
            var y1 = (int)defect.TopLeft.Y;
            var y2 = (int)defect.BottomRight.Y;
            var rect = new RectangularPolygon(x1, y1, x2 - x1, y2 - y1);
            var redPen = Pens.Solid(SixLabors.ImageSharp.Color.Red, 5); // 5px stroke width
            image.Mutate(x => x.Draw(redPen, rect));
        }
        await image.SaveAsync(resultPath);
    }
    
}