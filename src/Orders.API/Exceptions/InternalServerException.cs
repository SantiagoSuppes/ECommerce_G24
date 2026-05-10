namespace ECommerce_G24.src.Orders.API.Exceptions;

public class InternalServerException : Exception
{
    public InternalServerException(string message) : base(message)
    {
    }
}
