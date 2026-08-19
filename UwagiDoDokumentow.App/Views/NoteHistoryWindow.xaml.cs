using System.Windows;
using UwagiDoDokumentow.App.ViewModels;

namespace UwagiDoDokumentow.App.Views;

/// <summary>
/// Historia zmian (kto, kiedy, co) pojedynczej uwagi do dokumentu.
/// </summary>
public partial class NoteHistoryWindow : Window
{
    private readonly NoteHistoryViewModel _viewModel;

    public NoteHistoryWindow(NoteHistoryViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
    }

    public Task LoadAsync(int noteId, string? noteLabel = null) => _viewModel.LoadAsync(noteId, noteLabel);

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
