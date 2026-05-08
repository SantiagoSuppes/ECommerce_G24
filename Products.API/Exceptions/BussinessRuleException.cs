using System.Globalization;

namespace ECommerce.Products.API.Exceptions
{
    public class BussinessRuleException (string errorCode,string message):Exception(message)
    {
        public string ErrorCode { get; } = errorCode;
    }
}
