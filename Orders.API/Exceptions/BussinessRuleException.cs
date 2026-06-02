namespace ECommerce_G24.Orders.API.Exceptions;

public class BussinessRuleException : Exception
{
    public BussinessRuleException(string message) : base(message)
    {
    }
}
