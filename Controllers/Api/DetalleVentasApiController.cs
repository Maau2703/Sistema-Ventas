using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVentas.Models;
using SistemaVentas.Data;

namespace SistemaVentas.Controllers.Api;

[ApiController]
[Route("api/detalleventas")]
public class DetalleVentasApiController : ControllerBase
{
    private readonly AppDbContext _context;

    public DetalleVentasApiController(AppDbContext context)
    {
        _context = context;
    }
    
    // GET api/detalleventas
    [HttpGet]
    public async Task<ActionResult<List<DetalleVenta>>> GetAll()
    {
        return await _context.DetalleVentas
            .Include(d => d.Producto)
            .Include(d => d.venta)
            .ToListAsync();
    }

    // GET api/detalleventas/1
    [HttpGet("{id}")]
    public async Task<ActionResult<DetalleVenta>> GetById(int id)
    {
        var detalleVenta = await _context.DetalleVentas
            .Include(d => d.Producto)
            .Include(d => d.venta)
            .FirstOrDefaultAsync(d => d.Id == id);
            
        if (detalleVenta == null)
            return NotFound();
        
        return detalleVenta;
    }

    // POST api/detalleventas
    [HttpPost]
    public async Task<ActionResult<DetalleVenta>> Create(DetalleVenta detalleVenta)
    {
        detalleVenta.SubTotal(detalleVenta.Cantidad, detalleVenta.Precio);
        
        _context.DetalleVentas.Add(detalleVenta);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = detalleVenta.Id }, detalleVenta);
    }

    // PUT api/detalleventas/3
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, DetalleVenta detalleVenta)
    {
        if (id != detalleVenta.Id)
            return BadRequest("El id de la url no coincide con el del cuerpo");
        if (!await _context.DetalleVentas.AnyAsync(d => d.Id == id)) 
            return NotFound();
        
        _context.DetalleVentas.Update(detalleVenta);
        await _context.SaveChangesAsync();
        return NoContent();
    } 

    // DELETE api/detalleventas/3
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var detalleVenta = await _context.DetalleVentas.FindAsync(id);
        if (detalleVenta == null)
            return NotFound();

        _context.DetalleVentas.Remove(detalleVenta);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}