using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using AlgorithmAcceptanceToolAvalonia.Models;
using AlgorithmAcceptanceToolAvalonia.Models.Responses;
using AlgorithmAcceptanceToolAvalonia.Converters;
using Flurl.Http;
using Flurl.Http.Content;

namespace AlgorithmAcceptanceToolAvalonia.Utils;

public static class RequestUtil
{
    public static async Task<HttpResponse<AlgorithmResponse>> GetDefectiveLabel(string url ,FileStream imageStream, string fileName ,string taskName)
    {
        // TODO: 检查这个stream的作用域是否存在bug
        var response = await url.PostMultipartAsync(mp =>
        {
            
            mp.AddFile("image_file", imageStream, fileName);
            mp.AddString("task_name", taskName);
            
        }).ReceiveJson<HttpResponse<AlgorithmResponse>>();
        return response;
    }
}