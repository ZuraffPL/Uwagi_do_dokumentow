using UwagiDoDokumentow.App.ViewModels;

namespace UwagiDoDokumentow.App.Views.Admin;

/// <summary>
/// Panel administracyjny słownika symboli dokumentów (tylko dla IsAdmin).
/// </summary>
public partial class DocumentTypesAdminView : System.Windows.Window
{
    public DocumentTypesAdminView(DocumentTypesAdminViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += async (_, _) => await viewModel.InitializeAsync();
    }
}
