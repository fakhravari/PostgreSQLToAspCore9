
using System.ComponentModel.DataAnnotations;

namespace ShopApiPostgreSQL.Models;


public record PagedQuery([Range(1, int.MaxValue)] int Page = 1, [Range(1, 200)] int PageSize = 20);

public record CategoryCreateDto(
    [Required, StringLength(100)] string Name,
    [StringLength(100)] string? Slug);

public record ProductCreateDto(
    [Required, StringLength(150)] string Name,
    [Range(0, double.MaxValue)] decimal Price,
    [Required] int CategoryId);

public record CustomerCreateDto(
    [Required, StringLength(150)] string FullName,
    [Required, EmailAddress, StringLength(150)] string Email);

public record OrderItemCreateDto([Required] int ProductId, [Range(1, int.MaxValue)] int Qty);

public record OrderCreateDto(
    [Required] int CustomerId,
    [Required] List<OrderItemCreateDto> Items,
    DateTime? OrderDate,
    OrderStatus? Status);




public record ProductSlimDto(int ProductId, string Name, decimal Price, int CategoryId);
public record OrderItemDto(int OrderItemId, int ProductId, int Qty, decimal UnitPrice, decimal LineTotal, ProductSlimDto Product);
public record OrderDto(int OrderId, DateTime OrderDate, string Status, IEnumerable<OrderItemDto> Items, decimal OrderTotal);
public record CustomerWithOrdersDto(int CustomerId, string FullName, string Email, int OrderCounts, IEnumerable<OrderDto> Orders);
