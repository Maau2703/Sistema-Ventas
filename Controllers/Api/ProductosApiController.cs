using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVentas.Models;
using SistemaVentas.Data;

namespace SistemaVentas.Controllers.Api;

[ApiController]
[Route("api/productos")]
public class ProductosApiController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductosApiController(AppDbContext context)
    {
        _context = context;
    }
    
    [HttpGet]
    public async Task<ActionResult<List<Producto>>> GetAll()
    {
        return await _context.Producto.Include(p => p.Categoria).OrderBy(p => p.Nombre).ToListAsync();
    }

    [HttpGet("buscar")]
    public async Task<IActionResult> Buscar(string search)
    {
        if (string.IsNullOrEmpty(search))
            return Ok(new List<object>());

        var productos = await _context.Producto
            .Include(p => p.Categoria)
            .Where(p => p.Nombre.Contains(search) && p.Stock > 0)
            .Select(p => new { 
                p.Id, 
                p.Nombre, 
                p.Precio, 
                p.Stock 
            })
            .Take(10)
            .ToListAsync();

        return Ok(productos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Producto>> GetById(int id)
    {
        var producto = await _context.Producto.Include(p => p.Categoria).FirstOrDefaultAsync(p => p.Id == id);
        if (producto == null)
            return NotFound();
        
        return producto;
    }

    [HttpPost]
    public async Task<ActionResult<Producto>> Create(Producto producto)
    {
        _context.Producto.Add(producto);
        await _context.SaveChangesAsync();

        var productoCreado = await _context.Producto
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Id == producto.Id) ?? producto;

        return CreatedAtAction(nameof(GetById), new { id = productoCreado.Id }, productoCreado);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, Producto producto)
    {
        if (id != producto.Id)
            return BadRequest("El id de la url no coincide con el del cuerpo");
        if (!await _context.Producto.AnyAsync(p => p.Id == id)) 
            return NotFound();
            
        _context.Producto.Update(producto);
        await _context.SaveChangesAsync();
        return NoContent();
    } 

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var producto = await _context.Producto.FindAsync(id);
        if (producto == null)
            return NotFound();

        if (await _context.DetalleVentas.AnyAsync(d => d.ProductoId == id))
            return Conflict("No se puede eliminar: el producto tiene ventas registradas");
            
        _context.Producto.Remove(producto);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}