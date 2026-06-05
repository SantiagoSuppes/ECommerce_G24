namespace ECommerce_G24.Notifications.API.Exceptions;

public class UserNotFoundException : Exception
{
    public UserNotFoundException(string message) : base(message)
    {
    }
}
