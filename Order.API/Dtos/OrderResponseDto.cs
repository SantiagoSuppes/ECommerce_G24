using Orders.API.DTOs;

namespace Orders.API.Dtos;

/// <summary>
/// DTO de respuesta de una orden.
/// </summary>
public class OrderResponseDto
{
    /// <summary>ID único de la orden.</summary>
    public Guid Id { get; set; }

    /// <summary>ID del usuario que realizó la orden.</summary>
    public Guid UsuarioId { get; set; }

    /// <summary>Lista de items de la orden.</summary>
    public List<OrderItemResponseDto> Items { get; set; } = new();

    /// <summary>Total de la orden.</summary>
    public decimal Total { get; set; }

    /// <summary>Estado actual de la orden.</summary>
    public string Estado { get; set; } = string.Empty;

    /// <summary>Fecha de creación de la orden.</summary>
    public DateTime FechaCreacion { get; set; }
}