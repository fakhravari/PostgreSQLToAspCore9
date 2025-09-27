using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopApiPostgreSQL.Data;
using ShopApiPostgreSQL.Models;

namespace ShopApiPostgreSQL.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController(ShopDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> GetAll()
    {
        var data = await db.Categories
            .Select(c => new
            {
                c.CategoryId,
                c.Name,
                Products = c.Products.Select(p => new
                {
                    p.ProductId,
                    p.Name,
                    p.Price,
                    p.CategoryId
                }).ToList()
            })
            .ToListAsync();

        return Ok(data);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Category>> Get(int id)
    {
        var data = await db.Categories.Select(c => new
                    {
                        c.CategoryId,
                        c.Name,
                        Products = c.Products.Select(p => new
                        {
                            p.ProductId,
                            p.Name,
                            p.Price,
                            p.CategoryId
                        }).ToList()
                    }).FirstOrDefaultAsync(v => v.CategoryId == id);

        return data is { } c ? Ok(c) : NotFound();
    }

    [HttpPost]
    public async Task<ActionResult<Category>> Create([FromBody] CategoryCreateDto dto)
    {
        var c = new Category { Name = dto.Name, Slug = dto.Slug };
        db.Add(c); await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = c.CategoryId }, c);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CategoryCreateDto dto)
    {
        var c = await db.Categories.FindAsync(id);
        if (c is null) return NotFound();
        c.Name = dto.Name; c.Slug = dto.Slug;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var c = await db.Categories.FindAsync(id);
        if (c is null) return NotFound();
        db.Remove(c); await db.SaveChangesAsync();
        return NoContent();
    }
}