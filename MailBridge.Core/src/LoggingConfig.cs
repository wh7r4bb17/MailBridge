using Serilog.Core;
using Serilog.Events;

namespace MailBridge.Core
    {
    /// <summary>
    /// Provides a shared logging level switch that host applications can wire up
    /// to their Serilog configuration, allowing runtime control of the minimum
    /// log level (e.g. via an EnableLogging setting).
    /// </summary>
    public static class LoggingConfig
        {
        /// <summary>
        /// The shared level switch controlling the minimum log level at runtime.
        /// </summary>
        public static readonly LoggingLevelSwitch LevelSwitch = new(LogEventLevel.Information);
        }
    }