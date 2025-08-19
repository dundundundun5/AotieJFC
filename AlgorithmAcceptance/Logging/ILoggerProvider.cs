namespace AlgorithmAcceptanceTool.Logging
{
    public interface ILoggerProvider
    {
        ILogger CreateLogger(string name);
    }
}
