namespace ECommerce_G24.Cart.Api.Exceptions
{
    // Se usa para errores 404.
    // Ejemplo: carrito no encontrado o producto no encontrado.
    public class NotFoundException(string errorCode, string message) : Exception(message)
    {
        public string ErrorCode { get; } = errorCode;
    }
}
