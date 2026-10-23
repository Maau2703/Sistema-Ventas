using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVentas.Data;
using SistemaVentas.Models;

namespace SistemaVentas.Controllers;

[Authorize]
public class ReportesController : Controller
{
    private readonly AppDbContext _context;

    public ReportesController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.TotalVentas = await _context.Ventas.SumAsync(v => (decimal?)v.Total) ?? 0;
        ViewBag.TotalProductos = await _context.Producto.CountAsync();
        ViewBag.TotalClientes = await _context.Clientes.CountAsync();
        ViewBag.TotalCategorias = await _context.Categorias.CountAsync();
        ViewBag.ProductosBajoStock = await _context.Producto.CountAsync(p => p.Stock <= p.StockMinimo);

        return View();
    }

    public async Task<IActionResult> Ventas(DateTime? desde, DateTime? hasta)
    {
        var d = desde ?? DateTime.Today.AddDays(-30);
        var h = hasta ?? DateTime.Today;

        var ventas = await _context.Ventas
            .Include(v => v.Cliente)
            .Where(v => v.Fecha >= d && v.Fecha <= h)
            .OrderByDescending(v => v.Fecha)
            .ToListAsync();

        ViewBag.Desde = d;
        ViewBag.Hasta = h;
        ViewBag.Total = ventas.Sum(v => (decimal?)v.Total) ?? 0;

        return View(ventas);
    }

    public async Task<IActionResult> Productos()
    {
        var productos = await _context.Producto
            .Include(p => p.Categoria)
            .OrderByDescending(p => p.Stock)
            .ToListAsync();

        ViewBag.TotalStock = productos.Sum(p => p.Stock);
        ViewBag.BajoStock = productos.Count(p => p.Stock <= p.StockMinimo);

        return View(productos);
    }

    public async Task<IActionResult> TopProductos()
    {
        var top = await _context.DetalleVentas
            .Include(d => d.Producto)
            .GroupBy(d => d.Producto.Nombre)
            .Select(g => new
            {
                Nombre = g.Key ?? "Desconocido",
                Cantidad = g.Sum(d => d.Cantidad),
                Total = g.Sum(d => d.Cantidad * d.Precio)
            })
            .OrderByDescending(x => x.Cantidad)
            .Take(10)
            .ToListAsync();

        return View(top);
    }
}