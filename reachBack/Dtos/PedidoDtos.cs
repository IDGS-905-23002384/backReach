using System.ComponentModel.DataAnnotations;

namespace reachBack.Dtos;

public record DireccionDto(
    int Id,
    string Alias,
    string NombreRecibe,
    string Telefono,
    string Calle,
    string NumeroExterior,
    string? NumeroInterior,
    string Colonia,
    string Ciudad,
    string Estado,
    string CodigoPostal,
    string? Referencias,
    bool EsPredeterminada);

public class DireccionGuardarDto
{
    [StringLength(40)]
    public string Alias { get; set; } = "Casa";

    [Required(ErrorMessage = "Indica quién recibe.")]
    [StringLength(120, MinimumLength = 2)]
    public string NombreRecibe { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "El teléfono debe tener 10 dígitos.")]
    public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "La calle es obligatoria.")]
    [StringLength(150)]
    public string Calle { get; set; } = string.Empty;

    [Required(ErrorMessage = "El número exterior es obligatorio.")]
    [StringLength(20)]
    public string NumeroExterior { get; set; } = string.Empty;

    [StringLength(20)]
    public string? NumeroInterior { get; set; }

    [Required(ErrorMessage = "La colonia es obligatoria.")]
    [StringLength(120)]
    public string Colonia { get; set; } = string.Empty;

    [Required(ErrorMessage = "La ciudad es obligatoria.")]
    [StringLength(120)]
    public string Ciudad { get; set; } = string.Empty;

    [Required(ErrorMessage = "El estado es obligatorio.")]
    [StringLength(80)]
    public string Estado { get; set; } = string.Empty;

    [Required(ErrorMessage = "El código postal es obligatorio.")]
    [RegularExpression(@"^\d{5}$", ErrorMessage = "El código postal debe tener 5 dígitos.")]
    public string CodigoPostal { get; set; } = string.Empty;

    [StringLength(250)]
    public string? Referencias { get; set; }

    public bool EsPredeterminada { get; set; }
}

public record MetodoEnvioDto(
    int Id,
    string Nombre,
    string Descripcion,
    string Paqueteria,
    decimal Costo,
    decimal CostoAplicado,
    int DiasMin,
    int DiasMax,
    bool RequiereDireccion,
    bool EsGratis);

public class PagoDto
{
    [Required(ErrorMessage = "Indica el titular de la tarjeta.")]
    [StringLength(120, MinimumLength = 3)]
    public string Titular { get; set; } = string.Empty;

    [Required(ErrorMessage = "El número de tarjeta es obligatorio.")]
    [RegularExpression(@"^\d{15,16}$", ErrorMessage = "El número de tarjeta no es válido.")]
    public string NumeroTarjeta { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de vencimiento es obligatoria.")]
    [RegularExpression(@"^(0[1-9]|1[0-2])\/\d{2}$", ErrorMessage = "Usa el formato MM/AA.")]
    public string Vencimiento { get; set; } = string.Empty;

    [Required(ErrorMessage = "El CVV es obligatorio.")]
    [RegularExpression(@"^\d{3,4}$", ErrorMessage = "El CVV no es válido.")]
    public string Cvv { get; set; } = string.Empty;
}

public class CheckoutDto
{
    public int? DireccionId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un método de envío.")]
    public int MetodoEnvioId { get; set; }

    [Required]
    public PagoDto Pago { get; set; } = new();

    [StringLength(500)]
    public string? Notas { get; set; }
}

public record PedidoResumenDto(
    int Id,
    string Folio,
    DateTime Fecha,
    decimal Total,
    string Estado,
    string EstadoArmado,
    string EstadoEnvio,
    bool IncluyeEnsamble,
    int Articulos,
    string ImagenUrl,
    string Cliente);

public record PedidoDetalleItemDto(
    int ProductoId,
    string Sku,
    string Nombre,
    string ImagenUrl,
    int Cantidad,
    decimal PrecioUnitario,
    decimal Subtotal,
    bool EsEnsamble,
    string? EnsambleId,
    string? EnsambleNombre);

public record SeguimientoDto(string Titulo, string Descripcion, DateTime Fecha);

public record PedidoDetalleDto(
    int Id,
    string Folio,
    DateTime Fecha,
    DateTime FechaActualizacion,
    string Estado,
    string EstadoArmado,
    string EstadoEnvio,
    bool IncluyeEnsamble,
    decimal Subtotal,
    decimal CostoManoObra,
    decimal CostoEnvio,
    decimal Iva,
    decimal Total,
    string MetodoEnvio,
    bool RequiereDireccion,
    string? Paqueteria,
    string? NumeroGuia,
    DateTime? FechaEstimadaEntrega,
    string? EnvioNombreRecibe,
    string? EnvioTelefono,
    string? EnvioDireccion,
    string? EnvioCiudad,
    string? EnvioEstado,
    string? EnvioCodigoPostal,
    string? EnvioReferencias,
    string MetodoPago,
    string? Tarjeta,
    string? Notas,
    string Cliente,
    string ClienteEmail,
    bool PuedeCancelar,
    List<PedidoDetalleItemDto> Detalles,
    List<SeguimientoDto> Seguimiento);

public class ActualizarArmadoDto
{
    [Required]
    [RegularExpression("^(Atendido|Ensamblando|Terminado)$", ErrorMessage = "Estado de armado no válido.")]
    public string Estado { get; set; } = string.Empty;
}

public class ActualizarEnvioDto
{
    [Required]
    [RegularExpression("^(Preparando paquete|Enviado|Entregado)$", ErrorMessage = "Estado de envío no válido.")]
    public string Estado { get; set; } = string.Empty;

    [StringLength(80)]
    public string? Paqueteria { get; set; }

    [StringLength(60)]
    [RegularExpression("^[A-Za-z0-9-]*$", ErrorMessage = "La guía solo admite letras, números y guiones.")]
    public string? NumeroGuia { get; set; }
}

public record ResumenPanelDto(
    int PedidosHoy,
    decimal VentasHoy,
    decimal VentasMes,
    int PendientesEnvio,
    int EnEnsamble,
    int Enviados,
    int ProductosStockBajo,
    int ProductosActivos,
    int Clientes,
    List<PedidoResumenDto> UltimosPedidos);
