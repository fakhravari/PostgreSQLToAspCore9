using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopApiPostgreSQL.Data;
using ShopApiPostgreSQL.Models;

namespace ShopApiPostgreSQL.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController(ShopDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerWithOrdersDto>>> GetAll()
    {
        var data = await db.Customers
            .AsNoTracking()
            .Select(c => new CustomerWithOrdersDto(
                c.CustomerId,
                c.FullName,
                c.Email,
                c.Orders.Count,
                c.Orders
                 .OrderByDescending(o => o.OrderDate)
                 .Select(o => new OrderDto(
                     o.OrderId,
                     o.OrderDate,
                     o.Status.ToString(),
                     o.Items.Select(i => new OrderItemDto(
                         i.OrderItemId,
                         i.ProductId,
                         i.Qty,
                         i.UnitPrice,
                         i.Qty * i.UnitPrice,
                         new ProductSlimDto(
                             i.Product!.ProductId,
                             i.Product.Name,
                             i.Product.Price,
                             i.Product.CategoryId
                         )
                     )),
                     o.Items.Sum(i => i.Qty * i.UnitPrice)
                 ))
            ))
            .ToListAsync();

        return Ok(data);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CustomerWithOrdersDto>> Get(int id)
    {
        var data = await db.Customers
            .AsNoTracking()
            .Where(c => c.CustomerId == id)
            .Select(c => new CustomerWithOrdersDto(
                c.CustomerId,
                c.FullName,
                c.Email,
                c.Orders.Count(),
                Orders: c.Orders
                    .OrderByDescending(o => o.OrderDate)
                    .Select(o => new OrderDto(
                        o.OrderId,
                        o.OrderDate,
                        o.Status.ToString(),
                        Items: o.Items.Select(i => new OrderItemDto(
                            i.OrderItemId,
                            i.ProductId,
                            i.Qty,
                            i.UnitPrice,
                            LineTotal: i.Qty * i.UnitPrice,
                            Product: new ProductSlimDto(
                                i.Product!.ProductId,
                                i.Product.Name,
                                i.Product.Price,
                                i.Product.CategoryId
                            )
                        )).ToList(),
                        OrderTotal: o.Items.Sum(i => i.Qty * i.UnitPrice)
                    ))
                    .ToList()
            )).SingleOrDefaultAsync();

        return data is null ? NotFound() : Ok(data);
    }


    [HttpPost]
    public async Task<ActionResult<Customer>> Create([FromBody] CustomerCreateDto dto)
    {
        var c = new Customer { FullName = dto.FullName, Email = dto.Email };
        db.Add(c); await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = c.CustomerId }, c);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CustomerCreateDto dto)
    {
        var c = await db.Customers.FindAsync(id);
        if (c is null) return NotFound();
        c.FullName = dto.FullName; c.Email = dto.Email;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var c = await db.Customers.FindAsync(id);
        if (c is null) return NotFound();
        db.Remove(c); await db.SaveChangesAsync();
        return NoContent();
    }
}
