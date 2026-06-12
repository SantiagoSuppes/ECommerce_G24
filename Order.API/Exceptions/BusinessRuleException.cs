namespace Orders.API.Exceptions;

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