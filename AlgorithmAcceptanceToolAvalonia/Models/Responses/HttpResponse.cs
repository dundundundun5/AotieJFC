using System;

namespace AlgoritmAcceptanceToolAvalonia.Models;

public class HttpResponse<T>
{
    public int Code { get; set; }
    public T Data { get; set; } 
}