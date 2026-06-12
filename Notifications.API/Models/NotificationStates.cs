namespace Notifications.API.Models;

/// <summary>
/// Estados permitidos para una notificación.
/// </summary>
public static class NotificationStates
{
    public const string Pending = "Pendiente";

    public const string Sent = "Enviada";

    public const string Failed = "Fallida";
}