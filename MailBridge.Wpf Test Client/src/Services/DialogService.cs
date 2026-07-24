using System.Windows;

namespace MailBridge.Wpf.Services;

/// <summary>
/// Abstraction over UI dialogs, allowing view models to display messages
/// without a direct dependency on a specific dialog implementation.
/// </summary>
public interface IDialogService
    {
    #region Public Methods

    /// <summary>
    /// Displays an error message.
    /// </summary>
    void ShowError(string message, string title = "Error");

    /// <summary>
    /// Displays an informational message.
    /// </summary>
    void ShowInfo(string message, string title = "Information");

    /// <summary>
    /// Displays a warning message.
    /// </summary>
    void ShowWarning(string message, string title = "Warning");

    #endregion Public Methods
    }

/// <summary>
/// A <see cref="IDialogService"/> implementation backed by WPF's <see cref="MessageBox"/>.
/// </summary>
public sealed class DialogService : IDialogService
    {
    #region Public Methods

    /// <inheritdoc />
    public void ShowError(string message, string title = "Error")
        { MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error); }

    /// <inheritdoc />
    public void ShowInfo(string message, string title = "Information")
        { MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information); }

    /// <inheritdoc />
    public void ShowWarning(string message, string title = "Warning")
        { MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning); }

    #endregion Public Methods
    }