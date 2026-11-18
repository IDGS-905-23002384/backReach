using System.ComponentModel.DataAnnotations;

namespace reachBack.Dtos;

public class AgregarCarritoDto
{
    [Range(1, int.MaxValue)]
    public int ProductoId { get; set; }

    [Range(1, 20, ErrorMessage = "La cantidad debe estar entre 1 y 20.")]
    public int Cantidad { get; set; } = 1;
}

public class ActualizarCantidadDto
{
    [Range(1, 20, ErrorMessage = "La cantidad debe estar entre 1 y 20.")]
    public int Cantidad { get; set; }
}

public class AgregarEnsambleDto
{
    [Required]
    public SeleccionPcDto Seleccion { get; set; } = new();

    [StringLength(60)]
    public string? Nombre { get; set; }
}

public record CarritoItemDto(
    int Id,
    int ProductoId,
    string Sku,
    string Nombre,
    string Marca,
    string ImagenUrl,
    decimal Precio,
    int Cantidad,
    int Stock,
    decimal Subtotal,
    bool EsEnsamble,
    string? EnsambleId,
    string? EnsambleNombre);

public record CarritoDto(
    List<CarritoItemDto> Items,
    int TotalArticulos,
    int TotalEnsambles,
    decimal SubtotalProductos,
    decimal CostoEnsamble,
    decimal Total,
    decimal EnvioGratisDesde,
    decimal FaltaParaEnvioGratis,
    List<string> Avisos);
