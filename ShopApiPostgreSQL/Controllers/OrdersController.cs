using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopApiPostgreSQL.Data;
using ShopApiPostgreSQL.Models;

namespace ShopApiPostgreSQL.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(ShopDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> List([FromQuery] int? year, [FromQuery] int? month, [FromQuery] OrderStatus? status, [FromQuery] PagedQuery q)
    {
        var query = db.Orders.AsQueryable();

        if (year.HasValue)
        {
            query = query.Where(o => o.OrderDate.Year == year.Value);
            if (month.HasValue) query = query.Where(o => o.OrderDate.Month == month.Value);
        }
        if (status.HasValue) query = query.Where(o => o.Status == status.Value);

        query = query.Include(o => o.Customer).OrderByDescending(o => o.OrderDate);

        var total = await query.CountAsync();
        var items = await query.Skip((q.Page - 1) * q.PageSize).Take(q.PageSize)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .ToListAsync();

        Response.Headers.Append("X-Total-Count", total.ToString());
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult> Get(int id)
    {
        var o = await db.Orders
            .Include(o => o.Customer)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.OrderId == id);

        return o is null ? NotFound() : Ok(o);
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] OrderCreateDto dto)
    {
        var prodIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await db.Products.Where(p => prodIds.Contains(p.ProductId)).ToListAsync();
        if (products.Count != prodIds.Count)
            return BadRequest("یک یا چند ProductId نامعتبر است.");

        var order = new Order
        {
            CustomerId = dto.CustomerId,
            OrderDate = dto.OrderDate ?? DateTime.UtcNow,
            Status = dto.Status ?? OrderStatus.Pending,
            Items = dto.Items.Select(i =>
            {
                var price = products.First(p => p.ProductId == i.ProductId).Price;
                return new OrderItem { ProductId = i.ProductId, Qty = i.Qty, UnitPrice = price };
            }).ToList()
        };

        db.Add(order);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = order.OrderId }, order);
    }

    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] OrderStatus status)
    {
        var o = await db.Orders.FindAsync(id);
        if (o is null) return NotFound();
        o.Status = status;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var o = await db.Orders.FindAsync(id);
        if (o is null) return NotFound();
        db.Remove(o); await db.SaveChangesAsync();
        return NoContent();
    }
}
