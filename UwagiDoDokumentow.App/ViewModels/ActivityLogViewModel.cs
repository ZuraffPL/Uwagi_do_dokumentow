using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using UwagiDoDokumentow.Application.Interfaces;
using UwagiDoDokumentow.Domain.Entities;

namespace UwagiDoDokumentow.App.ViewModels;

/// <summary>
/// Przegląd logu aktywności biznesowej (activity_log) — tylko dla administratora.
/// </summary>
public partial class ActivityLogViewModel : ObservableObject
{
    private readonly IActivityLogReaderService _activityLogReader;

    public ActivityLogViewModel(IActivityLogReaderService activityLogReader)
    {
        _activityLogReader = activityLogReader;
    }

    public ObservableCollection<ActivityLogEntry> Entries { get; } = new();

    public async Task InitializeAsync()
    {
        var entries = await _activityLogReader.GetRecentAsync();
        Entries.Clear();
        foreach (var entry in entries)
        {
            Entries.Add(entry);
        }
    }
}
