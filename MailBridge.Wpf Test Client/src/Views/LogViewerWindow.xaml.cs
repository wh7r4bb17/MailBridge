using System.Windows;
using MailBridge.Wpf.ViewModels;

namespace MailBridge.Wpf.Views;

/// <summary>
/// Code-behind for the log viewer window.
/// </summary>
public partial class LogViewerWindow : Window
    {
    #region Public Constructors

    /// <summary>
    /// Initializes the log viewer window and its view model.
    /// </summary>
    public LogViewerWindow()
        {
        InitializeComponent();
        DataContext = new LogViewerViewModel();
        }

    #endregion Public Constructors

    #region Private Methods

    private void CloseButton_Click(object sender, RoutedEventArgs e)
        { Close(); }

    #endregion Private Methods
    }