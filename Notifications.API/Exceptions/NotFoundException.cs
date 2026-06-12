namespace Notifications.API.Exceptions;

/// <summary>
/// Excepción utilizada cuando no se encuentra un usuario
/// o no existen notificaciones registradas.
/// </summary>
public class NotFoundException : Exception
{
    public string ErrorCode { get; }

    public NotFoundException(
        string errorCode,
        string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }
}