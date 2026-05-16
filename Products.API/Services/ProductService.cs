using ECommerce_G24.Products.API.Dtos;
using ECommerce_G24.Products.API.Exceptions;
using ECommerce_G24.Products.API.Models;

namespace ECommerce_G24.Products.API.Services
{
    // Servicio que contiene la lógica de negocio de productos.
    public class ProductService : IProductService
    {
        // Lista en memoria usada como persistencia temporal.
        private static readonly List<Product> Products = new();

        private readonly ILogger<ProductService> _logger;

        public ProductService(ILogger<ProductService> logger)
        {
            _logger = logger;
        }

        // Lista todos los productos y aplica filtros opcionales por nombre y categoría.
        public Task<IEnumerable<ProductResponseDto>> GetAllAsync(string? nombre, string? categoria)
        {
            IEnumerable<Product> query = Products;

            if (!string.IsNullOrWhiteSpace(nombre))
            {
                query = query.Where(p =>
                    p.Nombre.Contains(nombre, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(categoria))
            {
                query = query.Where(p =>
                    p.Categoria.Equals(categoria, StringComparison.OrdinalIgnoreCase));
            }

            var response = query.Select(MapToResponse);

            return Task.FromResult(response);
        }

        // Busca un producto por ID. Si no existe, lanza PRD-001.
        public Task<ProductResponseDto> GetByIdAsync(Guid id)
        {
            var product = Products.FirstOrDefault(p => p.Id == id);

            if (product is null)
                throw new NotFoundException("PRD-001", "Producto no encontrado.");

            return Task.FromResult(MapToResponse(product));
        }

        // Crea un producto. Valida datos y evita duplicados por nombre + categoría.
        public Task<ProductResponseDto> CreateAsync(CreateProductRequestDto request)
        {
            ValidateCreateRequest(request);

            var duplicate = Products.Any(p =>
                p.Nombre.Equals(request.Nombre.Trim(), StringComparison.OrdinalIgnoreCase)
                && p.Categoria.Equals(request.Categoria.Trim(), StringComparison.OrdinalIgnoreCase));

            if (duplicate)
            {
                throw new BusinessRuleException(
                    "PRD-003",
                    $"Ya existe un producto con ese nombre en la categoría '{request.Categoria}'.");
            }

            var product = new Product
            {
                Nombre = request.Nombre.Trim(),
                Descripcion = request.Descripcion?.Trim(),
                Precio = request.Precio,
                Stock = request.Stock,
                Categoria = request.Categoria.Trim()
            };

            Products.Add(product);

            _logger.LogInformation(
                "Producto creado correctamente. ProductId: {ProductId}",
                product.Id);

            return Task.FromResult(MapToResponse(product));
        }

        // Actualiza un producto existente. Si no existe, lanza PRD-001.
        public Task<ProductResponseDto> UpdateAsync(Guid id, UpdateProductRequestDto request)
        {
            ValidateUpdateRequest(request);

            var product = Products.FirstOrDefault(p => p.Id == id);

            if (product is null)
                throw new NotFoundException("PRD-001", "Producto no encontrado.");

            product.Nombre = request.Nombre.Trim();
            product.Descripcion = request.Descripcion?.Trim();
            product.Precio = request.Precio;
            product.Stock = request.Stock;
            product.Categoria = request.Categoria.Trim();

            _logger.LogInformation(
                "Producto actualizado correctamente. ProductId: {ProductId}",
                product.Id);

            return Task.FromResult(MapToResponse(product));
        }

        // Elimina un producto. Si no existe, lanza PRD-001.
        public Task DeleteAsync(Guid id)
        {
            var product = Products.FirstOrDefault(p => p.Id == id);

            if (product is null)
                throw new NotFoundException("PRD-001", "Producto no encontrado.");

            // En una versión integrada con Orders.API, acá se debería consultar
            // si el producto tiene órdenes activas en estado Pendiente o Confirmada.
            // Si tiene órdenes activas, se debe lanzar PRD-004.
            //
            // throw new BusinessRuleException(
            //     "PRD-004",
            //     "El producto tiene órdenes activas y no puede eliminarse.");

            Products.Remove(product);

            _logger.LogInformation(
                "Producto eliminado correctamente. ProductId: {ProductId}",
                id);

            return Task.CompletedTask;
        }

        // Valida los datos del request de creación.
        private static void ValidateCreateRequest(CreateProductRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Nombre)
                || string.IsNullOrWhiteSpace(request.Categoria)
                || request.Precio <= 0
                || request.Stock < 0)
            {
                throw new ValidationException(
                    "PRD-002",
                    "Los datos del producto son inválidos.");
            }
        }

        // Valida los datos del request de actualización.
        private static void ValidateUpdateRequest(UpdateProductRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Nombre)
                || string.IsNullOrWhiteSpace(request.Categoria)
                || request.Precio <= 0
                || request.Stock < 0)
            {
                throw new ValidationException(
                    "PRD-002",
                    "Los datos del producto son inválidos.");
            }
        }

        // Convierte una entidad Product en un DTO de respuesta.
        private static ProductResponseDto MapToResponse(Product product)
        {
            return new ProductResponseDto
            {
                Id = product.Id,
                Nombre = product.Nombre,
                Descripcion = product.Descripcion,
                Precio = product.Precio,
                Stock = product.Stock,
                Categoria = product.Categoria,
                FechaCreacion = product.FechaCreacion
            };
        }
    }
}
