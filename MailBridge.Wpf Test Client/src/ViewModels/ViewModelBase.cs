using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MailBridge.Wpf.ViewModels;

/// <summary>
/// Base class for view models, providing property change notification support.
/// </summary>
public abstract class ViewModelBase : INotifyPropertyChanged
    {
    #region Events

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion Events

    #region Protected Methods

    /// <summary>
    /// Raises <see cref="PropertyChanged"/> for the given property.
    /// </summary>
    /// <param name="propertyName">The name of the property. Supplied automatically by the compiler.</param>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }

    /// <summary>
    /// Sets the given field to the given value and raises <see cref="PropertyChanged"/>
    /// if the value actually changed.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="field">A reference to the backing field.</param>
    /// <param name="value">The new value.</param>
    /// <param name="propertyName">The name of the property. Supplied automatically by the compiler.</param>
    /// <returns>True if the value changed, otherwise false.</returns>
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
        if (EqualityComparer<T>.Default.Equals(field, value))
            { return false; }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
        }

    #endregion Protected Methods
    }