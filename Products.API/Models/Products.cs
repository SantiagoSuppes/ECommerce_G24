using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ECommerce_G24.Products.API.Models
{
    public class Products
    {
        public Guid Id { get; init; }
        public string Nombre { get; init; } =string.Empty;
        public string ?Descripcion { get; init; }
        public decimal Precio { get; init; }
        public int Stock { get; init; }
        public string Categoria { get; init; } = string.Empty;
        public DateTime FechaCreacion { get; init; }


    }
}



    

