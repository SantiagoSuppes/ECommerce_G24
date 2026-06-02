namespace ECommerce_G24.Orders.API.Exceptions;

public class InternalServerException : Exception
{
    public InternalServerException(string message) : base(message)
    {
    }
}
