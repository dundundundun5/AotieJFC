using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using AlgoritmAcceptanceToolAvalonia.Converters;
using AlgoritmAcceptanceToolAvalonia.Models;
using AlgoritmAcceptanceToolAvalonia.Models.Responses;
using Flurl.Http;
using Flurl.Http.Content;

namespace AlgoritmAcceptanceToolAvalonia.Utils;

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