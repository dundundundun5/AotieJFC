namespace AlgorithmAcceptanceToolAvalonia.Models.Responses;

public class HttpResponse<T>
{
    public int Code { get; set; }
    public T Data { get; set; } 
}