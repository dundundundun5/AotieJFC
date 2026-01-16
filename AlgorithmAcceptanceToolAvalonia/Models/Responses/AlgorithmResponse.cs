using System.Collections.Generic;

namespace AlgorithmAcceptanceToolAvalonia.Models.Responses;

public class AlgorithmResponse
{
    public int ImageWidth { get; set; }
    public int ImageHeight { get; set; }
    public List<DefectResponse> DefectList { get; set; } = new List<DefectResponse>();
}


public class DefectResponse
{
    // 共通
    public string DefectType { get; set; } = string.Empty;
    public Point TopLeft { get; set; } = new Point();
    public Point BottomRight { get; set; } = new Point();
    // 仅异常检测
    public int DefectArea { get; set; }
    public int DefectValue { get; set; }
    public double DefectScore { get; set; }
    // 仅OCR
    public string DefectContent { get; set; } = string.Empty;
    public string DefectKind { get; set; } = string.Empty;
    // 仅切割
    public double CenterX { get; set; }
}

public class Point
{
    public double X { get; set; } = 0d;
    public double Y { get; set; } = 0d;
}

