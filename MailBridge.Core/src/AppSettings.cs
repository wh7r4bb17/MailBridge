using NLog;
using System.Text.Json;

namespace MailBridge.Core
    {
    /// <summary>
    /// Persisted application settings for the mail client, stored as JSON in the
    /// current user's local application data folder. Contains no secrets such as
    /// passwords; those must be supplied at runtime.
    /// </summary>
    public class AppSettings
        {
        #region Fields

        private static readonly object _fileLock = new();

        private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
            {
            WriteIndented = true
            };

        private static readonly string _localAppDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        private static readonly string _settingsDirectory = Path.Combine(_localAppDataDirectory, "MailBridge");
        private static readonly string _settingsFilePath = Path.Combine(_settingsDirectory, "settings.json");
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion Fields

        #region Properties

        /// <summary>
        /// The connection timeout, in seconds, used when sending an email.
        /// </summary>
        public int ConnectionTimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// The last used sender email address.
        /// </summary>
        public string EmailFrom { get; set; } = string.Empty;

        /// <summary>
        /// The last used sender display name, shown alongside the from address.
        /// </summary>
        public string EmailFromName { get; set; } = string.Empty;

        /// <summary>
        /// The last used recipient email address.
        /// </summary>
        public string EmailTo { get; set; } = string.Empty;

        /// <summary>
        /// Whether logging is enabled.
        /// </summary>
        public bool EnableLogging { get; set; } = true;

        /// <summary>
        /// The last used SMTP port.
        /// </summary>
        public int Port { get; set; } = 587;

        /// <summary>
        /// The last used SMTP server host.
        /// </summary>
        public string SmtpServer { get; set; } = string.Empty;

        /// <summary>
        /// Whether SMTP authentication should be used.
        /// </summary>
        public bool UseAuthentication { get; set; }

        /// <summary>
        /// The last used SMTP username. The password is intentionally not persisted.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Whether TLS should be used for the SMTP connection.
        /// </summary>
        public bool UseTls { get; set; }

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Gets the directory in which settings are stored.
        /// </summary>
        /// <returns>The settings directory path.</returns>
        public static string GetSettingsDirectory()
            { return _settingsDirectory; }

        /// <summary>
        /// Loads settings from disk, or returns default settings if none exist or loading fails.
        /// </summary>
        /// <returns>The loaded or default <see cref="AppSettings"/> instance.</returns>
        public static AppSettings Load()
            {
            try
                {
                lock (_fileLock)
                    {
                    if (File.Exists(_settingsFilePath))
                        {
                        string json = File.ReadAllText(_settingsFilePath);
                        AppSettings? settings = JsonSerializer.Deserialize<AppSettings>(json, _jsonSerializerOptions);
                        return settings ?? new AppSettings();
                        }
                    }
                }
            catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
                {
                Logger.Warn(ex, "Failed to load settings");
                }

            return new AppSettings();
            }

        /// <summary>
        /// Saves the current settings to disk as JSON.
        /// </summary>
        public void Save()
            {
            try
                {
                lock (_fileLock)
                    {
                    if (!Directory.Exists(_settingsDirectory))
                        { Directory.CreateDirectory(_settingsDirectory); }

                    string json = JsonSerializer.Serialize(this, _jsonSerializerOptions);
                    File.WriteAllText(_settingsFilePath, json);
                    }

                Logger.Info("Settings saved successfully");
                }
            catch (Exception ex)
                {
                Logger.Error(ex, "Failed to save settings");
                }
            }

        #endregion Public Methods
        }
    }