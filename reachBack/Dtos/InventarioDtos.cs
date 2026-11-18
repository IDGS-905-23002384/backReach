using System.ComponentModel.DataAnnotations;

namespace reachBack.Dtos;

public record MovimientoDto(
    int Id,
    int ProductoId,
    string ProductoNombre,
    string Sku,
    string TipoMovimiento,
    int Cantidad,
    int StockResultante,
    string Motivo,
    int? PedidoId,
    string Responsable,
    DateTime Fecha);

public class MovimientoCrearDto
{
    [Range(1, int.MaxValue)]
    public int ProductoId { get; set; }

    [Required]
    [RegularExpression("^(Entrada|Salida)$", ErrorMessage = "El tipo debe ser Entrada o Salida.")]
    public string TipoMovimiento { get; set; } = "Entrada";

    [Range(1, 10000, ErrorMessage = "La cantidad debe estar entre 1 y 10,000.")]
    public int Cantidad { get; set; }

    [Required(ErrorMessage = "Indica el motivo.")]
    [StringLength(200, MinimumLength = 3)]
    public string Motivo { get; set; } = string.Empty;
}

public record AlertaStockDto(int ProductoId, string Sku, string Nombre, string ImagenUrl, int Stock, int StockMinimo, string Categoria);
