namespace Users.API.Exceptions;

public class BussinessRuleException : Exception
{
    public BussinessRuleException(string message) : base(message)
    {
    }
}
