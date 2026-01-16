using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using AlgorithmAcceptanceToolAvalonia.Models;
using AlgorithmAcceptanceToolAvalonia.Models.Entities;
using AlgorithmAcceptanceToolAvalonia.Models.Enums;
using AlgorithmAcceptanceToolAvalonia.Models.Responses;
using Avalonia.Media;

namespace AlgorithmAcceptanceToolAvalonia.Converters;

public class ResponseConverter
{
    
    public static RiskDetectResult FromResponse(HttpResponse<AlgorithmResponse>? httpResponse, string filePath, string? guessedLabel, string taskName)
    {
        
        string temp;
        if (guessedLabel == null)
            temp = nameof(EnumLabelStatus.无标注文件);
        else if (guessedLabel == string.Empty)
            temp = nameof(EnumLabelStatus.无目标);
        else
            temp = guessedLabel;
        
        var fileName = Path.GetFileName(filePath);
        
        
        if (httpResponse == null)
        {
            return  new RiskDetectResult()
            {
                TaskName = taskName,
                RawResult = ToJson(httpResponse),
                FileName = fileName,
                PredictLabel = "",
                GuessedLabel = temp,
                PredictScore = "",
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
        }
        var algorithmResponse = httpResponse.Data; 
        
        var predictLabelList = algorithmResponse.DefectList.Select(a => a.DefectType).ToList();
        var predictScoreList = algorithmResponse.DefectList.Select(a => a.DefectScore).ToList();
        
        RiskDetectResult result = new()
        {
            TaskName = taskName,
            RawResult = ToJson(httpResponse),
            FileName = fileName,
            PredictLabel = string.Join("-", predictLabelList),
            GuessedLabel = temp,
            PredictScore = string.Join("-", predictScoreList),
            Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };
        return result;
    }

    public static string GetPredictLabel(AlgorithmResponse response)
    {
        var predictLabelList = response.DefectList.Select(a => a.DefectType).ToList();
        return string.Join("-", predictLabelList);
    }

    public static string ToJson<T>(T data)
    {
        
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            // 可选：配置序列化选项
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        return JsonSerializer.Serialize(data, options);

    }
}