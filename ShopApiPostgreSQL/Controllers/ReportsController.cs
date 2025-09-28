using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopApiPostgreSQL.Data;
using ShopApiPostgreSQL.Models;

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
            .Select(p => new
            {
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
            .Select(g => new
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                TotalSales = g.Sum(x => x.Amount),
                OrdersCount = g.Select(x => x.OrderId).Distinct().Count()
            })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToListAsync();

        return Ok(data);
    }

    // صدا زدن ویو
    [HttpGet("vw_products-with-category")]
    public async Task<ActionResult<IEnumerable<VwProductWithCategory>>> GetProductsWithCategory()
    {
        var sql = "SELECT * FROM vw_products_with_category";
        var data = await db.Database.SqlQueryRaw<VwProductWithCategory>(sql).ToListAsync();
        return Ok(data);
    }

    // صدا زدن فانکشن تیبلی
    [HttpGet("orders/by-customer/{customerId:int}")]
    public async Task<ActionResult<IEnumerable<OrderSummaryDto>>> GetOrdersByCustomer(int customerId)
    {
        var sql = """
SELECT
  order_id    AS "OrderId",
  order_date  AS "OrderDate",
  status      AS "Status",
  items_count AS "ItemsCount",
  order_total AS "OrderTotal"
FROM fn_orders_by_customer({0});
""";

        var data = await db.Database.SqlQueryRaw<OrderSummaryDto>(sql, customerId).ToListAsync();
        return Ok(data);
    }

    // صدا زدن فانکشن تیبلی با تنظیم ستون در ویو مدل
    [HttpGet("orders/by-customer2/{customerId:int}")]
    public async Task<ActionResult<IEnumerable<OrderSummary2Dto>>> GetOrdersByCustomer2(int customerId)
    {
        var data = await db.Database
            .SqlQueryRaw<OrderSummary2Dto>("SELECT * FROM fn_orders_by_customer({0});", customerId)
            .ToListAsync();
        return Ok(data);
    }
}
