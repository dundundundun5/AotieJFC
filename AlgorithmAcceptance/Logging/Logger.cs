using System;
using System.Collections.Generic;

namespace AlgorithmAcceptanceTool.Logging
{
    class Logger : ILogger
    {
        public List<ILogger> Loggers = new List<ILogger>();

        public bool IsEnabled(LogLevel level) => true;

        public void Log(LogLevel level, string message)
        {
            foreach (var item in Loggers)
            {
                if (!item.IsEnabled(level))
                    continue;
                try
                {
                    item.Log(level, message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception：{DateTime.Now:HH:mm:ss} [{level}] [{ex}]");
                }
            }
        }

        public void Log(LogLevel level, string message, Exception exception)
        {
            if (!IsEnabled(level))
                return;
            foreach (var item in Loggers)
            {
                if (!item.IsEnabled(level))
                    continue;
                try
                {
                    item.Log(level, message, exception);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception：{DateTime.Now:HH:mm:ss} [{level}] [{exception}] [{ex}]");
                }
            }
        }
    }
}
