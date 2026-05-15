namespace ECommerce_G24.Cart.API.Exceptions
{
    // Se usa para errores de regla de negocio.
    // Ejemplo: stock insuficiente.
    public class BusinessRuleException(string errorCode, string message) : Exception(message)
    {
        public string ErrorCode { get; } = errorCode;
    }
}
