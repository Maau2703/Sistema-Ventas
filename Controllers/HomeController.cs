using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using SistemaVentas.Models;
using SistemaVentas.Data;
using SistemaVentas.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace SistemaVentas.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AppDbContext _context;

    public HomeController(ILogger<HomeController> logger, AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var hoy = DateTime.Today;

        // Consultas con conversión explícita a int
        var totalVentas = (int)await _context.Ventas.CountAsync();
        var totalProductos = (int)await _context.Producto.CountAsync();
        var totalClientes = (int)await _context.Clientes.CountAsync();
        var productosBajoStock = (int)await _context.Producto.CountAsync(p => p.Stock <= p.StockMinimo);
        var ventasHoy = (int)await _context.Ventas
            .Where(v => v.Fecha >= hoy && v.Fecha < hoy.AddDays(1))
            .CountAsync();

        var viewModel = new DashboardViewModel
        {
            TotalVentas = await _context.Ventas.SumAsync(v => (decimal?)v.Total) ?? 0,
            TotalProductos = totalProductos,
            TotalClientes = totalClientes,
            ProductosBajoStock = productosBajoStock,
            TotalVentasHoy = ventasHoy,

            TopProductos = await _context.DetalleVentas
                .Include(d => d.Producto)
                .GroupBy(d => d.Producto.Nombre)
                .Select(g => new ProductoTopViewModel
                {
                    Nombre = g.Key ?? "Sin nombre",
                    CantidadVendida = (int)g.Sum(d => d.Cantidad),
                    TotalVendido = g.Sum(d => d.Cantidad * d.Precio)
                })
                .OrderByDescending(x => x.CantidadVendida)
                .Take(5)
                .ToListAsync(),

            AlertasStock = await _context.Producto
                .Include(p => p.Categoria)
                .Where(p => p.Stock <= p.StockMinimo)
                .Select(p => new ProductoStockBajoViewModel
                {
                    Nombre = p.Nombre ?? "Sin nombre",
                    StockActual = p.Stock ?? 0,
                    StockMinimo = p.StockMinimo ?? 0,
                    Categoria = p.Categoria != null ? p.Categoria.Nombre : "Sin categoría"
                })
                .ToListAsync(),

            UltimasVentas = await _context.Ventas
                .Include(v => v.Cliente)
                .OrderByDescending(v => v.Fecha)
                .Take(5)
                .Select(v => new UltimaVentaViewModel
                {
                    Id = v.Id,
                    Fecha = v.Fecha,
                    Cliente = v.Cliente != null ? v.Cliente.Nombre : "Cliente desconocido",
                    Total = v.Total
                })
                .ToListAsync()
        };

        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}