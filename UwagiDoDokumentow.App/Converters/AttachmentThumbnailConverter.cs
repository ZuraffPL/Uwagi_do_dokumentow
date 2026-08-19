using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using UwagiDoDokumentow.Application.DTO;
using UwagiDoDokumentow.Infrastructure;

namespace UwagiDoDokumentow.App.Converters;

/// <summary>
/// Generuje miniaturę załącznika (jpg/jpeg/png/webp) do podglądu w oknie szczegółów.
/// Dla pozostałych typów lub brakującego pliku zwraca null (obraz pozostaje ukryty).
/// </summary>
public class AttachmentThumbnailConverter : IValueConverter
{
    private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

    public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not AttachmentDto attachment)
        {
            return null;
        }

        var ext = (attachment.Extension ?? string.Empty).ToLowerInvariant();
        if (!ext.StartsWith('.'))
        {
            ext = "." + ext;
        }

        if (Array.IndexOf(ImageExtensions, ext) < 0)
        {
            return null;
        }

        try
        {
            var fullPath = Path.Combine(AppPaths.AttachmentsDirectory, attachment.RelativePath);
            if (!File.Exists(fullPath))
            {
                return null;
            }

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.DecodePixelWidth = 96;
            bitmap.UriSource = new Uri(fullPath, UriKind.Absolute);
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }
        catch
        {
            // Uszkodzony/niedostępny plik — brak miniatury, nie przerywamy wyświetlania listy.
            return null;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
