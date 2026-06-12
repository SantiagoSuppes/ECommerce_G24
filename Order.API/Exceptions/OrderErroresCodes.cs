namespace Orders.API.Exceptions;

/// <summary>
/// Catálogo de errores de Orders.API definido por el TP.
/// </summary>
public static class OrderErrorCodes
{
    public const string OrderNotFound = "ORD-001";

    public const string InvalidOrderData = "ORD-002";

    public const string UserNotFound = "ORD-003";

    public const string ProductNotFound = "ORD-004";

    public const string InsufficientStock = "ORD-005";

    public const string InvalidStatusTransition = "ORD-006";

    public const string InternalOrderError = "ORD-007";
}