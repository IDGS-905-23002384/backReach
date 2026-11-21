using Microsoft.Extensions.Options;
using reachBack.Dtos;
using reachBack.Infrastructure;
using reachBack.Models;

namespace reachBack.Services;

public class RastreoService
{
    private const int PuntosRuta = 32;
    private const double LeonLatitud = 21.1221;
    private const double LeonLongitud = -101.6827;
    private readonly TiendaOptions _tienda;

    public RastreoService(IOptions<TiendaOptions> tienda)
    {
        _tienda = tienda.Value;
    }

    public RastreoDto? Calcular(Pedido p)
    {
        if (p.Estado == EstadosPedido.Cancelado) return null;
        if (p.MetodoEnvio is { RequiereDireccion: false }) return null;

        var origen = new PuntoDto(_tienda.AlmacenLatitud, _tienda.AlmacenLongitud);
        var aproximado = p.EnvioLatitud is null || p.EnvioLongitud is null;
        var destino = aproximado
            ? new PuntoDto(LeonLatitud, LeonLongitud)
            : new PuntoDto((double)p.EnvioLatitud!.Value, (double)p.EnvioLongitud!.Value);

        var ruta = Curva(origen, destino);
        var (progreso, estado) = Progreso(p);
        var repartidor = PuntoEn(ruta, progreso);
        var destinoNombre = string.IsNullOrWhiteSpace(p.EnvioDireccion) ? "Tu domicilio" : p.EnvioDireccion!;

        return new RastreoDto(estado, Math.Round(progreso, 3), _tienda.AlmacenNombre, origen, destinoNombre, destino, aproximado, repartidor, ruta);
    }

    private static (double Progreso, string Estado) Progreso(Pedido p)
    {
        if (p.EstadoEnvio == EstadosPedido.EnvioEntregado) return (1, "Entregado");
        if (p.EstadoEnvio != EstadosPedido.EnvioEnviado) return (0, p.IncluyeEnsamble && p.EstadoArmado != EstadosPedido.ArmadoTerminado ? "En ensamble" : "En almacén");

        var salida = p.Seguimiento.Where(s => s.Titulo == "Pedido enviado").Select(s => (DateTime?)s.Fecha).FirstOrDefault() ?? p.FechaActualizacion;
        var llegada = p.FechaEstimadaEntrega is { } f && f > salida ? f : salida.AddDays(2);
        var total = (llegada - salida).TotalMinutes;
        var transcurrido = (DateTime.UtcNow - salida).TotalMinutes;
        var progreso = total <= 0 ? 0.5 : Math.Clamp(transcurrido / total, 0.05, 0.92);
        return (progreso, "En camino");
    }

    private static List<PuntoDto> Curva(PuntoDto a, PuntoDto b)
    {
        var dx = b.Longitud - a.Longitud;
        var dy = b.Latitud - a.Latitud;
        var control = new PuntoDto(
            (a.Latitud + b.Latitud) / 2 + dx * 0.18,
            (a.Longitud + b.Longitud) / 2 - dy * 0.18);
        var puntos = new List<PuntoDto>(PuntosRuta + 1);
        for (var i = 0; i <= PuntosRuta; i++)
        {
            var t = i / (double)PuntosRuta;
            var u = 1 - t;
            puntos.Add(new PuntoDto(
                Math.Round(u * u * a.Latitud + 2 * u * t * control.Latitud + t * t * b.Latitud, 6),
                Math.Round(u * u * a.Longitud + 2 * u * t * control.Longitud + t * t * b.Longitud, 6)));
        }
        return puntos;
    }

    private static PuntoDto PuntoEn(List<PuntoDto> ruta, double progreso)
    {
        var posicion = progreso * (ruta.Count - 1);
        var i = (int)Math.Floor(posicion);
        if (i >= ruta.Count - 1) return ruta[^1];
        var f = posicion - i;
        return new PuntoDto(
            Math.Round(ruta[i].Latitud + (ruta[i + 1].Latitud - ruta[i].Latitud) * f, 6),
            Math.Round(ruta[i].Longitud + (ruta[i + 1].Longitud - ruta[i].Longitud) * f, 6));
    }
}
