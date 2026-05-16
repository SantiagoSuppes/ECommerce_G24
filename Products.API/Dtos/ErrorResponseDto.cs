namespace ECommerce_G24.Products.API.Dtos
{
    // DTO estándar para todas las respuestas de error.
    public class ErrorResponseDto
    {
        // URL de referencia del tipo de error HTTP.
        public string Type { get; set; } = string.Empty;

        // Título corto del error.
        public string Title { get; set; } = string.Empty;

        // Código HTTP del error.
        public int Status { get; set; }

        // Detalle general del error.
        public string Detail { get; set; } = string.Empty;

        // Endpoint donde ocurrió el error.
        public string Instance { get; set; } = string.Empty;

        // Código propio del catálogo del TP.
        public string ErrorCode { get; set; } = string.Empty;

        // Mensaje propio del catálogo del TP.
        public string ErrorMessage { get; set; } = string.Empty;

        // Identificador de correlación del request.
        public string? CorrelationId { get; set; }
    }
}
