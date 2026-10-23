using MailBridge.Core;
using MailBridge.Wpf.Commands;
using MailBridge.Wpf.Services;
using MailBridge.Wpf.Views;
using MailKit.Security;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MailBridge.Wpf.ViewModels
    {
    /// <summary>
    /// View model for the main window, allowing the user to configure SMTP connection
    /// details and send a test email.
    /// </summary>
    public partial class MainWindowViewModel : ViewModelBase
        {
        #region Fields

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IDialogService _dialogService;
        private readonly SmtpService _smtpService;
        private ObservableCollection<string> _attachmentPaths = [];
        private CancellationTokenSource? _cancellationTokenSource;
        private string _emailBcc = string.Empty;
        private string _emailBody = "This is a test";
        private string _emailCc = string.Empty;
        private string _emailFrom = string.Empty;
        private string _emailFromName = string.Empty;
        private string _emailHtmlBody = string.Empty;
        private string _emailSubject = "Test Email";
        private string _emailTo = string.Empty;
        private bool _enableLogging = true;
        private bool _isBusy;
        private bool _isHtmlEditMode;
        private string _password = string.Empty;
        private string _portText = "587";
        private string _sendButtonText = "Send Mail";
        private AppSettings _settings = null!;
        private string _smtpServer = string.Empty;
        private int _timeoutSeconds = 30;
        private int _tlsVersionIndex;
        private bool _useAuthentication;
        private string _username = string.Empty;
        private bool _useTls;

        #endregion Fields

        #region Constructor

        /// <summary>
        /// Initializes a new instance using default service implementations.
        /// </summary>
        public MainWindowViewModel() : this(new DialogService(), new SmtpService())
            { }

        /// <summary>
        /// Initializes a new instance using the given dialog and SMTP services.
        /// </summary>
        /// <param name="dialogService">The dialog service used to show messages to the user.</param>
        /// <param name="smtpService">The SMTP service used to send emails.</param>
        public MainWindowViewModel(IDialogService dialogService, SmtpService smtpService)
            {
            _dialogService = dialogService;
            _smtpService = smtpService;

            TlsVersions = ["Auto (System Default)", "TLS 1.0", "TLS 1.1", "TLS 1.2", "TLS 1.3"];

            SendMailCommand = new RelayCommand(async parameter => await SendMailAsync(parameter), parameter => !IsBusy);
            CancelCommand = new RelayCommand(CancelSend, () => IsBusy);
            ViewLogCommand = new RelayCommand(OpenLogViewer);
            OpenLogFolderCommand = new RelayCommand(OpenLogDirectory);
            AddAttachmentCommand = new RelayCommand(AddAttachment);
            RemoveAttachmentCommand = new RelayCommand(parameter => RemoveAttachment(parameter as string));
            Load();
            }

        #endregion Constructor

        #region Regex

        [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase)]
        private static partial Regex EmailValidationRegex();

        #endregion Regex

        #region Properties

        /// <summary>
        /// Add an Attachement
        /// </summary>
        public RelayCommand AddAttachmentCommand { get; }

        /// <summary>
        /// Whether the credentials fields should be enabled in the UI.
        /// </summary>
        public bool AreCredentialsEnabled => UseAuthentication;

        /// <summary>
        /// The Pathes from Attachments
        /// </summary>
        public ObservableCollection<string> AttachmentPaths
            {
            get { return _attachmentPaths; }
            set { SetProperty(ref _attachmentPaths, value); }
            }

        /// <summary>
        /// Cancels an in-progress send operation.
        /// </summary>
        public RelayCommand CancelCommand { get; }

        /// <summary>
        /// The BCC recipient email address(es), semicolon-separated.
        /// </summary>
        public string EmailBcc
            {
            get { return _emailBcc; }
            set { SetProperty(ref _emailBcc, value); }
            }

        /// <summary>
        /// The email body text.
        /// </summary>
        public string EmailBody
            {
            get { return _emailBody; }
            set { SetProperty(ref _emailBody, value); }
            }

        /// <summary>
        /// The CC recipient email address(es), semicolon-separated.
        /// </summary>
        public string EmailCc
            {
            get { return _emailCc; }
            set { SetProperty(ref _emailCc, value); }
            }

        /// <summary>
        /// The sender email address.
        /// </summary>
        public string EmailFrom
            {
            get { return _emailFrom; }
            set { SetProperty(ref _emailFrom, value); }
            }

        /// <summary>
        /// The display name of the sender, shown alongside the from address.
        /// </summary>
        public string EmailFromName
            {
            get { return _emailFromName; }
            set { SetProperty(ref _emailFromName, value); }
            }

        /// <summary>
        /// The HTML representation of the email body, captured automatically when
        /// HTML content is pasted into the plain-text body field. Not shown in the UI.
        /// </summary>
        public string EmailHtmlBody
            {
            get { return _emailHtmlBody; }
            set { _emailHtmlBody = value; }
            }

        /// <summary>
        /// The email subject.
        /// </summary>
        public string EmailSubject
            {
            get { return _emailSubject; }
            set { SetProperty(ref _emailSubject, value); }
            }

        /// <summary>
        /// The recipient email address.
        /// </summary>
        public string EmailTo
            {
            get { return _emailTo; }
            set { SetProperty(ref _emailTo, value); }
            }

        /// <summary>
        /// Whether logging is enabled.
        /// </summary>
        public bool EnableLogging
            {
            get { return _enableLogging; }
            set
                {
                if (SetProperty(ref _enableLogging, value))
                    {
                    LoggingConfig.SetMinLevel(value
                        ? LogLevel.Debug
                        : LogLevel.Error);

                    _logger.Info("Logging {State}", value ? "enabled" : "disabled");
                    }
                }
            }

        /// <summary>
        /// Whether a send operation is currently in progress.
        /// </summary>
        public bool IsBusy
            {
            get { return _isBusy; }
            set
                {
                if (SetProperty(ref _isBusy, value))
                    {
                    OnPropertyChanged(nameof(IsNotBusy));
                    OnPropertyChanged(nameof(IsCancelVisible));
                    SendButtonText = value ? "Sending..." : "Send Mail";
                    SendMailCommand.RaiseCanExecuteChanged();
                    CancelCommand.RaiseCanExecuteChanged();
                    }
                }
            }

        /// <summary>
        /// Whether the cancel button should be visible.
        /// </summary>
        public bool IsCancelVisible => IsBusy;

        /// <summary>
        /// Whether the HTML editor (WebView2) is shown instead of the plain-text editor.
        /// </summary>
        public bool IsHtmlEditMode
            {
            get { return _isHtmlEditMode; }
            set { SetProperty(ref _isHtmlEditMode, value); }
            }

        /// <summary>
        /// The inverse of <see cref="IsBusy"/>, for UI binding convenience.
        /// </summary>
        public bool IsNotBusy => !IsBusy;

        /// <summary>
        /// Whether the TLS version selector should be enabled in the UI.
        /// </summary>
        public bool IsTlsVersionEnabled => UseTls;

        /// <summary>
        /// Opens the log directory in the file explorer.
        /// </summary>
        public RelayCommand OpenLogFolderCommand { get; }

        /// <summary>
        /// The SMTP password used for authentication. Not persisted to settings.
        /// </summary>
        public string Password
            {
            get { return _password; }
            set { SetProperty(ref _password, value); }
            }

        /// <summary>
        /// The SMTP port, as entered by the user.
        /// </summary>
        public string PortText
            {
            get { return _portText; }
            set { SetProperty(ref _portText, value); }
            }

        /// <summary>
        /// remove an Attachement
        /// </summary>
        public RelayCommand RemoveAttachmentCommand { get; }

        /// <summary>
        /// The current text of the send button, reflecting the busy state.
        /// </summary>
        public string SendButtonText
            {
            get { return _sendButtonText; }
            set { SetProperty(ref _sendButtonText, value); }
            }

        /// <summary>
        /// Sends the configured test email.
        /// </summary>
        public RelayCommand SendMailCommand { get; }

        /// <summary>
        /// The SMTP server host name or IP address.
        /// </summary>
        public string SmtpServer
            {
            get { return _smtpServer; }
            set { SetProperty(ref _smtpServer, value); }
            }

        /// <summary>
        /// The connection timeout, in seconds, clamped between 5 and 300.
        /// </summary>
        public int TimeoutSeconds
            {
            get { return _timeoutSeconds; }
            set { SetProperty(ref _timeoutSeconds, Math.Clamp(value, 5, 300)); }
            }

        /// <summary>
        /// The index of the selected TLS version option in the UI. Not persisted —
        /// resets to the default on each application start.
        /// </summary>
        public int TlsVersionIndex
            {
            get { return _tlsVersionIndex; }
            set { SetProperty(ref _tlsVersionIndex, value); }
            }

        /// <summary>
        /// The list of selectable TLS version options.
        /// </summary>
        public IReadOnlyList<string> TlsVersions { get; }

        /// <summary>
        /// Whether SMTP authentication should be used.
        /// </summary>
        public bool UseAuthentication
            {
            get { return _useAuthentication; }
            set
                {
                if (SetProperty(ref _useAuthentication, value) && !value)
                    {
                    Username = string.Empty;
                    Password = string.Empty;
                    }

                OnPropertyChanged(nameof(AreCredentialsEnabled));
                }
            }

        /// <summary>
        /// The SMTP username used for authentication.
        /// </summary>
        public string Username
            {
            get { return _username; }
            set { SetProperty(ref _username, value); }
            }

        /// <summary>
        /// Whether TLS should be used for the SMTP connection.
        /// </summary>
        public bool UseTls
            {
            get { return _useTls; }
            set
                {
                if (SetProperty(ref _useTls, value))
                    { OnPropertyChanged(nameof(IsTlsVersionEnabled)); }
                }
            }

        /// <summary>
        /// Opens the log viewer window.
        /// </summary>
        public RelayCommand ViewLogCommand { get; }

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Persists the current settings and logs application shutdown. Should be
        /// called when the main window is closing.
        /// </summary>
        public void SaveOnExit()
            {
            SaveSettingsFromProperties();
            _logger.Info("Application closing.");
            }

        #endregion Public Methods

        #region Private Methods

        private static bool IsValidEmail(string email)
            { return EmailValidationRegex().IsMatch(email); }

        private static void OpenLogDirectory()
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
            catch (Exception ex)
                {
                _logger.Error(ex, "Failed to open log directory");
                }
            }

        private void AddAttachment()
            {
            Microsoft.Win32.OpenFileDialog dialog = new()
                {
                Multiselect = true,
                Title = "Select Attachments"
                };

            if (dialog.ShowDialog() == true)
                {
                foreach (string file in dialog.FileNames)
                    {
                    if (!AttachmentPaths.Contains(file))
                        { AttachmentPaths.Add(file); }
                    }
                }
            }

        private void CancelSend()
            {
            _cancellationTokenSource?.Cancel();
            _logger.Info("Send operation cancelled by user.");
            IsBusy = false;
            }

        private SecureSocketOptions GetSecureSocketOptions()
            {
            if (!UseTls)
                { return SecureSocketOptions.None; }

            return TlsVersionIndex is 0 or -1 ? SecureSocketOptions.Auto : SecureSocketOptions.StartTls;
            }

        private void Load()
            {
            _settings = AppSettings.Load();

            LoggingConfig.SetMinLevel(_settings.EnableLogging
                ? LogLevel.Debug
                : LogLevel.Error);

            _logger.Info("Application started.");

            SmtpServer = _settings.SmtpServer;
            PortText = _settings.Port.ToString();
            UseAuthentication = _settings.UseAuthentication;
            Username = _settings.Username;
            UseTls = _settings.UseTls;
            EmailFrom = _settings.EmailFrom;
            EmailFromName = _settings.EmailFromName;
            EmailTo = _settings.EmailTo;
            TimeoutSeconds = Math.Clamp(_settings.ConnectionTimeoutSeconds, 5, 300);
            EnableLogging = _settings.EnableLogging;

            _logger.Debug("Settings loaded to form.");
            }

        private void OpenLogViewer()
            {
            LogViewerWindow window = new()
                {
                Owner = Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
                };
            window.ShowDialog();
            }

        private void RemoveAttachment(string? path)
            {
            if (path is not null)
                { AttachmentPaths.Remove(path); }
            }

        private void SaveSettingsFromProperties()
            {
            _settings.SmtpServer = SmtpServer.Trim();

            if (int.TryParse(PortText.Trim(), out int port))
                { _settings.Port = port; }

            _settings.UseAuthentication = UseAuthentication;
            _settings.Username = Username.Trim();
            _settings.UseTls = UseTls;
            _settings.EmailFrom = EmailFrom.Trim();
            _settings.EmailFromName = EmailFromName.Trim();
            _settings.EmailTo = EmailTo.Trim();
            _settings.ConnectionTimeoutSeconds = TimeoutSeconds;
            _settings.EnableLogging = EnableLogging;
            _settings.Save();
            }

        private async Task SendMailAsync(object? parameter)
            {
            if (parameter is PasswordBox pwdBox)
                { Password = pwdBox.Password; }

            if (!ValidateInputs())
                { return; }

            string host = SmtpServer.Trim();
            int port = int.Parse(PortText.Trim());
            string fromEmail = EmailFrom.Trim();
            string[] toEmails = EmailTo.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            string[] ccEmails = EmailCc.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            string[] bccEmails = EmailBcc.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            _logger.Debug($"Email send attempt - Server: {host}:{port}, From: {fromEmail}, To: {string.Join("; ", toEmails)}, TLS: {(UseTls ? "Yes" : "No")}");
            _cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(TimeoutSeconds));

            try
                {
                IsBusy = true;

                SmtpSendRequest request = new()
                    {
                    Host = host,
                    Port = port,
                    SocketOptions = GetSecureSocketOptions(),
                    FromEmail = fromEmail,
                    FromName = EmailFromName.Trim(),
                    ToEmails = toEmails,
                    CcEmails = ccEmails,
                    BccEmails = bccEmails,
                    Subject = EmailSubject,
                    Body = EmailBody,
                    HtmlBody = string.IsNullOrWhiteSpace(EmailHtmlBody) ? null : EmailHtmlBody,
                    Attachments = [.. AttachmentPaths],
                    UseAuthentication = UseAuthentication,
                    Username = Username,
                    Password = Password,
                    TimeoutMilliseconds = TimeoutSeconds * 1000
                    };

                await _smtpService.SendAsync(request, _cancellationTokenSource.Token);

                _logger.Info($"Email sent successfully to {string.Join("; ", toEmails)} via {host}");
                _dialogService.ShowInfo("Email Sent.", "Success");
                SaveSettingsFromProperties();
                }
            catch (OperationCanceledException)
                {
                if (_cancellationTokenSource?.IsCancellationRequested == true)
                    {
                    string message = IsBusy ? "Connection timed out." : "Operation cancelled by user.";
                    _logger.Warn($"Email send failed to {string.Join("; ", toEmails)} via {host}: {message}");
                    _dialogService.ShowWarning(message, "Cancelled");
                    }
                }
            catch (Exception ex)
                {
                _logger.Error(ex, $"Email send failed to {string.Join("; ", toEmails)} via {host}");
                _dialogService.ShowError($"Error sending email: {ex.Message}", "Error");
                }
            finally
                {
                IsBusy = false;
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
                }
            }

        private void ShowValidationError(string message)
            {
            _logger.Warn($"Validation error: {message}");
            _dialogService.ShowWarning(message, "Validation Error");
            }

        private bool ValidateInputs()
            {
            if (string.IsNullOrWhiteSpace(SmtpServer))
                {
                ShowValidationError("Please enter the SMTP server name.");
                return false;
                }

            if (string.IsNullOrWhiteSpace(PortText))
                {
                ShowValidationError("Please enter the SMTP port number.");
                return false;
                }

            if (!int.TryParse(PortText.Trim(), out int port) || port is < 1 or > 65535)
                {
                ShowValidationError("Please enter a valid port number (1-65535).");
                return false;
                }

            if (UseAuthentication && string.IsNullOrWhiteSpace(Username))
                {
                ShowValidationError("Please enter a username for authentication.");
                return false;
                }

            if (string.IsNullOrWhiteSpace(EmailFrom))
                {
                ShowValidationError("Please enter a sender email address.");
                return false;
                }

            if (!IsValidEmail(EmailFrom.Trim()))
                {
                ShowValidationError("Please enter a valid sender email address.");
                return false;
                }

            if (string.IsNullOrWhiteSpace(EmailTo))
                {
                ShowValidationError("Please enter at least one recipient email address.");
                return false;
                }

            string[] toEmails = EmailTo.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            if (toEmails.Length == 0)
                {
                ShowValidationError("Please enter at least one recipient email address.");
                return false;
                }

            foreach (string to in toEmails)
                {
                if (!IsValidEmail(to))
                    {
                    ShowValidationError($"'{to}' is not a valid email address.");
                    return false;
                    }
                }

            string[] ccEmails = EmailCc.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            foreach (string cc in ccEmails)
                {
                if (!IsValidEmail(cc))
                    {
                    ShowValidationError($"'{cc}' is not a valid CC email address.");
                    return false;
                    }
                }

            string[] bccEmails = EmailBcc.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            foreach (string bcc in bccEmails)
                {
                if (!IsValidEmail(bcc))
                    {
                    ShowValidationError($"'{bcc}' is not a valid BCC email address.");
                    return false;
                    }
                }

            return true;
            }

        #endregion Private Methods
        }
    }