using System;
using System.Windows.Input;

namespace MailBridge.Wpf.Commands;

/// <summary>
/// A simple <see cref="ICommand"/> implementation that delegates execution and
/// can-execute logic to supplied delegates.
/// </summary>
/// <remarks>
/// Initializes a new command with parameterized execute/can-execute delegates.
/// </remarks>
/// <param name="execute">The action to invoke when the command executes.</param>
/// <param name="canExecute">An optional predicate determining whether the command can execute.</param>
public sealed class RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null) : ICommand
    {
    #region Fields

    private readonly Predicate<object?>? _canExecute = canExecute;
    private readonly Action<object?> _execute = execute;

    #endregion Fields

    #region Constructor

    /// <summary>
    /// Initializes a new command with parameterless execute/can-execute delegates.
    /// </summary>
    /// <param name="execute">The action to invoke when the command executes.</param>
    /// <param name="canExecute">An optional predicate determining whether the command can execute.</param>
    public RelayCommand(Action execute, Func<bool>? canExecute = null)
        : this(_ => execute(), canExecute is null ? null : _ => canExecute())
        {
        }

    #endregion Constructor

    #region Events

    /// <inheritdoc />
    public event EventHandler? CanExecuteChanged;

    #endregion Events

    #region Public Methods

    /// <inheritdoc />
    public bool CanExecute(object? parameter)
        { return _canExecute?.Invoke(parameter) ?? true; }

    /// <inheritdoc />
    public void Execute(object? parameter)
        { _execute(parameter); }

    /// <summary>
    /// Notifies subscribers that the result of <see cref="CanExecute"/> may have changed.
    /// </summary>
    public void RaiseCanExecuteChanged()
        { CanExecuteChanged?.Invoke(this, EventArgs.Empty); }

    #endregion Public Methods
    }