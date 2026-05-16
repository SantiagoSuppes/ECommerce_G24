namespace ECommerce_G24.Products.API.Exceptions
{
    // Excepción para productos no encontrados.
    public class NotFoundException(string errorCode,string message):Exception(message)
    {
        public string ErrorCode { get; }=errorCode;

        
    }

}
