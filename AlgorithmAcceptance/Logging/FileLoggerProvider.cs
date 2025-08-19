using AlgorithmAcceptanceTool.Models;
using System;

namespace AlgorithmAcceptanceTool.Logging
{

    public class FileLoggerProvider : ILoggerProvider
    {
        private static readonly SafeDictionary<string, ILogger> Loggers = new SafeDictionary<string, ILogger>();
        private readonly LogLevel _minimumLevel;

        public FileLoggerProvider(LogLevel minimumLevel = LogLevel.Info)
        {
            if (minimumLevel < LogLevel.Trace || minimumLevel > LogLevel.OpFatal)
                throw new ArgumentOutOfRangeException(nameof(minimumLevel), "minimumLevel must be between Trace and OpFatal");

            _minimumLevel = minimumLevel;
        }

        public ILogger CreateLogger(string name)
        {
            var loggers = Loggers.GetOrAdd(name, _ => new FileLogger(name, _minimumLevel));
            return loggers;
        }
    }
}
