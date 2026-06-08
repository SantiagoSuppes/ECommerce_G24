using System.Globalization;

namespace ECommerce_G24.Products.API.Exceptions
{
    // Excepción para reglas de negocio.
    public class BusinessRuleException (string errorCode,string message):Exception(message)
    {
        public string ErrorCode { get; } = errorCode;
    }
}
