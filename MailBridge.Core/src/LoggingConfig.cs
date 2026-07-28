using NLog;
using NLog.Config;

namespace MailBridge.Core
    {
    /// <summary>
    /// Provides a shared helper to control the runtime minimum log level for
    /// all configured NLog targets, allowing dynamic control (e.g. via an
    /// EnableLogging setting) without reloading the whole configuration file.
    /// </summary>
    public static class LoggingConfig
        {
        #region Properties

        /// <summary>
        /// The currently active minimum log level.
        /// </summary>
        public static LogLevel MinLevel { get; private set; } = LogLevel.Info;

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Updates the minimum log level for all configured logging rules
        /// and reconfigures existing loggers to apply the change immediately.
        /// </summary>
        /// <param name="level">The new minimum log level.</param>
        public static void SetMinLevel(LogLevel level)
            {
            MinLevel = level;

            LoggingConfiguration? config = LogManager.Configuration;
            if (config is null)
                { return; }

            foreach (LoggingRule rule in config.LoggingRules)
                { rule.SetLoggingLevels(level, LogLevel.Fatal); }

            LogManager.ReconfigExistingLoggers();
            }

        #endregion Public Methods
        }
    }