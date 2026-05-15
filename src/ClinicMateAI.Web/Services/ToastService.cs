namespace ClinicMateAI.Web.Services;

public sealed class ToastService
{
    public event Action<ToastMessage>? OnShow;
    public event Action<Guid>? OnHide;

    public void Success(string message, int durationMs = 3000) => Show(message, ToastLevel.Success, durationMs);
    public void Error(string message, int durationMs = 4500) => Show(message, ToastLevel.Error, durationMs);
    public void Warning(string message, int durationMs = 4000) => Show(message, ToastLevel.Warning, durationMs);
    public void Info(string message, int durationMs = 3000) => Show(message, ToastLevel.Info, durationMs);

    private void Show(string message, ToastLevel level, int durationMs)
    {
        var toast = new ToastMessage(Guid.NewGuid(), message, level, durationMs);
        OnShow?.Invoke(toast);
    }

    public void Hide(Guid id)
    {
        OnHide?.Invoke(id);
    }
}

public enum ToastLevel
{
    Success = 1,
    Error = 2,
    Info = 3,
    Warning = 4,
}

public sealed record ToastMessage(Guid Id, string Message, ToastLevel Level, int DurationMs);
