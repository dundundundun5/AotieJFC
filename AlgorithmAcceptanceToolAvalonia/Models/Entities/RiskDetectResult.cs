using System;

namespace AlgoritmAcceptanceToolAvalonia.Models;

public class RiskDetectResult
{
    public string Timestamp { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string TrueLabel { get; set; } = string.Empty;
    public string PredictLabel { get; set; } = string.Empty;
    public string PredictScore { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RawResult { get; set; } = string.Empty;

    public RiskDetectResult()
    {
        
    }
    
}