using UwagiDoDokumentow.Application.Interfaces;
using UwagiDoDokumentow.Domain.Entities;

namespace UwagiDoDokumentow.Infrastructure.Storage;

/// <summary>
/// Przechowywanie załączników na dysku lokalnym/sieciowym pod:
/// attachments\{rok}\{miesiąc}\{guid}.{ext}. Nazwa pliku na dysku to zawsze
/// GUID + oryginalne rozszerzenie — nigdy oryginalna nazwa (path traversal, kolizje).
/// </summary>
public class LocalAttachmentStorage : IAttachmentStorage
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp",
        ".pdf", ".txt", ".rtf", ".doc", ".docx", ".odt", ".ods", ".odp",
        ".zip", ".rar"
    };

    private const long MaxFileSizeBytes = 50 * 1024 * 1024; // 50 MB

    public async Task<NoteAttachment> StoreAsync(string sourceFilePath, int uploadedByUserId, CancellationToken ct = default)
    {
        if (!File.Exists(sourceFilePath))
        {
            throw new FileNotFoundException("Plik źródłowy nie istnieje.", sourceFilePath);
        }

        var extension = Path.GetExtension(sourceFilePath);
        if (!AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException($"Rozszerzenie „{extension}” nie jest dozwolone.");
        }

        var fileInfo = new FileInfo(sourceFilePath);
        if (fileInfo.Length > MaxFileSizeBytes)
        {
            throw new InvalidOperationException("Plik przekracza maksymalny dozwolony rozmiar (50 MB).");
        }

        var now = DateTime.Now;
        var relativeDir = Path.Combine(now.Year.ToString(), now.Month.ToString("D2"));
        var storedFileName = $"{Guid.NewGuid()}{extension}";
        var relativePath = Path.Combine(relativeDir, storedFileName);

        var targetDir = Path.Combine(AppPaths.AttachmentsDirectory, relativeDir);
        Directory.CreateDirectory(targetDir);
        var targetPath = Path.Combine(targetDir, storedFileName);

        using (var sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read))
        using (var targetStream = new FileStream(targetPath, FileMode.CreateNew, FileAccess.Write))
        {
            await sourceStream.CopyToAsync(targetStream, ct);
        }

        return new NoteAttachment
        {
            OriginalFileName = Path.GetFileName(sourceFilePath),
            StoredFileName = storedFileName,
            RelativePath = relativePath,
            ContentType = null,
            Extension = extension.TrimStart('.').ToLowerInvariant(),
            SizeBytes = fileInfo.Length,
            UploadedByUserId = uploadedByUserId,
            CreatedAt = DateTime.Now
        };
    }

    public void Delete(NoteAttachment attachment)
    {
        var path = GetFullPath(attachment);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    public string GetFullPath(NoteAttachment attachment) =>
        Path.Combine(AppPaths.AttachmentsDirectory, attachment.RelativePath);
}
