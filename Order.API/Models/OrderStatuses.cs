namespace Orders.API.Models;

/// <summary>
/// Estados y transiciones permitidas para las órdenes.
/// </summary>
public static class OrderStatuses
{
    public const string Pending = "Pendiente";

    public const string Confirmed = "Confirmada";

    public const string Shipped = "Enviada";

    public const string Delivered = "Entregada";

    public const string Cancelled = "Cancelada";

    /// <summary>
    /// Indica si el valor ingresado corresponde
    /// a uno de los estados reconocidos.
    /// </summary>
    public static bool IsValid(string? status)
    {
        return status is not null &&
               (status.Equals(
                    Pending,
                    StringComparison.OrdinalIgnoreCase) ||
                status.Equals(
                    Confirmed,
                    StringComparison.OrdinalIgnoreCase) ||
                status.Equals(
                    Shipped,
                    StringComparison.OrdinalIgnoreCase) ||
                status.Equals(
                    Delivered,
                    StringComparison.OrdinalIgnoreCase) ||
                status.Equals(
                    Cancelled,
                    StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Normaliza mayúsculas y minúsculas.
    /// </summary>
    public static string Normalize(string status)
    {
        if (status.Equals(
                Pending,
                StringComparison.OrdinalIgnoreCase))
        {
            return Pending;
        }

        if (status.Equals(
                Confirmed,
                StringComparison.OrdinalIgnoreCase))
        {
            return Confirmed;
        }

        if (status.Equals(
                Shipped,
                StringComparison.OrdinalIgnoreCase))
        {
            return Shipped;
        }

        if (status.Equals(
                Delivered,
                StringComparison.OrdinalIgnoreCase))
        {
            return Delivered;
        }

        return Cancelled;
    }

    /// <summary>
    /// Comprueba si se permite cambiar del estado actual
    /// al nuevo estado.
    /// </summary>
    public static bool CanTransition(
        string currentStatus,
        string newStatus)
    {
        var current = Normalize(currentStatus);
        var next = Normalize(newStatus);

        return current switch
        {
            Pending =>
                next == Confirmed ||
                next == Cancelled,

            Confirmed =>
                next == Shipped ||
                next == Cancelled,

            Shipped =>
                next == Delivered,

            // Entregada y Cancelada son estados finales.
            Delivered => false,
            Cancelled => false,

            _ => false
        };
    }
}