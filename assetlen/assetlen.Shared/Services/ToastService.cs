namespace assetlen.Shared.Services;

public enum ToastKind { Info, Success, Warning, Error }

public sealed record Toast(string Id, ToastKind Kind, string Message, string? Title, int DurationMs);

/// <summary>
/// Transient messages. Replaces Microsoft.FluentUI's IToastService — the method
/// names are kept identical so the fourteen existing call sites did not have to
/// be rewritten alongside the visual redesign.
/// </summary>
public interface IToastService
{
    event Action? OnChange;
    IReadOnlyList<Toast> Current { get; }

    void ShowInfo(string? message, string? title = null);
    void ShowSuccess(string? message, string? title = null);
    void ShowWarning(string? message, string? title = null);
    void ShowError(string? message, string? title = null);
    void Dismiss(string id);
}

public sealed class ToastService : IToastService
{
    private readonly List<Toast> _toasts = new();

    public event Action? OnChange;
    public IReadOnlyList<Toast> Current => _toasts;

    public void ShowInfo(string? message, string? title = null) => Push(ToastKind.Info, message, title, 4500);
    public void ShowSuccess(string? message, string? title = null) => Push(ToastKind.Success, message, title, 4000);
    public void ShowWarning(string? message, string? title = null) => Push(ToastKind.Warning, message, title, 6500);

    // Errors stay until dismissed. An error that vanishes before it is read is
    // the failure mode this whole product exists to argue against.
    public void ShowError(string? message, string? title = null) => Push(ToastKind.Error, message, title, 0);

    private void Push(ToastKind kind, string? message, string? title, int durationMs)
    {
        if (string.IsNullOrWhiteSpace(message)) return;

        _toasts.Add(new Toast(Guid.NewGuid().ToString("N"), kind, message!, title, durationMs));

        // A stack taller than the viewport hides its own oldest entries.
        while (_toasts.Count > 5) _toasts.RemoveAt(0);

        OnChange?.Invoke();
    }

    public void Dismiss(string id)
    {
        var i = _toasts.FindIndex(t => t.Id == id);
        if (i < 0) return;
        _toasts.RemoveAt(i);
        OnChange?.Invoke();
    }
}
