namespace ECommerce_G24.Products.API.Exceptions
{
    // Excepción para errores de validación de productos.
    public class ValidationException(string errorCode, string message):Exception(message)
    {
        public string ErrorCode { get; } = errorCode;

       
    }
  
}
