namespace reachBack.Models;

public class Pedido
{
    public int Id { get; set; }
    public string Folio { get; set; } = string.Empty;

    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

    public decimal Subtotal { get; set; }
    public decimal CostoManoObra { get; set; }
    public decimal CostoEnvio { get; set; }
    public decimal Iva { get; set; }
    public decimal Total { get; set; }

    public string Estado { get; set; } = EstadosPedido.Pagado;
    public string EstadoArmado { get; set; } = EstadosPedido.ArmadoNoAplica;
    public string EstadoEnvio { get; set; } = EstadosPedido.EnvioPreparando;
    public bool IncluyeEnsamble { get; set; }

    public int MetodoEnvioId { get; set; }
    public MetodoEnvio? MetodoEnvio { get; set; }
    public string? Paqueteria { get; set; }
    public string? NumeroGuia { get; set; }
    public DateTime? FechaEstimadaEntrega { get; set; }

    public string? EnvioNombreRecibe { get; set; }
    public string? EnvioTelefono { get; set; }
    public string? EnvioDireccion { get; set; }
    public string? EnvioCiudad { get; set; }
    public string? EnvioEstado { get; set; }
    public string? EnvioCodigoPostal { get; set; }
    public string? EnvioReferencias { get; set; }
    public decimal? EnvioLatitud { get; set; }
    public decimal? EnvioLongitud { get; set; }

    public int? SucursalId { get; set; }
    public Sucursal? Sucursal { get; set; }

    public string MetodoPago { get; set; } = "Tarjeta";
    public string? TarjetaMarca { get; set; }
    public string? TarjetaUltimos4 { get; set; }
    public string? Notas { get; set; }

    public List<PedidoDetalle> Detalles { get; set; } = new();
    public List<PedidoSeguimiento> Seguimiento { get; set; } = new();
}
