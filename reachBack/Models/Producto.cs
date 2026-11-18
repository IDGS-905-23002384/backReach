using System.ComponentModel.DataAnnotations;

namespace reachBack.Models;

public class Producto
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;

    public decimal Precio { get; set; }
    public decimal? PrecioAnterior { get; set; }

    public int Stock { get; set; }
    public int StockMinimo { get; set; } = 5;
    public string TipoInventario { get; set; } = "Almacen";

    public string ImagenUrl { get; set; } = string.Empty;
    public string EspecificacionesTecnicas { get; set; } = string.Empty;

    public bool Envio { get; set; } = true;
    public bool Activo { get; set; } = true;
    public bool Destacado { get; set; }
    public DateTime FechaAlta { get; set; } = DateTime.UtcNow;

    public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }

    public int? ProveedorId { get; set; }
    public Proveedor? Proveedor { get; set; }

    public EspecificacionComponente? Especificacion { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
