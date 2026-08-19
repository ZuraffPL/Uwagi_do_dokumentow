using System.Globalization;
using System.Windows.Data;

namespace UwagiDoDokumentow.App.Converters;

/// <summary>
/// Wyświetla "(Wszystkie)" dla pustego symbolu dokumentu (pseudo-pozycja "brak filtra"
/// w ComboBoxie wyboru symbolu), w przeciwnym razie zwraca symbol bez zmian.
/// </summary>
public class EmptySymbolToAllTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture) =>
        string.IsNullOrWhiteSpace(value as string) ? "(Wszystkie)" : value!;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
