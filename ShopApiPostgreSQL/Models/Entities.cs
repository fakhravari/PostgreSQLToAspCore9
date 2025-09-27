using Microsoft.EntityFrameworkCore;

namespace ShopApiPostgreSQL.Models;

public enum OrderStatus { Pending, Paid, Shipped, Cancelled }

public class Category
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = null!;
    public string? Slug { get; set; }
    public ICollection<Product> Products { get; set; } = [];
}

public class Product
{
    public int ProductId { get; set; }
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = [];
}

public class Customer
{
    public int CustomerId { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public ICollection<Order> Orders { get; set; } = [];
}

public class Order
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public ICollection<OrderItem> Items { get; set; } = [];
}

public class OrderItem
{
    public int OrderItemId { get; set; }
    public int OrderId { get; set; }
    public Order? Order { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int Qty { get; set; }
    public decimal UnitPrice { get; set; }
}
