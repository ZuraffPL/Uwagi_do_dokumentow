using System.Windows;
using UwagiDoDokumentow.App.ViewModels;
using UwagiDoDokumentow.Application.DTO;

namespace UwagiDoDokumentow.App.Views;

/// <summary>
/// Tylko-do-odczytu widok szczegółów uwagi do dokumentu.
/// </summary>
public partial class NoteDetailsWindow : Window
{
    private readonly NoteDetailsViewModel _viewModel;

    public NoteDetailsWindow(NoteDetailsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
    }

    public Task LoadAsync(int noteId) => _viewModel.LoadAsync(noteId);

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

    private void AttachmentThumbnail_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: AttachmentDto attachment } && _viewModel.OpenAttachmentCommand.CanExecute(attachment))
        {
            _viewModel.OpenAttachmentCommand.Execute(attachment);
        }
    }
}
