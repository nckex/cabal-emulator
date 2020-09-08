using Microsoft.Extensions.Logging;
using HPT.Common;

namespace HPT.Logging
{
    public class CustomLoggerConfigurator
    {
        public static CustomLoggerConfigurator Instance => Singleton<CustomLoggerConfigurator>.I;

        private LogLevel _logLevel;
        private ILoggerFactory _loggerFactory;

        public CustomLoggerConfigurator()
        {
            _logLevel = LogLevel.Debug;
        }

        public void SetLogLevel(LogLevel logLevel)
        {
            _logLevel = logLevel;
        }

        public ILoggerFactory BuildLoggerFactory()
        {
            if (_loggerFactory != null)
                return _loggerFactory;

            _loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .SetMinimumLevel(_logLevel)
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddConsole(c => 
                    {
                        c.IncludeScopes = true;
                        c.TimestampFormat = "[HH:mm:ss] ";
                    });
            });

            return _loggerFactory;
        }
    }
}
