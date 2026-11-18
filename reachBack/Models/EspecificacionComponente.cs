namespace reachBack.Models;

public class EspecificacionComponente
{
    public int ProductoId { get; set; }
    public Producto? Producto { get; set; }

    public string? Socket { get; set; }
    public string? SocketsSoportados { get; set; }
    public string? TipoMemoria { get; set; }
    public string? FormFactor { get; set; }
    public string? FormFactorsSoportados { get; set; }
    public string? Interfaz { get; set; }

    public int? ConsumoW { get; set; }
    public int? CapacidadW { get; set; }
    public int? FuenteRecomendadaW { get; set; }
    public int? TdpMaxW { get; set; }
    public int? LongitudMm { get; set; }
    public int? LongitudMaxGpuMm { get; set; }
    public int? AlturaMm { get; set; }
    public int? AlturaMaxCoolerMm { get; set; }
    public int? RadiadorMm { get; set; }
    public int? RadiadorMaxMm { get; set; }
    public int? SlotsRam { get; set; }
    public int? ModulosRam { get; set; }
    public int? CapacidadGb { get; set; }
    public int? RanurasM2 { get; set; }

    public bool? GraficosIntegrados { get; set; }
    public bool? IncluyeDisipador { get; set; }
}
