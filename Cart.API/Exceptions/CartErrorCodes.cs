namespace ECommerce_G24.Cart.API.Exceptions
{
    // Catálogo centralizado de errores de Cart.API.
 
    public static class CartErrorCodes
    {
        public const string CartNotFound = "CRT-001";
        public const string ProductNotFound = "CRT-002";
        public const string InsufficientStock = "CRT-003";
        public const string InvalidQuantity = "CRT-004";
        public const string InternalCartError = "CRT-005";
    }
}
