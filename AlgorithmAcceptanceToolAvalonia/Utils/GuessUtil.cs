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
    private static readonly List<string> LOAD = ["PB", "P", "SL", "YW", "M", "BT", "Z", "C"];

    // 异常类型与关键词的映射
    private static readonly Dictionary<string, List<string>> GuessLabels = new()
    {
        ["JGQ"] = ["紧", "JGQ"],
        ["ZL"] = ["闸链", "ZL"],
        ["ZW"] = ["异物", "ZW", "悬挂"],
        ["ZJ"] = ["折角", "ZJ"],
        ["RG"] = ["软管", "RG"],
        ["HX"] = ["火星", "HX"],
        ["PB"] = ["PB", "篷布"],
        ["P"] = ["P", "人员"],
        ["SL"] = ["撒漏", "SL"],
        ["YW"] = ["自燃", "YW"],
        ["M"] = ["车门", "M"],
        ["BT"] = ["灯",  "BT"],
        ["C"] = ["车窗", "车门纵向", "C"],
        ["Z"] = ["主机", "列尾", "Z"],
        [""] = ["误", "误检"]
    };
    

    // 根据路径猜测标签
    private static string? GuessLabelFromPath(string filePath)
    {
        filePath = filePath.Replace("\\", "/");
        filePath = filePath.Replace(".jpg", "").Replace("manual", "");
        List<string> filePathList = filePath.Split("/").ToList();
        filePathList = filePathList[1..];
        // 遍历所有异常类型，检查路径是否包含对应的关键词
        foreach (var (label, keywords) in GuessLabels)
        {
            if (keywords.Any(filePathList.Contains) || keywords.Any(filePathList[^1].ToUpper().Contains))
                return label;
        }

        return ""; // 未匹配到任何已知异常类型
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
                if (label.Equals("PBT"))
                    return "PB";
                return label;
            }

            // 如果没有 JSON 标签文件，尝试从路径猜测标签
            label = GuessLabelFromPath(jpgPath);
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
        return nameof(LOAD);
    }
}