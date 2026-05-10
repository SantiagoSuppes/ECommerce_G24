namespace Users.API.Exceptions;

public class UserBlockedException : Exception
{
    public const string ErrorCode = "USR-004";
    public const string ErrorMessage = "Usuario bloqueado por demasiados intentos fallidos.";

    public UserBlockedException(string message = ErrorMessage) : base(message)
    {
    }
}
