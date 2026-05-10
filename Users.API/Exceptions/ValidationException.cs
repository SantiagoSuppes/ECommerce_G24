namespace Users.API.Exceptions;

public class ValidationException : Exception
{
    public const string ErrorCode = "USR-002";

    public ValidationException(string message) : base(message)
    {
    }
}
