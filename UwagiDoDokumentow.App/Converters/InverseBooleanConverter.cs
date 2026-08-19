using System.Globalization;
using System.Windows.Data;

namespace UwagiDoDokumentow.App.Converters;

/// <summary>
/// Odwraca wartość bool — przydatne np. do IsEnabled="{Binding IsBusy, Converter=...}".
/// </summary>
public class InverseBooleanConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture) =>
        !(value is true);

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        !(value is true);
}
