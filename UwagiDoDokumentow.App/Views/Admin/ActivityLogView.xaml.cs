using UwagiDoDokumentow.App.ViewModels;

namespace UwagiDoDokumentow.App.Views.Admin;

/// <summary>
/// Przegląd logu aktywności biznesowej (tylko dla IsAdmin).
/// </summary>
public partial class ActivityLogView : System.Windows.Window
{
    public ActivityLogView(ActivityLogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += async (_, _) => await viewModel.InitializeAsync();
    }
}
