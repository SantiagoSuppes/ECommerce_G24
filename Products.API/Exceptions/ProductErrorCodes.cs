namespace ECommerce_G24.Products.API.Exceptions
{
    // Catálogo centralizado de códigos de error de Products.API.
    
    public class ProductErrorCodes
    {
        public const string ProductNotFound = "PRD-001";
        public const string InvalidProductData = "PRD-002";
        public const string DuplicateProduct = "PRD-003";
        public const string ProductWithActiveOrders = "PRD-004";
        public const string InternalProductError = "PRD-005";
    }
}
