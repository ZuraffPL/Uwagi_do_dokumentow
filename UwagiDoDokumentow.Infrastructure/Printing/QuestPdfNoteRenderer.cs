using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using UwagiDoDokumentow.Application.Interfaces;
using UwagiDoDokumentow.Infrastructure.Persistence;

namespace UwagiDoDokumentow.Infrastructure.Printing;

/// <summary>
/// Generowanie PDF (pojedynczy rekord / zestawienie) przy użyciu QuestPDF (code-first, bez HTML).
/// Podgląd i druk odbywają się przez domyślną aplikację systemową (Process.Start).
/// </summary>
public class QuestPdfNoteRenderer : IPrintService
{
    private readonly NotesDbContext _dbContext;

    public QuestPdfNoteRenderer(NotesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string> GenerateNotePdfAsync(int noteId, CancellationToken ct = default)
    {
        var note = await _dbContext.DocumentNotes
            .Include(n => n.Attachments)
            .Include(n => n.CreatedByUser)
            .Include(n => n.UpdatedByUser)
            .FirstOrDefaultAsync(n => n.Id == noteId, ct)
            ?? throw new InvalidOperationException($"Nie znaleziono uwagi o id {noteId}.");

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Text($"Uwaga do dokumentu {note.DocumentSymbol} {note.DocumentNumber}")
                    .SemiBold().FontSize(16);

                page.Content().PaddingTop(15).Column(col =>
                {
                    col.Item().Text($"Data dokumentu: {note.DocumentDate:yyyy-MM-dd}");
                    col.Item().Text($"Zlecił: {note.OrderedBy}");
                    if (!string.IsNullOrWhiteSpace(note.Title))
                    {
                        col.Item().Text($"Tytuł: {note.Title}");
                    }
                    col.Item().PaddingTop(10).Text("Treść:").SemiBold();
                    col.Item().Text(note.Content);

                    if (!string.IsNullOrWhiteSpace(note.Tags))
                    {
                        col.Item().PaddingTop(10).Text($"Tagi: {note.Tags}");
                    }

                    if (note.Attachments.Count > 0)
                    {
                        col.Item().PaddingTop(15).Text("Załączniki:").SemiBold();
                        foreach (var attachment in note.Attachments)
                        {
                            col.Item().Text($"- {attachment.OriginalFileName}");
                        }
                    }

                    col.Item().PaddingTop(15)
                        .Text($"Utworzono: {note.CreatedAt:yyyy-MM-dd HH:mm} przez {note.CreatedByUser?.DisplayName}")
                        .FontSize(9).FontColor(Colors.Grey.Darken1);
                    col.Item()
                        .Text($"Ostatnia zmiana: {note.UpdatedAt:yyyy-MM-dd HH:mm} przez {note.UpdatedByUser?.DisplayName}")
                        .FontSize(9).FontColor(Colors.Grey.Darken1);
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
        });

        return SaveToTempFile(document, $"uwaga_{note.Id}");
    }

    public async Task<string> GenerateNotesListPdfAsync(IEnumerable<int> noteIds, CancellationToken ct = default)
    {
        var ids = noteIds.ToList();
        var notes = await _dbContext.DocumentNotes
            .Where(n => ids.Contains(n.Id))
            .Include(n => n.Attachments)
            .OrderBy(n => n.DocumentDate)
            .ToListAsync(ct);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Text("Zestawienie uwag do dokumentów").SemiBold().FontSize(16);

                page.Content().PaddingTop(15).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(70);
                        columns.ConstantColumn(60);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(3);
                        columns.ConstantColumn(50);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("Data").SemiBold();
                        header.Cell().Text("Symbol/nr").SemiBold();
                        header.Cell().Text("Zlecił").SemiBold();
                        header.Cell().Text("Tytuł").SemiBold();
                        header.Cell().Text("Zał.").SemiBold();
                    });

                    foreach (var note in notes)
                    {
                        table.Cell().Text(note.DocumentDate.ToString("yyyy-MM-dd"));
                        table.Cell().Text($"{note.DocumentSymbol} {note.DocumentNumber}");
                        table.Cell().Text(note.OrderedBy);
                        table.Cell().Text(note.Title ?? string.Empty);
                        table.Cell().Text(note.Attachments.Count.ToString());
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
        });

        return SaveToTempFile(document, "zestawienie");
    }

    public void OpenPreview(string pdfFilePath)
    {
        Process.Start(new ProcessStartInfo(pdfFilePath) { UseShellExecute = true });
    }

    public void Print(string pdfFilePath)
    {
        Process.Start(new ProcessStartInfo(pdfFilePath) { UseShellExecute = true, Verb = "print" });
    }

    private static string SaveToTempFile(IDocument document, string fileNamePrefix)
    {
        AppPaths.EnsureDirectoriesExist();
        var path = Path.Combine(AppPaths.TempDirectory, $"{fileNamePrefix}_{Guid.NewGuid():N}.pdf");
        document.GeneratePdf(path);
        return path;
    }
}
