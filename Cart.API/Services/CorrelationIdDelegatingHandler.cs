namespace ECommerce_G24.Cart.API.Services
{
    // Este handler se ejecuta en las llamadas HTTP salientes.
    // Sirve para enviar el mismo X-Correlation-Id a Products.API.
    public class CorrelationIdDelegatingHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CorrelationIdDelegatingHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            // Buscamos el correlation ID del request actual.
            var correlationId = _httpContextAccessor.HttpContext?
                .Response.Headers["X-Correlation-Id"]
                .FirstOrDefault();

            // Si existe se agrega a la llamada a Products.API.
            if (!string.IsNullOrWhiteSpace(correlationId))
            {
                request.Headers.TryAddWithoutValidation("X-Correlation-Id", correlationId);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
