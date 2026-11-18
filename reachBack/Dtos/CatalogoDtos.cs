using System.ComponentModel.DataAnnotations;

namespace reachBack.Dtos;

public record CategoriaDto(int Id, string Nombre, string Slug, string? TipoComponente, int Orden, int TotalProductos, string? ImagenUrl);

public class CategoriaGuardarDto
{
    [Required, StringLength(80, MinimumLength = 2)]
    public string Nombre { get; set; } = string.Empty;

    [RegularExpression("^(CPU|MOTHERBOARD|RAM|STORAGE|GPU|COOLER|CASE|PSU)$", ErrorMessage = "Tipo de componente no válido.")]
    public string? TipoComponente { get; set; }

    [Range(0, 999)]
    public int Orden { get; set; }
}

public record ProductoListaDto(
    int Id,
    string Sku,
    string Nombre,
    string Marca,
    decimal Precio,
    decimal? PrecioAnterior,
    int DescuentoPorcentaje,
    int Stock,
    string ImagenUrl,
    int CategoriaId,
    string CategoriaNombre,
    string? TipoComponente,
    bool Destacado,
    bool Activo,
    bool Envio);

public record EspecificacionLineaDto(string Nombre, string Valor);

public class EspecificacionComponenteDto
{
    [StringLength(20)] public string? Socket { get; set; }
    [StringLength(100)] public string? SocketsSoportados { get; set; }
    [StringLength(10)] public string? TipoMemoria { get; set; }
    [StringLength(20)] public string? FormFactor { get; set; }
    [StringLength(60)] public string? FormFactorsSoportados { get; set; }
    [StringLength(20)] public string? Interfaz { get; set; }
    [Range(0, 2000)] public int? ConsumoW { get; set; }
    [Range(0, 3000)] public int? CapacidadW { get; set; }
    [Range(0, 3000)] public int? FuenteRecomendadaW { get; set; }
    [Range(0, 1000)] public int? TdpMaxW { get; set; }
    [Range(0, 600)] public int? LongitudMm { get; set; }
    [Range(0, 600)] public int? LongitudMaxGpuMm { get; set; }
    [Range(0, 300)] public int? AlturaMm { get; set; }
    [Range(0, 300)] public int? AlturaMaxCoolerMm { get; set; }
    [Range(0, 480)] public int? RadiadorMm { get; set; }
    [Range(0, 480)] public int? RadiadorMaxMm { get; set; }
    [Range(0, 16)] public int? SlotsRam { get; set; }
    [Range(0, 16)] public int? ModulosRam { get; set; }
    [Range(0, 100000)] public int? CapacidadGb { get; set; }
    [Range(0, 10)] public int? RanurasM2 { get; set; }
    public bool? GraficosIntegrados { get; set; }
    public bool? IncluyeDisipador { get; set; }
}

public record ProductoDetalleDto(
    int Id,
    string Sku,
    string Nombre,
    string Marca,
    string Descripcion,
    decimal Precio,
    decimal? PrecioAnterior,
    int DescuentoPorcentaje,
    int Stock,
    int StockMinimo,
    string ImagenUrl,
    int CategoriaId,
    string CategoriaNombre,
    string? TipoComponente,
    bool Destacado,
    bool Activo,
    bool Envio,
    int? ProveedorId,
    string? ProveedorNombre,
    string EspecificacionesTecnicas,
    List<EspecificacionLineaDto> Especificaciones,
    EspecificacionComponenteDto? Especificacion);

public class ProductoGuardarDto
{
    [Required(ErrorMessage = "El SKU es obligatorio.")]
    [StringLength(40, MinimumLength = 3)]
    [RegularExpression("^[A-Za-z0-9-]+$", ErrorMessage = "El SKU solo admite letras, números y guiones.")]
    public string Sku { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(200, MinimumLength = 3)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La marca es obligatoria.")]
    [StringLength(80)]
    public string Marca { get; set; } = string.Empty;

    [StringLength(2000)]
    public string Descripcion { get; set; } = string.Empty;

    [Range(1, 999999, ErrorMessage = "El precio debe ser mayor a cero.")]
    public decimal Precio { get; set; }

    [Range(0, 999999)]
    public decimal? PrecioAnterior { get; set; }

    [Range(0, 100000)]
    public int Stock { get; set; }

    [Range(0, 10000)]
    public int StockMinimo { get; set; } = 5;

    [StringLength(500)]
    public string ImagenUrl { get; set; } = string.Empty;

    [StringLength(4000)]
    public string EspecificacionesTecnicas { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Selecciona una categoría.")]
    public int CategoriaId { get; set; }

    public int? ProveedorId { get; set; }
    public bool Envio { get; set; } = true;
    public bool Destacado { get; set; }
    public bool Activo { get; set; } = true;
    public EspecificacionComponenteDto? Especificacion { get; set; }
}

public class ProductoFiltroDto
{
    public int? CategoriaId { get; set; }

    [StringLength(80)]
    public string? Q { get; set; }

    [StringLength(80)]
    public string? Marca { get; set; }

    [RegularExpression("^(relevancia|precio_asc|precio_desc|nombre|recientes|descuento)$")]
    public string? Orden { get; set; }

    public bool SoloOfertas { get; set; }
    public bool SoloDestacados { get; set; }
    public bool IncluirInactivos { get; set; }

    [Range(1, 10000)]
    public int Pagina { get; set; } = 1;

    [Range(1, 100)]
    public int Tamano { get; set; } = 20;
}

public record PaginaDto<T>(List<T> Items, int Pagina, int Tamano, int Total, int TotalPaginas);

public record ProveedorDto(int Id, string NombreEmpresa, string ContactoPersona, string Telefono, string Email, bool Activo, int TotalProductos);

public class ProveedorGuardarDto
{
    [Required, StringLength(150, MinimumLength = 2)]
    public string NombreEmpresa { get; set; } = string.Empty;

    [StringLength(120)]
    public string ContactoPersona { get; set; } = string.Empty;

    [RegularExpression(@"^\d{10}$", ErrorMessage = "El teléfono debe tener 10 dígitos.")]
    public string? Telefono { get; set; }

    [EmailAddress, StringLength(160)]
    public string? Email { get; set; }

    public bool Activo { get; set; } = true;
}
