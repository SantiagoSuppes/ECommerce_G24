namespace Notifications.API.Models;

/// <summary>
/// Tipos de notificación
/// </summary>
public static class NotificationTypes
{
    public const string Email = "Email";

    public const string Push = "Push";

    public const string Sms = "SMS";

    /// <summary>
    /// Indica si el tipo ingresado está permitido.
    /// </summary>
    public static bool IsValid(string? tipo)
    {
        return tipo is not null &&
               (tipo.Equals(Email, StringComparison.OrdinalIgnoreCase) ||
                tipo.Equals(Push, StringComparison.OrdinalIgnoreCase) ||
                tipo.Equals(Sms, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Normaliza el valor para almacenarlo siempre igual.
    /// </summary>
    public static string Normalize(string tipo)
    {
        if (tipo.Equals(Email, StringComparison.OrdinalIgnoreCase))
            return Email;

        if (tipo.Equals(Push, StringComparison.OrdinalIgnoreCase))
            return Push;

        return Sms;
    }
}