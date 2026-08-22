namespace DogSab.Platform.Ui.Notifications;

/// <summary>A single notification shown to the user, e.g. as a toast or in a notifications panel.</summary>
public readonly struct Notification(string title, string message, NotificationSeverity severity)
{
    public string Title { get; } = title;
    public string Message { get; } = message;
    public NotificationSeverity Severity { get; } = severity;
}

/// <summary>The severity/visual style of a notification</summary>
public enum NotificationSeverity
{
    Info,
    Warning,
    Error
}