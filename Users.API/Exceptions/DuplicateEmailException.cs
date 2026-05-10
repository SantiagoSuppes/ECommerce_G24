namespace Users.API.Exceptions;

public class DuplicateEmailException : Exception
{
    public const string ErrorCode = "USR-001";
    public const string ErrorMessage = "El email ya está registrado.";

    public DuplicateEmailException() : base(ErrorMessage)
    {
    }
}
