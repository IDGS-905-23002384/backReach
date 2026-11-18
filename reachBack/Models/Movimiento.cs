namespace reachBack.Models;

public class Movimiento
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public Producto? Producto { get; set; }

    public string TipoMovimiento { get; set; } = "Entrada";
    public int Cantidad { get; set; }
    public int StockResultante { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public string Motivo { get; set; } = string.Empty;
    public int? PedidoId { get; set; }
    public string AdministradorResponsable { get; set; } = string.Empty;
}
