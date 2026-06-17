namespace AlgorithmAcceptanceToolAvalonia.Models.Entities;

public class RiskDetectResult
{
    public string Timestamp { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string GuessedLabel { get; set; } = string.Empty;
    public string PredictLabel { get; set; } = string.Empty;
    public string PredictScore { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TaskName { get; set; } = string.Empty;
    public string RawResult { get; set; } = string.Empty;
    public string ResultJpgPath { get; set; } = string.Empty;
    public string Shape { get; set; } = string.Empty;

    public RiskDetectResult()
    {
        
    }
    
}