using MailBridge.Core;
using MailBridge.Wpf.Commands;
using MailBridge.Wpf.ViewModels;
using System;
using System.IO;

namespace MailBridge.Wpf.ViewModels
    {
    /// <summary>
    /// View model for the log viewer window, displaying today's log contents.
    /// </summary>
    public sealed class LogViewerViewModel : ViewModelBase
        {
        #region Fields

        private string _logText = string.Empty;

        #endregion Fields

        #region Constructor

        /// <summary>
        /// Initializes the view model and loads today's log.
        /// </summary>
        public LogViewerViewModel()
            {
            RefreshCommand = new RelayCommand(RefreshLog);
            OpenFolderCommand = new RelayCommand(OpenLogFolder);
            RefreshLog();
            }

        #endregion Constructor

        #region Properties

        /// <summary>
        /// The current log text displayed in the viewer.
        /// </summary>
        public string LogText
            {
            get { return _logText; }
            set { SetProperty(ref _logText, value); }
            }

        /// <summary>
        /// Opens the log directory in the file explorer.
        /// </summary>
        public RelayCommand OpenFolderCommand { get; }

        /// <summary>
        /// Reloads today's log contents.
        /// </summary>
        public RelayCommand RefreshCommand { get; }

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Reloads today's log file contents from disk.
        /// </summary>
        public void RefreshLog()
            {
            try
                {
                string logDirectory = Path.Combine(AppSettings.GetSettingsDirectory(), "Logs");
                string logPath = Path.Combine(logDirectory, $"mailbridge_{DateTime.Now:yyyyMMdd}.log");

                if (!File.Exists(logPath))
                    {
                    LogText = "No log entries for today.";
                    return;
                    }

                using FileStream stream = new(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using StreamReader reader = new(stream);
                LogText = reader.ReadToEnd();
                }
            catch (Exception ex)
                {
                LogText = $"Error reading log file: {ex.Message}";
                }
            }

        #endregion Public Methods

        #region Private Methods

        private static void OpenLogFolder()
            {
            try
                {
                string logDirectory = Path.Combine(AppSettings.GetSettingsDirectory(), "Logs");

                if (!Directory.Exists(logDirectory))
                    { Directory.CreateDirectory(logDirectory); }

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                    FileName = "explorer.exe",
                    Arguments = logDirectory,
                    UseShellExecute = true
                    });
                }
            catch
                {
                // Best-effort; failure to open the folder is not critical.
                }
            }

        #endregion Private Methods
        }
    }