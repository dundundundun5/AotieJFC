namespace AlgorithmAcceptance.Logging
{
    public interface ILoggerProvider
    {
        ILogger CreateLogger(string name);
    }
}
