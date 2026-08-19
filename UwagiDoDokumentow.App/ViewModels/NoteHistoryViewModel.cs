using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using UwagiDoDokumentow.Application.Interfaces;
using UwagiDoDokumentow.Domain.Entities;

namespace UwagiDoDokumentow.App.ViewModels;

/// <summary>
/// Historia zmian (kto, kiedy, co) pojedynczej uwagi do dokumentu — na bazie activity_log.
/// </summary>
public partial class NoteHistoryViewModel : ObservableObject
{
    private readonly IActivityLogReaderService _activityLogReader;

    public NoteHistoryViewModel(IActivityLogReaderService activityLogReader)
    {
        _activityLogReader = activityLogReader;
    }

    public ObservableCollection<ActivityLogEntry> Entries { get; } = new();

    [ObservableProperty]
    private string? noteLabel;

    public async Task LoadAsync(int noteId, string? noteLabel = null)
    {
        NoteLabel = noteLabel;
        var entries = await _activityLogReader.GetForEntityAsync(nameof(DocumentNote), noteId);
        Entries.Clear();
        foreach (var entry in entries)
        {
            Entries.Add(entry);
        }
    }
}
