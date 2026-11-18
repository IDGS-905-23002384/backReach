namespace reachBack.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Rol { get; set; } = Roles.Comprador;
    public bool Activo { get; set; } = true;
    public int IntentosFallidos { get; set; }
    public DateTime? BloqueadoHasta { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public List<Pedido> Pedidos { get; set; } = new();
    public List<CarritoItem> CarritoItems { get; set; } = new();
    public List<Direccion> Direcciones { get; set; } = new();
}
