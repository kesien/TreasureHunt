using MudBlazor;

namespace TreasureHuntApp.Client.Services.Utilities;

public class NotificationService : INotificationService
{
    private readonly ISnackbar _snackbar;

    public NotificationService(ISnackbar snackbar)
    {
        _snackbar = snackbar;
    }

    public void ShowSuccess(string message)
    {
        _snackbar.Add(message, Severity.Success, config =>
        {
            config.VisibleStateDuration = 3000;
            config.HideTransitionDuration = 500;
            config.ShowTransitionDuration = 500;
            config.SnackbarVariant = Variant.Filled;
        });
    }

    public void ShowError(string message)
    {
        _snackbar.Add(message, Severity.Error, config =>
        {
            config.VisibleStateDuration = 5000;
            config.HideTransitionDuration = 500;
            config.ShowTransitionDuration = 500;
            config.SnackbarVariant = Variant.Filled;
        });
    }

    public void ShowWarning(string message)
    {
        _snackbar.Add(message, Severity.Warning, config =>
        {
            config.VisibleStateDuration = 4000;
            config.HideTransitionDuration = 500;
            config.ShowTransitionDuration = 500;
            config.SnackbarVariant = Variant.Filled;
        });
    }

    public void ShowInfo(string message)
    {
        _snackbar.Add(message, Severity.Info, config =>
        {
            config.VisibleStateDuration = 3000;
            config.HideTransitionDuration = 500;
            config.ShowTransitionDuration = 500;
            config.SnackbarVariant = Variant.Filled;
        });
    }

    public void Clear()
    {
        _snackbar.Clear();
    }
}
