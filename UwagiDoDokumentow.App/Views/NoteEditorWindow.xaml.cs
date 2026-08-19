using System.Windows;
using Microsoft.Win32;
using UwagiDoDokumentow.App.ViewModels;

namespace UwagiDoDokumentow.App.Views;

/// <summary>
/// Formularz dodawania/edycji uwagi do dokumentu.
/// </summary>
public partial class NoteEditorWindow : Window
{
    private readonly NoteEditorViewModel _viewModel;

    public NoteEditorWindow(NoteEditorViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        viewModel.SavedSuccessfully += (_, _) =>
        {
            DialogResult = true;
            Close();
        };
    }

    public Task LoadAsync(int? noteId) => _viewModel.LoadAsync(noteId);

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private async void AddAttachmentButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Obsługiwane pliki|*.jpg;*.jpeg;*.png;*.webp;*.pdf;*.txt;*.rtf;*.doc;*.docx;*.odt;*.ods;*.odp;*.zip;*.rar|Wszystkie pliki|*.*",
            Multiselect = false
        };

        if (dialog.ShowDialog(this) == true)
        {
            await _viewModel.AddAttachmentCommand.ExecuteAsync(dialog.FileName);
        }
    }
}
