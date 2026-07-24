using MailBridge.Wpf.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace MailBridge.Wpf;

/// <summary>
/// Code-behind for the main window. Wires up the view model and handles
/// password box binding and shutdown persistence.
/// </summary>
public partial class MainWindow : Window
    {
    #region Public Constructors

    /// <summary>
    /// Initializes the main window and its view model.
    /// </summary>
    public MainWindow()
        {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
        Closing += OnClosing;
        Loaded += MainWindow_Loaded;
        }

    #endregion Public Constructors

    #region Private Methods

    private static string ExtractHtmlDocument(string clipboardHtml)
        {
        int htmlStart = clipboardHtml.IndexOf("<html", StringComparison.OrdinalIgnoreCase);

        return htmlStart >= 0
            ? clipboardHtml[htmlStart..]
            : clipboardHtml;
        }

    private static string InjectContentEditableAndPasteScript(string html)
        {
        string withEditable = System.Text.RegularExpressions.Regex.Replace(
            html,
            "<body",
            "<body contenteditable=\"true\"",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        const string pasteScript = """
        <script>
        document.addEventListener('paste', function (e) {
            e.preventDefault();
            window.chrome.webview.postMessage('paste-requested');
        });
        </script>
        """;

        return System.Text.RegularExpressions.Regex.Replace(
            withEditable,
            "</head>",
            pasteScript + "</head>",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

    private static string WrapAsEditableDocument(string bodyContent)
        {
        return $$"""
        <html>
        <head>
        <script>
        document.addEventListener('paste', function (e) {
            e.preventDefault();
            window.chrome.webview.postMessage('paste-requested');
        });
        </script>
        </head>
        <body contenteditable="true" style="font-family:sans-serif;padding:8px;">
        {{bodyContent}}
        </body>
        </html>
        """;
        }

    private void EmailBodyTextBox_OnPaste(object sender, DataObjectPastingEventArgs e)
        {
        if (DataContext is not MainWindowViewModel vm)
            { return; }

        if (!vm.IsHtmlEditMode && e.SourceDataObject.GetDataPresent(DataFormats.Html))
            {
            string html = (string)e.SourceDataObject.GetData(DataFormats.Html);
            vm.EmailHtmlBody = html;
            }
        }

    private async Task LoadHtmlIntoEditorAsync(string html)
        {
        bool isFullDocument =
            html.Contains("<body", StringComparison.OrdinalIgnoreCase) &&
            html.Contains("</head>", StringComparison.OrdinalIgnoreCase);

        string editableHtml = isFullDocument
            ? InjectContentEditableAndPasteScript(html)
            : WrapAsEditableDocument(html);

        HtmlBodyWebView.NavigateToString(editableHtml);
        }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
        await HtmlBodyWebView.EnsureCoreWebView2Async();

        HtmlBodyWebView.CoreWebView2.Settings.IsScriptEnabled = true;
        HtmlBodyWebView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;
        HtmlBodyWebView.CoreWebView2.Profile.PreferredColorScheme = Microsoft.Web.WebView2.Core.CoreWebView2PreferredColorScheme.Light;

        if (DataContext is MainWindowViewModel vm)
            {
            vm.PropertyChanged += async (_, args) =>
            {
                if (args.PropertyName == nameof(MainWindowViewModel.IsHtmlEditMode) && vm.IsHtmlEditMode)
                    { await LoadHtmlIntoEditorAsync(vm.EmailHtmlBody); }
            };
            }
        }

    private async void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
        if (DataContext is MainWindowViewModel vm)
            {
            if (vm.IsHtmlEditMode)
                { vm.EmailHtmlBody = await ReadHtmlFromEditorAsync(); }

            vm.SaveOnExit();
            }
        }

    private async void OnWebMessageReceived(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
        {
        if (e.TryGetWebMessageAsString() != "paste-requested")
            { return; }

        if (!Clipboard.ContainsData(DataFormats.Html))
            { return; }

        string rawHtml = (string)Clipboard.GetData(DataFormats.Html);
        string html = ExtractHtmlDocument(rawHtml);

        if (DataContext is MainWindowViewModel vm)
            { vm.EmailHtmlBody = html; }

        await LoadHtmlIntoEditorAsync(html);
        }

    private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
        if (DataContext is MainWindowViewModel vm && sender is System.Windows.Controls.PasswordBox passwordBox)
            { vm.Password = passwordBox.Password; }
        }

    private async Task<string> ReadHtmlFromEditorAsync()
        {
        string json = await HtmlBodyWebView.ExecuteScriptAsync("document.body.innerHTML");
        return System.Text.Json.JsonSerializer.Deserialize<string>(json) ?? string.Empty;
        }

    private async void SendButton_OnClick(object sender, RoutedEventArgs e)
        {
        if (DataContext is MainWindowViewModel vm)
            {
            if (vm.IsHtmlEditMode)
                { vm.EmailHtmlBody = await ReadHtmlFromEditorAsync(); }

            if (vm.SendMailCommand.CanExecute(PwdBox))
                { vm.SendMailCommand.Execute(PwdBox); }
            }
        }

    #endregion Private Methods
    }