namespace Users.API.Exceptions;

public class InvalidCredentialsException : Exception
{
    public const string ErrorCode = "USR-003";
    public const string ErrorMessage = "Las credenciales no son válidas.";

    public InvalidCredentialsException() : base(ErrorMessage)
    {
    }
}
