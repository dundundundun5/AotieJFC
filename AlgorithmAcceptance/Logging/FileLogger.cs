using AlgorithmAcceptance.Utils;
using log4net;
using System;

namespace AlgorithmAcceptance.Logging
{
    class FileLogger : ILogger
    {
        private readonly string _name;
        private readonly LogLevel _minimumLevel;
        private readonly ILog _log;

        public FileLogger(string name, LogLevel minimumLevel)
        {
            _name = name;
            _minimumLevel = minimumLevel;
            _log = LogManager.GetLogger(name);
        }

        public bool IsEnabled(LogLevel level) => level >= _minimumLevel;

        public void Log(LogLevel level, string message)
        {
            if (!IsEnabled(level))
            {
                return;
            }
            _log.Info($"{DateTime.Now:HH:mm:ss} [{level}] {message}");
        }

        public void Log(LogLevel level, string message, Exception exception)
        {
            if (!IsEnabled(level))
            {
                return;
            }
            _log.Info($"{DateTime.Now:HH:mm:ss} [{level}] {message} - {exception.GetDetailMessage()}");
        }
    }
}
