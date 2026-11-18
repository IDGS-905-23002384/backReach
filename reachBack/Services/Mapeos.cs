using reachBack.Dtos;
using reachBack.Models;

namespace reachBack.Services;

public static class Mapeos
{
    public static int Descuento(decimal precio, decimal? anterior)
    {
        if (anterior is null || anterior <= precio || anterior <= 0) return 0;
        return (int)Math.Round((anterior.Value - precio) / anterior.Value * 100m, MidpointRounding.AwayFromZero);
    }

    public static ProductoListaDto ALista(this Producto p) => new(
        p.Id,
        p.Sku,
        p.Nombre,
        p.Marca,
        p.Precio,
        p.PrecioAnterior,
        Descuento(p.Precio, p.PrecioAnterior),
        p.Stock,
        p.ImagenUrl,
        p.CategoriaId,
        p.Categoria?.Nombre ?? string.Empty,
        p.Categoria?.TipoComponente,
        p.Destacado,
        p.Activo,
        p.Envio);

    public static List<EspecificacionLineaDto> Lineas(string texto)
    {
        var lista = new List<EspecificacionLineaDto>();
        if (string.IsNullOrWhiteSpace(texto)) return lista;
        foreach (var linea in texto.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var idx = linea.IndexOf(':');
            if (idx > 0)
            {
                lista.Add(new EspecificacionLineaDto(linea[..idx].Trim(), linea[(idx + 1)..].Trim()));
            }
            else
            {
                lista.Add(new EspecificacionLineaDto("Detalle", linea));
            }
        }
        return lista;
    }

    public static EspecificacionComponenteDto? ADto(this EspecificacionComponente? e)
    {
        if (e is null) return null;
        return new EspecificacionComponenteDto
        {
            Socket = e.Socket,
            SocketsSoportados = e.SocketsSoportados,
            TipoMemoria = e.TipoMemoria,
            FormFactor = e.FormFactor,
            FormFactorsSoportados = e.FormFactorsSoportados,
            Interfaz = e.Interfaz,
            ConsumoW = e.ConsumoW,
            CapacidadW = e.CapacidadW,
            FuenteRecomendadaW = e.FuenteRecomendadaW,
            TdpMaxW = e.TdpMaxW,
            LongitudMm = e.LongitudMm,
            LongitudMaxGpuMm = e.LongitudMaxGpuMm,
            AlturaMm = e.AlturaMm,
            AlturaMaxCoolerMm = e.AlturaMaxCoolerMm,
            RadiadorMm = e.RadiadorMm,
            RadiadorMaxMm = e.RadiadorMaxMm,
            SlotsRam = e.SlotsRam,
            ModulosRam = e.ModulosRam,
            CapacidadGb = e.CapacidadGb,
            RanurasM2 = e.RanurasM2,
            GraficosIntegrados = e.GraficosIntegrados,
            IncluyeDisipador = e.IncluyeDisipador
        };
    }

    public static void Aplicar(this EspecificacionComponente e, EspecificacionComponenteDto d)
    {
        e.Socket = Limpiar(d.Socket);
        e.SocketsSoportados = Limpiar(d.SocketsSoportados);
        e.TipoMemoria = Limpiar(d.TipoMemoria)?.ToUpperInvariant();
        e.FormFactor = Limpiar(d.FormFactor);
        e.FormFactorsSoportados = Limpiar(d.FormFactorsSoportados);
        e.Interfaz = Limpiar(d.Interfaz);
        e.ConsumoW = d.ConsumoW;
        e.CapacidadW = d.CapacidadW;
        e.FuenteRecomendadaW = d.FuenteRecomendadaW;
        e.TdpMaxW = d.TdpMaxW;
        e.LongitudMm = d.LongitudMm;
        e.LongitudMaxGpuMm = d.LongitudMaxGpuMm;
        e.AlturaMm = d.AlturaMm;
        e.AlturaMaxCoolerMm = d.AlturaMaxCoolerMm;
        e.RadiadorMm = d.RadiadorMm;
        e.RadiadorMaxMm = d.RadiadorMaxMm;
        e.SlotsRam = d.SlotsRam;
        e.ModulosRam = d.ModulosRam;
        e.CapacidadGb = d.CapacidadGb;
        e.RanurasM2 = d.RanurasM2;
        e.GraficosIntegrados = d.GraficosIntegrados;
        e.IncluyeDisipador = d.IncluyeDisipador;
    }

    public static UsuarioDto ADto(this Usuario u) => new(u.Id, u.Nombre, u.Email, u.Telefono, u.Rol);

    public static DireccionDto ADto(this Direccion d) => new(
        d.Id, d.Alias, d.NombreRecibe, d.Telefono, d.Calle, d.NumeroExterior, d.NumeroInterior,
        d.Colonia, d.Ciudad, d.Estado, d.CodigoPostal, d.Referencias, d.EsPredeterminada);

    private static string? Limpiar(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
