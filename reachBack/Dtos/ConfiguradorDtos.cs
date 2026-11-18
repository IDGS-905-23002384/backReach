using System.ComponentModel.DataAnnotations;

namespace reachBack.Dtos;

public class SeleccionPcDto
{
    public int? CpuId { get; set; }
    public int? MotherboardId { get; set; }
    public int? RamId { get; set; }
    public int? GpuId { get; set; }
    public int? CoolerId { get; set; }
    public int? CaseId { get; set; }
    public int? PsuId { get; set; }

    [MaxLength(4, ErrorMessage = "Puedes agregar hasta 4 unidades de almacenamiento.")]
    public List<int> AlmacenamientoIds { get; set; } = new();
}

public class OpcionesRequestDto
{
    [Required]
    [RegularExpression("^(CPU|MOTHERBOARD|RAM|STORAGE|GPU|COOLER|CASE|PSU)$", ErrorMessage = "Paso no válido.")]
    public string Tipo { get; set; } = string.Empty;

    [Required]
    public SeleccionPcDto Seleccion { get; set; } = new();
}

public record PasoConfiguradorDto(string Tipo, string Nombre, int CategoriaId, bool Obligatorio, bool PermiteVarios, string Ayuda);

public record OpcionComponenteDto(ProductoListaDto Producto, bool Compatible, List<string> Motivos, string Resumen);

public record ComponenteSeleccionadoDto(string Tipo, string Paso, ProductoListaDto Producto);

public record ResumenConfiguracionDto(
    List<ComponenteSeleccionadoDto> Componentes,
    List<string> Errores,
    List<string> Advertencias,
    List<string> Pendientes,
    int ConsumoEstimadoW,
    int FuenteRecomendadaW,
    decimal Subtotal,
    decimal CostoEnsamble,
    decimal Total,
    bool Completo);

public record PrearmadoComponenteDto(string Tipo, ProductoListaDto Producto, int Cantidad);

public record PrearmadoDto(
    int Id,
    string Nombre,
    string CategoriaUso,
    string Descripcion,
    string ImagenUrl,
    decimal PrecioComponentes,
    decimal CostoManoObra,
    decimal PrecioTotal,
    bool Disponible,
    List<PrearmadoComponenteDto> Componentes);
