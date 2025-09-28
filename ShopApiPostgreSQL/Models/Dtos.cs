
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopApiPostgreSQL.Models;

public record PagedQuery([Range(1, int.MaxValue)] int Page = 1, [Range(1, 200)] int PageSize = 20);
public record CategoryCreateDto([Required, StringLength(100)] string Name, [StringLength(100)] string? Slug);
public record ProductCreateDto([Required, StringLength(150)] string Name, [Range(0, double.MaxValue)] decimal Price, [Required] int CategoryId);

public record CustomerCreateDto([Required, StringLength(150)] string FullName, [Required, EmailAddress, StringLength(150)] string Email);

public record OrderItemCreateDto([Required] int ProductId,
    [Range(1, int.MaxValue)] int Qty);

public record OrderCreateDto([Required] int CustomerId,
    [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)][Required] List<OrderItemCreateDto> Items,
    DateTime? OrderDate,
    OrderStatus? Status);




public record ProductSlimDto(int ProductId, string Name, decimal Price, int CategoryId);
public record OrderItemDto(int OrderItemId, int ProductId, int Qty, decimal UnitPrice, decimal LineTotal, ProductSlimDto Product);
public record OrderDto(int OrderId,
    DateTime OrderDate,
    string Status,
    [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    IEnumerable<OrderItemDto> Items,
    decimal OrderTotal);
public record CustomerWithOrdersDto(int CustomerId, string FullName, string Email, int OrderCounts,
    [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)] IEnumerable<OrderDto> Orders);


public class VwProductWithCategory
{
    public int IdProduct { get; set; }
    public string ProductName { get; set; } = null!;
    public string CategorieName { get; set; } = null!;
    public int IdCategory { get; set; }
}


public class OrderSummaryDto
{
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = default!;
    public int ItemsCount { get; set; }
    public decimal OrderTotal { get; set; }
}

public class OrderSummary2Dto
{
    [Column("order_id")] public int OrderId { get; set; }
    [Column("order_date")] public DateTime OrderDate { get; set; }
    [Column("status")] public string Status { get; set; } = default!;
    [Column("items_count")] public int ItemsCount { get; set; }
    [Column("order_total")] public decimal OrderTotal { get; set; }
}