using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVentas.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using SistemaVentas.Models;

namespace SistemaVentas.Controllers;

public class VentasController : Controller
{
    private readonly AppDbContext _context;

    public VentasController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var ventas = await _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Detalles).ThenInclude(d => d.Producto)
            .OrderByDescending(v => v.Fecha)
            .ToListAsync();
        return View(ventas);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.ClienteId = new SelectList(await _context.Clientes.ToListAsync(), "Id", "Nombre");
        ViewBag.Productos = await _context.Producto.ToListAsync(); // Para usar en la vista si haces un formulario dinámico
        return View(new Venta());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Venta venta)
    {
        if (!ModelState.IsValid) 
        {
            ViewBag.ClienteId = new SelectList(await _context.Clientes.ToListAsync(), "Id", "Nombre", venta.ClienteId);
            return View(venta);
        }

        // Ejecutamos la lógica de negocio de tu modelo
        venta.CalcularTotal();
        venta.CalcularVambio();
        venta.Actualizarstock();

        _context.Ventas.Add(venta);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var venta = await _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Detalles).ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(v => v.Id == id);
            
        if (venta == null) return NotFound();
        return View(venta);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var venta = await _context.Ventas
            .Include(v => v.Cliente)
            .FirstOrDefaultAsync(v => v.Id == id);
            
        if (venta == null) return NotFound();
        return View(venta);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var venta = await _context.Ventas.FindAsync(id);
        if (venta != null)
        {
            _context.Ventas.Remove(venta);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
    [HttpGet("api/clientes")]
    public async Task<IActionResult> BuscarClientes(string search)
    {
        var clientes = await _context.Clientes
            .Where(c => c.Nombre.Contains(search) || c.Id.ToString().Contains(search))
            .Select(c => new { c.Id, c.Nombre })
            .Take(5)
            .ToListAsync();
        
        return Json(clientes);
    }
}