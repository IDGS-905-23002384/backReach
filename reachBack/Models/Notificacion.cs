namespace reachBack.Models;

public class Notificacion
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string Tipo { get; set; } = TiposNotificacion.Pedido;
    public int? PedidoId { get; set; }
    public bool Leida { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
}

public static class TiposNotificacion
{
    public const string Pedido = "Pedido";
    public const string Inventario = "Inventario";
    public const string Sistema = "Sistema";
}
