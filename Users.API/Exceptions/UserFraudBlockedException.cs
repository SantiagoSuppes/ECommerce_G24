namespace ECommerce_G24.source.Users.API.Exceptions;

public class UserFraudBlockedException : Exception
{
    public const string ErrorCode = "USR-005";
    public const string ErrorMessage = "Usuario bloqueado por razones de seguridad.";

    public UserFraudBlockedException() : base(ErrorMessage)
    {
    }
}
