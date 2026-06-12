namespace ECommerce_G24.Products.API.Services
{
    /// <summary>
    /// Define la comunicación entre Products.API y Orders.API.
    /// </summary>
    public interface IOrdersClient
    {
        Task<bool> HasActiveOrdersAsync(
            Guid productId,
            CancellationToken cancellationToken = default);
    }
}
