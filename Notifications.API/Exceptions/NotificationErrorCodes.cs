namespace Notifications.API.Exceptions;

/// <summary>
/// Códigos de error para Notifications.API.
/// </summary>
public static class NotificationErrorCodes
{
    public const string UserNotFound = "NTF-001";

    public const string InvalidNotificationData = "NTF-002";

    public const string NotificationsNotFound = "NTF-003";

    public const string InternalNotificationError = "NTF-004";
}