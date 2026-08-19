using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace UwagiDoDokumentow.App.Converters;

/// <summary>
/// Odwrotność BooleanToVisibilityConverter — true daje Collapsed, false daje Visible.
/// </summary>
public class InverseBooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture) =>
        value is true ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
