using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ECommerce_G24.Products.API.Models
{
    public class Product
    {
        public Guid Id { get; init; }
        public string Nombre { get; set; } =string.Empty;
        public string ?Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; init; }


    }
}



    

