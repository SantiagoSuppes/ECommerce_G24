using ECommerce_G24.Products.API.Dtos;
using ECommerce_G24.Products.API.Exceptions;
using ECommerce_G24.Products.API.Models;
namespace ECommerce_G24.Products.API.Services
{
    public class ProductService : IProductService
    {
        private static readonly List<Product> Products = new();
        public Task<IEnumerable<ProductResponseDto>> GetAllAsync(string? nombre, string? categoria)
        {
            var query = Products.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(categoria))
                query = query.Where(p => p.Categoria.Equals(categoria, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(nombre))
                query = query.Where(p => p.Nombre.Contains(nombre, StringComparison.OrdinalIgnoreCase));

            var response = query.Select(product => new ProductResponseDto
            {
                Id = product.Id,
                Nombre = product.Nombre,
                Descripcion = product.Descripcion,
                Precio = product.Precio,
                Stock = product.Stock,
                Categoria = product.Categoria,
                FechaCreacion = product.FechaCreacion
            });

            return Task.FromResult(response);
        }
        public Task<ProductResponseDto> GetByIDAsync(Guid id)
        {
            var product = Products.FirstOrDefault(p => p.Id == id);

            if (product == null)
                throw new NotFoundException(
                    "PRD-001",
                    "Producto no encontrado.");

            return Task.FromResult(new ProductResponseDto
            {
                Id = product.Id,
                Nombre = product.Nombre,
                Descripcion = product.Descripcion,
                Precio = product.Precio,
                Stock = product.Stock,
                Categoria = product.Categoria,
                FechaCreacion = product.FechaCreacion
            });
        }

        public Task<ProductResponseDto> CreateAsync(CreateProductRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Nombre) ||
                request.Nombre.Length > 100 ||
                request.Precio <= 0 ||
                request.Stock < 0 ||
                string.IsNullOrWhiteSpace(request.Categoria))
            {
                throw new ValidationException(
                    "PRD-002",
                    "Los datos del producto son inválidos.");
            }

            var exists = Products.Any(p =>
                p.Nombre.Equals(request.Nombre, StringComparison.OrdinalIgnoreCase) &&
                p.Categoria.Equals(request.Categoria, StringComparison.OrdinalIgnoreCase));

            if (exists)
            {
                throw new BusinessRuleException(
                    "PRD-003",
                    $"Ya existe un producto con ese nombre en la categoría '{request.Categoria}'.");
            }

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Nombre = request.Nombre,
                Descripcion = request.Descripcion,
                Precio = request.Precio,
                Stock = request.Stock,
                Categoria = request.Categoria,
                FechaCreacion = DateTime.UtcNow
            };

            Products.Add(product);

            return Task.FromResult(new ProductResponseDto
            {
                Id = product.Id,
                Nombre = product.Nombre,
                Descripcion = product.Descripcion,
                Precio = product.Precio,
                Stock = product.Stock,
                Categoria = product.Categoria,
                FechaCreacion = product.FechaCreacion
            });
        }

        public Task<ProductResponseDto> UpdateAsync(Guid id, UpdateProductRequestDto request)
        {
            var product = Products.FirstOrDefault(p => p.Id == id);

            if (product == null)
                throw new NotFoundException(
                    "PRD-001",
                    "Producto no encontrado.");

            if (string.IsNullOrWhiteSpace(request.Nombre) ||
                request.Nombre.Length > 100 ||
                request.Precio <= 0 ||
                request.Stock < 0 ||
                string.IsNullOrWhiteSpace(request.Categoria))
            {
                throw new ValidationException(
                    "PRD-002",
                    "Los datos del producto son inválidos.");
            }

            product.Nombre = request.Nombre;
            product.Descripcion = request.Descripcion;
            product.Precio = request.Precio;
            product.Stock = request.Stock;
            product.Categoria = request.Categoria;

            return Task.FromResult(new ProductResponseDto
            {
                Id = product.Id,
                Nombre = product.Nombre,
                Descripcion = product.Descripcion,
                Precio = product.Precio,
                Stock = product.Stock,
                Categoria = product.Categoria,
                FechaCreacion = product.FechaCreacion
            });
        }

        public Task DeleteAsync(Guid id)
        {
            var product = Products.FirstOrDefault(p => p.Id == id);

            if (product == null)
                throw new NotFoundException(
                    "PRD-001",
                    "Producto no encontrado.");

            // Simulación de validación de órdenes activas
            var tieneOrdenesActivas = false;

            if (tieneOrdenesActivas)
            {
                throw new BusinessRuleException(
                    "PRD-004",
                    "El producto tiene órdenes activas y no puede eliminarse.");
            }

            Products.Remove(product);

            return Task.CompletedTask;
        }
    }
}
