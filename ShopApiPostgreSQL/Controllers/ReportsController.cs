using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopApiPostgreSQL.Data;

namespace ShopApiPostgreSQL.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController(ShopDbContext db) : ControllerBase
{
    // معادل ویو products + categories
    [HttpGet("products-with-category")]
    public async Task<ActionResult> ProductsWithCategory()
    {
        var data = await db.Products.Include(p => p.Category)
            .Select(p => new {
                IdProduct = p.ProductId,
                ProductName = p.Name,
                CategorieName = p.Category!.Name,
                IdCategory = p.CategoryId
            }).ToListAsync();
        return Ok(data);
    }

    // فروش ماهانه فقط Paid/Shipped
    [HttpGet("monthly-sales")]
    public async Task<ActionResult> MonthlySales([FromQuery] int year)
    {
        var data = await db.Orders
            .Where(o => (o.Status == Models.OrderStatus.Paid || o.Status == Models.OrderStatus.Shipped)
                        && o.OrderDate.Year == year)
            .SelectMany(o => o.Items.Select(i => new { o.OrderId, o.OrderDate, Amount = i.Qty * i.UnitPrice }))
            .GroupBy(x => new { x.OrderDate.Year, x.OrderDate.Month })
            .Select(g => new {
                Year = g.Key.Year,
                Month = g.Key.Month,
                TotalSales = g.Sum(x => x.Amount),
                OrdersCount = g.Select(x => x.OrderId).Distinct().Count()
            })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToListAsync();

        return Ok(data);
    }
}
