namespace UwagiDoDokumentow.App.Services;

/// <summary>
/// Prosty dyspozytor UI — pozwala wykonać akcję w wątku UI z dowolnego miejsca aplikacji.
/// </summary>
public class UiDispatcher
{
    public void Invoke(Action action) => System.Windows.Application.Current.Dispatcher.Invoke(action);

    public Task InvokeAsync(Action action) => System.Windows.Application.Current.Dispatcher.InvokeAsync(action).Task;
}
