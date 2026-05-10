using System.Globalization;

namespace ECommerce_G24.source.Products.API.Exceptions
{
    public class BussinessRuleException (string errorCode,string message):Exception(message)
    {
        public string ErrorCode { get; } = errorCode;
    }
}
