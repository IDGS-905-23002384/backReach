namespace reachBack.Models;

public class PedidoDetalle
{
    public int Id { get; set; }
    public int PedidoId { get; set; }
    public Pedido? Pedido { get; set; }

    public int ProductoId { get; set; }
    public Producto? Producto { get; set; }

    public string Sku { get; set; } = string.Empty;
    public string NombreProducto { get; set; } = string.Empty;
    public string ImagenUrl { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public bool EsEnsamble { get; set; }
    public string? EnsambleId { get; set; }
    public string? EnsambleNombre { get; set; }
}
