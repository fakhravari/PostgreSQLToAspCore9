using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopApiPostgreSQL.Data;
using ShopApiPostgreSQL.Models;

namespace ShopApiPostgreSQL.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(ShopDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] PagedQuery q)
    {
        var query = db.Products.Include(p => p.Category).AsNoTracking().OrderBy(p => p.ProductId);
        var total = await query.CountAsync();
        var items = await query.Skip((q.Page - 1) * q.PageSize).Take(q.PageSize).ToListAsync();
        Response.Headers.Append("X-Total-Count", total.ToString());
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Product>> Get(int id)
    {
        var p = await db.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.ProductId == id);
        return p is null ? NotFound() : Ok(p);
    }

    [HttpPost]
    public async Task<ActionResult<Product>> Create([FromBody] ProductCreateDto dto)
    {
        if (!await db.Categories.AnyAsync(c => c.CategoryId == dto.CategoryId))
            return BadRequest("Invalid CategoryId.");

        var p = new Product { Name = dto.Name, Price = dto.Price, CategoryId = dto.CategoryId };
        db.Add(p); await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = p.ProductId }, p);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ProductCreateDto dto)
    {
        var p = await db.Products.FindAsync(id);
        if (p is null) return NotFound();
        p.Name = dto.Name; p.Price = dto.Price; p.CategoryId = dto.CategoryId;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var p = await db.Products.FindAsync(id);
        if (p is null) return NotFound();
        db.Remove(p); await db.SaveChangesAsync();
        return NoContent();
    }

    // معادل JOIN: محصول + نام دسته
    [HttpGet("with-category")]
    public async Task<ActionResult> WithCategory()
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
}
