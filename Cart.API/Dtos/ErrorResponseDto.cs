namespace Cart.API.Cart.API.Dtos
{
    // DTO usado para documentar en Swagger las respuestas de error.
    public class ErrorResponseDto
    {
        // URL de referencia al tipo de error HTTP.
        public string Type { get; set; } = string.Empty;

        // Título corto del error.
        public string Title { get; set; } = string.Empty;

        // Código de estado HTTP.
        public int Status { get; set; }

        // Descripción general del error.
        public string Detail { get; set; } = string.Empty;

        // Endpoint que produjo el error.
        public string Instance { get; set; } = string.Empty;

        // Código propio del catálogo de Cart.API.
        public string ErrorCode { get; set; } = string.Empty;

        // Mensaje propio del catálogo de Cart.API.
        public string ErrorMessage { get; set; } = string.Empty;

        // Identificador de correlación del request.
        public string CorrelationId { get; set; } = string.Empty;
    }
}
