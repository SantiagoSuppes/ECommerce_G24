namespace Users.API.ExceptionHandlers;

/// <summary>
/// Funciones compartidas por los handlers de excepciones.
/// </summary>
public static class ExceptionHandlerHelper
{
    public const string ErrorCodeItemName = "ErrorCode";

    public static string GetCorrelationId(
        HttpContext context)
    {
        if (context.Items.TryGetValue(
                "X-Correlation-Id",
                out var value))
        {
            return value?.ToString()
                   ?? context.TraceIdentifier;
        }

        return context.TraceIdentifier;
    }
}