using System;

namespace AlgorithmAcceptance.Logging
{
    public class OpFeedbackLoggerProvider : ILoggerProvider
    {
        /// <summary>
        /// 一个日志控件只提供一个Logger
        /// </summary>
        private readonly ILogger _logger;

        public OpFeedbackLoggerProvider(System.Windows.Forms.ListView listView,LogLevel minimumLevel = LogLevel.Info)
        {
            if (minimumLevel < LogLevel.Trace || minimumLevel > LogLevel.OpFatal)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumLevel), "minimumLevel must be between Trace and OpFatal");
            }
            _logger = new OpFeedbackLogger(listView, minimumLevel);
        }
        public ILogger CreateLogger(string name)
        {
            return _logger;
        }
    }
}
