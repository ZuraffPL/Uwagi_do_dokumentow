using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace UwagiDoDokumentow.App.Converters;

/// <summary>
/// Pokazuje element (np. miniaturę) tylko dla rozszerzeń plików graficznych (jpg/jpeg/png/webp).
/// </summary>
public class ImageExtensionToVisibilityConverter : IValueConverter
{
    private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        var ext = (value as string ?? string.Empty).ToLowerInvariant();
        if (!ext.StartsWith('.'))
        {
            ext = "." + ext;
        }

        return Array.IndexOf(ImageExtensions, ext) >= 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
