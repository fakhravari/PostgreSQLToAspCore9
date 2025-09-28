using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using ShopApiPostgreSQL.Data;
using ShopApiPostgreSQL.Models;
using System.Data;

namespace ShopApiPostgreSQL.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController(ShopDbContext db) : ControllerBase
{
    // معادل ویو products + categories
    [HttpGet("products-with-category")]
    public async Task<ActionResult> ProductsWithCategory()
    {
        var data = await db.Products.Include(p => p.Category).Select(p => new
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
            .Where(o => (o.Status == Models.OrderStatus.Paid || o.Status == Models.OrderStatus.Shipped) && o.OrderDate.Year == year)
            .SelectMany(o => o.Items.Select(i => new { o.OrderId, o.OrderDate, Amount = i.Qty * i.UnitPrice }))
            .GroupBy(x => new { x.OrderDate.Year, x.OrderDate.Month })
            .Select(g => new
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                TotalSales = g.Sum(x => x.Amount),
                OrdersCount = g.Select(x => x.OrderId).Distinct().Count()
            }).OrderBy(x => x.Year).ThenBy(x => x.Month).ToListAsync();

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
    [HttpGet("by-customer/{customerId:int}")]
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
    [HttpGet("by-customer2/{customerId:int}")]
    public async Task<ActionResult<IEnumerable<OrderSummary2Dto>>> GetOrdersByCustomer2(int customerId)
    {
        var data = await db.Database.SqlQueryRaw<OrderSummary2Dto>("SELECT * FROM fn_orders_by_customer({0});", customerId).ToListAsync();
        return Ok(data);
    }

    // گرفتن خروجی یک products
    [HttpGet("by-category/{categoryId:int}")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByCategory(int categoryId)
    {
        var result = new List<ProductDto>();

        await using var conn = new NpgsqlConnection(db.Database.GetConnectionString());
        await conn.OpenAsync();

        await using var tx = await conn.BeginTransactionAsync();

        // 1. صدا زدن پروسیجر
        await using (var cmd = new NpgsqlCommand("CALL sp_get_products_by_category(@cat, @cur);", conn, (NpgsqlTransaction)tx))
        {
            cmd.Parameters.AddWithValue("cat", categoryId);
            cmd.Parameters.Add(new NpgsqlParameter("cur", NpgsqlTypes.NpgsqlDbType.Refcursor)
            {
                Direction = ParameterDirection.InputOutput,
                Value = "mycur"
            });

            await cmd.ExecuteNonQueryAsync();
        }

        // 2. گرفتن داده‌ها از cursor
        await using (var fetchCmd = new NpgsqlCommand("FETCH ALL FROM \"mycur\";", conn, (NpgsqlTransaction)tx))
        await using (var reader = await fetchCmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                result.Add(new ProductDto
                {
                    ProductId = reader.GetInt32(0),
                    ProductName = reader.GetString(1),
                    Price = reader.GetDecimal(2)
                });
            }
        }

        await tx.CommitAsync();
        return Ok(result);
    }

}
