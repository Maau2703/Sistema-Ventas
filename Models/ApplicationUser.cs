using Microsoft.AspNetCore.Identity;

namespace SistemaVentas.Models;

// Heredamos de IdentityUser para obtener todas las propiedades de seguridad (Id, PasswordHash, etc.)
public class ApplicationUser : IdentityUser
{
    // Puedes agregar campos personalizados si lo deseas
    public string? NombreCompleto { get; set; }
}