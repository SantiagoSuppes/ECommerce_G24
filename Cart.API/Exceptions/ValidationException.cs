namespace Cart.API.Cart.API.Exceptions
{
    // Se usa para errores 400.
    // Ejemplo: cantidad menor o igual a cero.
    public class ValidationException(string errorCode, string message) : Exception(message)
    {
        public string ErrorCode { get; } = errorCode;
    }
}
