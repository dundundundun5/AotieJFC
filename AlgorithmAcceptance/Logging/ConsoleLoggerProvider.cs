using AlgorithmAcceptance.Models;
using System;

namespace AlgorithmAcceptance.Logging
{
    public class ConsoleLoggerProvider : ILoggerProvider
    {
        private static readonly SafeDictionary<string, ILogger> Loggers = new SafeDictionary<string, ILogger>();
        private readonly LogLevel _minimumLevel;

        public ConsoleLoggerProvider(LogLevel minimumLevel = LogLevel.Info)
        {
            if (minimumLevel < LogLevel.Trace || minimumLevel > LogLevel.OpFatal)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumLevel), "minimumLevel must be between Trace and OpFatal");
            }

            _minimumLevel = minimumLevel;
        }

        public ILogger CreateLogger(string name) => Loggers.GetOrAdd(name, _ => new ConsoleLogger(name, _minimumLevel));
    }
}
