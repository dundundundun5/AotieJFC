using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlgorithmAcceptance.Logging
{
    public interface ILogger
    {
        bool IsEnabled(LogLevel level);

        void Log(LogLevel level, string message);

        void Log(LogLevel level, string message, Exception exception);
    }
}
