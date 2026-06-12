namespace Users.API.Exceptions;

/// <summary>
/// Excepción utilizada cuando los datos recibidos son inválidos.
/// </summary>
public class ValidationException : Exception
{
    public string ErrorCode { get; }

    public ValidationException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }
}