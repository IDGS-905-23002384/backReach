namespace reachBack.Models;

public class Categoria
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? TipoComponente { get; set; }
    public int Orden { get; set; }

    public List<Producto> Productos { get; set; } = new();
}
