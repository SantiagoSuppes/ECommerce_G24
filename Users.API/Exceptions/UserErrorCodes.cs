namespace Users.API.Exceptions;

/// <summary>
/// Catálogo de códigos de error definido para Users.API.
/// </summary>
public static class UserErrorCodes
{
    public const string DuplicateEmail = "USR-001";

    public const string InvalidUserData = "USR-002";

    public const string InvalidCredentials = "USR-003";

    public const string BlockedByFailedAttempts = "USR-004";

    public const string BlockedByFraud = "USR-005";

    public const string InternalUserError = "USR-006";
}