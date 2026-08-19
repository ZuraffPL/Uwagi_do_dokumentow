using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using UwagiDoDokumentow.App.ViewModels;

namespace UwagiDoDokumentow.App.Views;

/// <summary>
/// Lista uwag do dokumentów — otwiera edytor/szczegóły w odpowiedzi na żądania z ViewModelu.
/// </summary>
public partial class NotesListView : UserControl
{
    public NotesListView(NotesListViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        viewModel.AddRequested += async (_, _) => await OpenEditorAsync(null);
        viewModel.EditRequested += async (_, id) => await OpenEditorAsync(id);
        viewModel.DetailsRequested += (_, id) => OpenDetails(id);
        viewModel.HistoryRequested += (_, id) => OpenHistory(id);

        Loaded += async (_, _) => await viewModel.InitializeAsync();
    }

    private void NotesGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        var viewModel = (NotesListViewModel)DataContext;
        if (viewModel.SelectedNote is not null && viewModel.ShowDetailsCommand.CanExecute(null))
        {
            viewModel.ShowDetailsCommand.Execute(null);
        }
    }

    /// <summary>
    /// PPM na wierszu nie zmienia zaznaczenia domyślnie w WPF DataGrid — zaznaczamy
    /// ręcznie wiersz pod kursorem, żeby menu kontekstowe (Archiwizuj) działało na nim.
    /// </summary>
    private void NotesGrid_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        var row = FindAncestor<DataGridRow>(e.OriginalSource as DependencyObject);
        if (row is not null)
        {
            row.IsSelected = true;
        }
    }

    private static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
    {
        while (current is not null)
        {
            if (current is T target)
            {
                return target;
            }

            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }

    private async Task OpenEditorAsync(int? noteId)
    {
        var viewModel = (NotesListViewModel)DataContext;
        var editorWindow = App.Services.GetRequiredService<NoteEditorWindow>();
        editorWindow.Owner = Window.GetWindow(this);
        await editorWindow.LoadAsync(noteId);

        if (editorWindow.ShowDialog() == true)
        {
            await viewModel.SearchCommand.ExecuteAsync(null);
        }
    }

    private void OpenDetails(int noteId)
    {
        var detailsWindow = App.Services.GetRequiredService<NoteDetailsWindow>();
        detailsWindow.Owner = Window.GetWindow(this);
        _ = detailsWindow.LoadAsync(noteId);
        detailsWindow.ShowDialog();
    }

    private void OpenHistory(int noteId)
    {
        var viewModel = (NotesListViewModel)DataContext;
        var note = viewModel.SelectedNote;
        var label = note is not null
            ? $"{note.DocumentSymbol} {note.DocumentNumber} — {note.Title}"
            : null;

        var historyWindow = App.Services.GetRequiredService<NoteHistoryWindow>();
        historyWindow.Owner = Window.GetWindow(this);
        _ = historyWindow.LoadAsync(noteId, label);
        historyWindow.ShowDialog();
    }
}
