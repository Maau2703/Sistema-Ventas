using SistemaVentas.Models;

namespace SistemaVentas.ViewModels;

public class DashboardViewModel
{
    public decimal TotalVentas { get; set; }
    public int TotalProductos { get; set; }
    public int TotalClientes { get; set; }
    public int ProductosBajoStock { get; set; }
    public int TotalVentasHoy { get; set; }

    public List<ProductoTopViewModel> TopProductos { get; set; } = new();
    public List<ProductoStockBajoViewModel> AlertasStock { get; set; } = new();
    public List<UltimaVentaViewModel> UltimasVentas { get; set; } = new();
}

public class ProductoTopViewModel
{
    public string Nombre { get; set; } = string.Empty;
    public int CantidadVendida { get; set; }
    public decimal TotalVendido { get; set; }
}

public class ProductoStockBajoViewModel
{
    public string Nombre { get; set; } = string.Empty;
    public int StockActual { get; set; }
    public int StockMinimo { get; set; }
    public string? Categoria { get; set; }
}

public class UltimaVentaViewModel
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public string? Cliente { get; set; }
    public decimal Total { get; set; }
}