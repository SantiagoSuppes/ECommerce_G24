namespace Orders.API.Exceptions;

public class UnprocessableEntityException(string errorCode, string message) : Exception(message)
{
    public string ErrorCode { get; } = errorCode;
}