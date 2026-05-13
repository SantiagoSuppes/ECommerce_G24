namespace ECommerce_G24.Cart.Api.Exceptions
{
    public class ValidationException(string errorCode, string message) : Exception(message)
    {
        public string ErrorCode { get; } = errorCode;
    }
}
