using System.Windows;
using UwagiDoDokumentow.App.ViewModels;

namespace UwagiDoDokumentow.App.Views;

/// <summary>
/// Okno "Historia wersji" — lista zmian w kolejnych wersjach aplikacji.
/// </summary>
public partial class VersionHistoryWindow : Window
{
    public VersionHistoryWindow(VersionHistoryViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
