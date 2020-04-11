using System;
using System.Collections.Generic;
using AlgorithmAcceptance.Logging;

namespace AlgorithmAcceptance.Managers
{
    public class LogManager
    {
        private static List<ILoggerProvider> _providers = new List<ILoggerProvider>();
        private static readonly Dictionary<string, Logger> _loggers = new Dictionary<string, Logger>(StringComparer.Ordinal);
        public static void AddLoggerProvider(ILoggerProvider provider)
        {
            _providers.Add(provider);
        }

        public static ILogger CreateLogger(string name)
        {
            Logger logger;
            if (!_loggers.TryGetValue(name, out logger))
            {
                logger = new Logger()
                {
                    Loggers = CreateLoggers(name)
                };
                _loggers.Add(name, logger);
            }
            return logger;
        }
        public static ILogger CreateLogger(Type type) => CreateLogger(type.FullName);
        public static ILogger CreateLogger<T>() => CreateLogger(typeof(T).FullName);

        private static List<ILogger> CreateLoggers(string name)
        {
            var loggers = new List<ILogger>();
            foreach (var item in _providers)
            {
                loggers.Add(item.CreateLogger(name));
            }
            return loggers;
        }
    }
}
