namespace TreasureHuntApp.Client.Services.Utilities;

public interface INotificationService
{
    void Clear();
    void ShowError(string message);
    void ShowInfo(string message);
    void ShowSuccess(string message);
    void ShowWarning(string message);
}