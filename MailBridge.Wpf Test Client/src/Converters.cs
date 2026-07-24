using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MailBridge.Wpf.Converters
    {
    public sealed class BoolToVisibilityConverter : IValueConverter
        {
        #region Public Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            { return value is true ? Visibility.Visible : Visibility.Collapsed; }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            { return value is Visibility.Visible; }

        #endregion Public Methods
        }

    public sealed class InverseBoolToVisibilityConverter : IValueConverter
        {
        #region Public Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            { return value is true ? Visibility.Collapsed : Visibility.Visible; }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            { return value is not Visibility.Visible; }

        #endregion Public Methods
        }
    }