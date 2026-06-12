namespace Orders.API.Exceptions;

public class InternalServerException(string errorCode, string message) : Exception(message)
{
    public string ErrorCode { get; } = errorCode;
}