namespace Users.API.Exceptions;

/// <summary>
/// Excepción utilizada para reglas de negocio de Users.API.
/// conflictos, credenciales incorrectas y bloqueos.
/// </summary>
public class BusinessRuleException : Exception
{
    public string ErrorCode { get; }

    public int StatusCode { get; }

    public BusinessRuleException(
        string errorCode,
        string message,
        int statusCode)
        : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }
}