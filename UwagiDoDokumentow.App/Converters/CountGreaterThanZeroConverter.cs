using System.Globalization;
using System.Windows.Data;

namespace UwagiDoDokumentow.App.Converters;

/// <summary>
/// Zwraca true, jeśli kolekcja/liczba jest większa od zera (np. liczba załączników).
/// </summary>
public class CountGreaterThanZeroConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture) =>
        value is int count && count > 0;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
