namespace SistemaVentas.Models;
using System.Text.Json.Serialization;

public class Categoria
{
    public int Id { get; set; }
    public string? Nombre { get; set; } = string.Empty;
    [JsonIgnore]
    public List<Producto> Productos { get; set; } = new();

    //hola
}
