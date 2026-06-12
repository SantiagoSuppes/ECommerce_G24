namespace ECommerce_G24.source.Users.API.Exceptions;

public class InternalServerException : Exception
{
    public const string ErrorCode = "USR-006";
    public const string ErrorMessage = "Error interno del servidor.";

    public InternalServerException(string message = ErrorMessage, Exception? innerException = null) 
        : base(message, innerException)
    {
    }
}
