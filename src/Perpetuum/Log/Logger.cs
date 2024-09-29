using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System;
using System.Diagnostics;

namespace Perpetuum.Log
{
    public static class Logger
    {
        public static readonly ILoggerFactory Factory = LoggerFactory.Create(builder =>
            builder.AddConsole(options =>
            {
                options.IncludeScopes = true;
                options.DisableColors = true;
                options.Format = ConsoleLoggerFormat.Systemd;
            }));
        private static readonly ILogger _logger = Factory.CreateLogger("Logger");

        [Conditional("TRACE")]
        public static void Info(string message)
        {
            _logger.LogInformation(message);
        }

        [Conditional("TRACE")]
        public static void Warning(string message)
        {
            _logger.LogWarning(message);
        }

        public static void Error(string message)
        {
            _logger.LogError(message);
        }

        public static void Exception(Exception ex)
        {
            _logger.LogCritical(ex, ex.Message);
        }
    }
}
