using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Serilog;

namespace AlgorithmAcceptanceToolAvalonia.Utils;

public static class GuessUtil
{
    private static readonly List<string> BODY = ["JGQ", "ZL", "ZJ", "RG", "HX"];
    private static readonly List<string> LOAD = ["PB", "P", "SL", "YW", "M", "BT", "Z"];

    // 异常类型与关键词的映射
    private static readonly Dictionary<string, List<string>> GuessLabels = new()
    {
        ["JGQ"] = ["紧", "紧固器", "JGQ"],
        ["ZL"] = ["闸链", "闸", "ZL"],
        ["ZJ"] = ["折角", "折", "ZJ"],
        ["RG"] = ["软管", "RG"],
        ["HX"] = ["火星", "火", "HX"],
        ["PB"] = ["布", "PB", "篷布"],
        ["P"] = ["P", "人员"],
        ["SL"] = ["撒", "SL"],
        ["YW"] = ["物", "YW"],
        ["M"] = ["门", "M"],
        ["BT"] = ["灯",  "BT"],
        // ["Z"] = ["主机", "Z"],
        [""] = ["误", "误检"]
    };
    

    // 根据路径猜测标签
    private static string? GuessLabelFromPath(string filePath)
    {

        // 遍历所有异常类型，检查路径是否包含对应的关键词
        foreach (var (label, keywords) in GuessLabels)
        {
            if (keywords.Any(filePath.Contains))
                return label;
        }

        return null; // 未匹配到任何已知异常类型
    }

    public static async Task<string?> TryGetLabel(string jpgPath)
    {
        string? label = null;
        try
        {
            var jsonPath = jpgPath.Replace(".jpg", ".json");
            if (File.Exists(jsonPath))
            {
                await using var stream = File.OpenRead(jsonPath);
                JsonNode? node = await JsonNode.ParseAsync(stream);
                label = node?["shapes"]?[0]?["label"]?.GetValue<string>();
                return label;
            }

            // 如果没有 JSON 标签文件，尝试从路径猜测标签
            label = GuessUtil.GuessLabelFromPath(jpgPath);
            return label;
        }
        catch (Exception ex)
        {
            Log.Error("{ErrorMessage}", ex.Message);
        }

        return label;
    }

    public static string TryGetTaskName(string label)
    {
        if (BODY.Any(label.ToUpper().Contains))
            return nameof(BODY);
        if (LOAD.Any(label.ToUpper().Contains))
            return nameof(LOAD);
        return nameof(BODY);
    }
}