using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using reachBack.Data;
using reachBack.Dtos;
using reachBack.Infrastructure;
using reachBack.Models;

namespace reachBack.Services;

public class RecomendadorService
{
    private sealed record PerfilUso(
        string Clave,
        string Nombre,
        string Descripcion,
        decimal PresupuestoSugerido,
        bool GpuObligatoria,
        double GpuMinima,
        int RamMinimaGb,
        int AlmacenamientoMinimoGb,
        bool RequiereNvme,
        Dictionary<string, (double Peso, double Tope)> Pesos);

    private sealed record Candidato(ArmadoPc Armado, decimal Total, double Puntaje, string Firma);

    private static readonly PerfilUso[] Perfiles =
    {
        new("OFICINA", "Oficina y escuela", "Ofimática, clases en línea, navegación y multitarea ligera.", 15000m,
            false, 0, 16, 480, false,
            new()
            {
                [TiposComponente.Cpu] = (0.45, 0.45),
                [TiposComponente.Ram] = (0.25, 0.5),
                [TiposComponente.Almacenamiento] = (0.30, 0.5),
                [TiposComponente.Gpu] = (0, 0)
            }),
        new("GAMING_FHD", "Gaming 1080p", "Juegos competitivos y AAA en Full HD a más de 60 FPS.", 22000m,
            true, 0, 16, 1000, true,
            new()
            {
                [TiposComponente.Cpu] = (0.25, 0.6),
                [TiposComponente.Gpu] = (0.55, 0.5),
                [TiposComponente.Ram] = (0.10, 0.5),
                [TiposComponente.Almacenamiento] = (0.10, 0.5)
            }),
        new("GAMING_QHD", "Gaming 1440p", "Juegos AAA en QHD con detalles altos y monitores de alta frecuencia.", 38000m,
            true, 0.35, 32, 1000, true,
            new()
            {
                [TiposComponente.Cpu] = (0.25, 0.85),
                [TiposComponente.Gpu] = (0.55, 1),
                [TiposComponente.Ram] = (0.10, 0.55),
                [TiposComponente.Almacenamiento] = (0.10, 0.7)
            }),
        new("CREACION", "Edición y creación", "Edición de video, diseño 3D, streaming y programación pesada.", 40000m,
            true, 0, 32, 1000, true,
            new()
            {
                [TiposComponente.Cpu] = (0.40, 1),
                [TiposComponente.Gpu] = (0.25, 0.7),
                [TiposComponente.Ram] = (0.20, 1),
                [TiposComponente.Almacenamiento] = (0.15, 1)
            })
    };

    private readonly ApplicationDbContext _db;
    private readonly ConfiguradorService _configurador;
    private readonly TiendaOptions _tienda;

    public RecomendadorService(ApplicationDbContext db, ConfiguradorService configurador, IOptions<TiendaOptions> tienda)
    {
        _db = db;
        _configurador = configurador;
        _tienda = tienda.Value;
    }

    public static List<UsoEquipoDto> Usos() =>
        Perfiles.Select(p => new UsoEquipoDto(p.Clave, p.Nombre, p.Descripcion, p.PresupuestoSugerido)).ToList();

    public async Task<RecomendacionesDto> RecomendarAsync(RecomendacionRequestDto req)
    {
        var perfil = Perfiles.FirstOrDefault(p => p.Clave == req.Uso) ?? throw new AppException("Uso no válido.");

        var productos = await _db.Productos.AsNoTracking()
            .Include(p => p.Categoria)
            .Include(p => p.Especificacion)
            .Where(p => p.Activo && p.Stock > 0 && p.Categoria!.TipoComponente != null)
            .ToListAsync();

        var porTipo = productos
            .GroupBy(p => p.Categoria!.TipoComponente!)
            .ToDictionary(g => g.Key, g => g.OrderBy(p => p.Precio).ToList());

        List<Producto> De(string tipo) => porTipo.TryGetValue(tipo, out var l) ? l : new List<Producto>();
        var maximos = porTipo.ToDictionary(k => k.Key, v => v.Value.Max(p => p.Precio));
        double Rendimiento(Producto? p, string tipo) =>
            p is null || !maximos.TryGetValue(tipo, out var max) || max <= 0 ? 0 : (double)(p.Precio / max);

        var ensamble = _tienda.CostoEnsamble;

        var rams = De(TiposComponente.Ram).Where(r => (r.Especificacion?.CapacidadGb ?? 0) >= perfil.RamMinimaGb).ToList();
        var discos = De(TiposComponente.Almacenamiento)
            .Where(d => (d.Especificacion?.CapacidadGb ?? 0) >= perfil.AlmacenamientoMinimoGb)
            .Where(d => !perfil.RequiereNvme || (d.Especificacion?.Interfaz ?? string.Empty).Contains("NVMe", StringComparison.OrdinalIgnoreCase))
            .ToList();
        var gpus = De(TiposComponente.Gpu).Where(g => Rendimiento(g, TiposComponente.Gpu) >= perfil.GpuMinima).ToList();
        var coolers = De(TiposComponente.Enfriamiento);
        var gabinetes = De(TiposComponente.Gabinete);
        var fuentes = De(TiposComponente.Fuente);

        var idealTotal = perfil.Pesos.Values.Sum(v => v.Peso * v.Tope);
        double Puntuar(ArmadoPc a)
        {
            double Parte(string tipo, Producto? p)
            {
                var (peso, tope) = perfil.Pesos.TryGetValue(tipo, out var v) ? v : (0d, 0d);
                return peso * Math.Min(Rendimiento(p, tipo), tope);
            }
            var suma = Parte(TiposComponente.Cpu, a.Cpu) + Parte(TiposComponente.Gpu, a.Gpu)
                + Parte(TiposComponente.Ram, a.Ram) + Parte(TiposComponente.Almacenamiento, a.Almacenamiento.FirstOrDefault());
            var (pesoGpu, topeGpu) = perfil.Pesos[TiposComponente.Gpu];
            var (_, topeCpu) = perfil.Pesos[TiposComponente.Cpu];
            if (a.Gpu is not null && pesoGpu > 0)
            {
                var nivelCpu = Math.Min(Rendimiento(a.Cpu, TiposComponente.Cpu), topeCpu) / topeCpu;
                var nivelGpu = Math.Min(Rendimiento(a.Gpu, TiposComponente.Gpu), topeGpu) / topeGpu;
                suma *= 1 - 0.6 * Math.Max(0, Math.Abs(nivelGpu - nivelCpu) - 0.3);
            }
            return idealTotal <= 0 ? 0 : Math.Round(suma / idealTotal * 100, 2);
        }

        var soportes = new Dictionary<string, (Producto? Cooler, Producto Gabinete)?>();
        var candidatos = new List<Candidato>();

        foreach (var cpu in De(TiposComponente.Cpu))
        {
            var cpuSpec = cpu.Especificacion ?? new EspecificacionComponente();
            var opcionesGpu = new List<Producto?>();
            if (!perfil.GpuObligatoria && cpuSpec.GraficosIntegrados == true) opcionesGpu.Add(null);
            if (perfil.GpuObligatoria || cpuSpec.GraficosIntegrados != true)
            {
                opcionesGpu.AddRange(perfil.GpuObligatoria ? gpus : De(TiposComponente.Gpu));
            }

            foreach (var mb in De(TiposComponente.TarjetaMadre))
            {
                var baseCpuMb = new ArmadoPc { Cpu = cpu, Motherboard = mb };
                if (ConfiguradorService.Evaluar(baseCpuMb).Count > 0) continue;

                foreach (var gpu in opcionesGpu)
                {
                    var clave = $"{cpu.Id}-{mb.Id}-{gpu?.Id ?? 0}";
                    if (!soportes.TryGetValue(clave, out var soporte))
                    {
                        soporte = ElegirSoporte(cpu, mb, gpu, coolers, gabinetes);
                        soportes[clave] = soporte;
                    }
                    if (soporte is null) continue;

                    foreach (var ram in rams)
                    {
                        foreach (var disco in discos)
                        {
                            var a = new ArmadoPc
                            {
                                Cpu = cpu,
                                Motherboard = mb,
                                Ram = ram,
                                Gpu = gpu,
                                Cooler = soporte.Value.Cooler,
                                Case = soporte.Value.Gabinete
                            };
                            a.Almacenamiento.Add(disco);
                            if (ConfiguradorService.Evaluar(a).Count > 0) continue;

                            var requerida = ConfiguradorService.FuenteRecomendada(a);
                            a.Psu = fuentes.FirstOrDefault(f => (f.Especificacion?.CapacidadW ?? 0) >= requerida);
                            if (a.Psu is null || ConfiguradorService.Evaluar(a).Count > 0) continue;

                            var total = a.Componentes().Sum(c => c.Producto.Precio) + ensamble;
                            var firma = string.Join(",", a.Componentes().Select(c => c.Producto.Id));
                            candidatos.Add(new Candidato(a, total, Puntuar(a), firma));
                        }
                    }
                }
            }
        }

        if (candidatos.Count == 0)
        {
            throw new AppException("Por ahora no hay componentes en existencia para armar un equipo de este tipo.");
        }

        var minimo = candidatos.Min(c => c.Total);
        if (req.Presupuesto < minimo)
        {
            throw new AppException($"Con ese presupuesto no alcanza para un equipo de {perfil.Nombre.ToLowerInvariant()}. El mínimo con el inventario actual es {Dinero(minimo)}.");
        }

        Candidato? Mejor(decimal tope) => candidatos
            .Where(c => c.Total <= tope)
            .OrderByDescending(c => c.Puntaje)
            .ThenBy(c => c.Total)
            .FirstOrDefault();

        var niveles = new List<(string Nivel, string Titulo, Candidato? Elegido)>
        {
            ("ECONOMICA", "Económica", Mejor(Math.Max(minimo, req.Presupuesto * 0.7m))),
            ("EQUILIBRADA", "Equilibrada", Mejor(Math.Max(minimo, req.Presupuesto * 0.85m))),
            ("MAXIMA", "Máximo rendimiento", Mejor(req.Presupuesto))
        };

        var opciones = new List<RecomendacionDto>();
        var vistas = new HashSet<string>();
        foreach (var (nivel, titulo, elegido) in niveles.AsEnumerable().Reverse())
        {
            if (elegido is null || !vistas.Add(elegido.Firma)) continue;
            var resumen = _configurador.Resumir(elegido.Armado);
            if (!resumen.Completo) continue;
            opciones.Add(new RecomendacionDto(
                nivel,
                titulo,
                Describir(nivel, perfil, elegido, req.Presupuesto),
                (int)Math.Round(elegido.Puntaje),
                req.Presupuesto - resumen.Total,
                Destacados(elegido.Armado),
                Seleccion(elegido.Armado),
                resumen));
        }
        opciones.Reverse();

        return new RecomendacionesDto(perfil.Clave, perfil.Nombre, req.Presupuesto, minimo, opciones);
    }

    private static (Producto? Cooler, Producto Gabinete)? ElegirSoporte(Producto cpu, Producto mb, Producto? gpu, List<Producto> coolers, List<Producto> gabinetes)
    {
        var opcionesCooler = new List<Producto?>();
        if (cpu.Especificacion?.IncluyeDisipador == true) opcionesCooler.Add(null);
        opcionesCooler.AddRange(coolers);

        (Producto? Cooler, Producto Gabinete)? mejor = null;
        decimal costo = decimal.MaxValue;
        foreach (var cooler in opcionesCooler)
        {
            foreach (var gabinete in gabinetes)
            {
                var precio = (cooler?.Precio ?? 0) + gabinete.Precio;
                if (precio >= costo) continue;
                var prueba = new ArmadoPc { Cpu = cpu, Motherboard = mb, Gpu = gpu, Cooler = cooler, Case = gabinete };
                if (ConfiguradorService.Evaluar(prueba).Count > 0) continue;
                mejor = (cooler, gabinete);
                costo = precio;
            }
        }
        return mejor;
    }

    private static string Describir(string nivel, PerfilUso perfil, Candidato c, decimal presupuesto)
    {
        var ahorro = presupuesto - c.Total;
        var uso = perfil.Nombre.ToLowerInvariant();
        return nivel switch
        {
            "ECONOMICA" => ahorro > 0
                ? $"Cumple con lo necesario para {uso} y te ahorra {Dinero(ahorro)}."
                : $"La opción más accesible para {uso}.",
            "EQUILIBRADA" => $"El mejor balance entre precio y rendimiento para {uso}.",
            _ => ahorro > 0
                ? $"Lo máximo que aprovecharás en {uso}; subir más no mejora tu experiencia y te sobran {Dinero(ahorro)}."
                : $"Aprovecha todo tu presupuesto para el mejor rendimiento en {uso}."
        };
    }

    private static List<string> Destacados(ArmadoPc a)
    {
        var lista = new List<string>();
        if (a.Cpu is not null) lista.Add(a.Cpu.Nombre);
        lista.Add(a.Gpu?.Nombre ?? "Gráficos integrados");
        var ram = a.Ram?.Especificacion;
        if (ram?.CapacidadGb is not null) lista.Add($"{ram.CapacidadGb} GB {ram.TipoMemoria}".Trim());
        var disco = a.Almacenamiento.FirstOrDefault()?.Especificacion;
        if (disco?.CapacidadGb is not null)
        {
            var capacidad = disco.CapacidadGb >= 1000 ? $"{disco.CapacidadGb / 1000} TB" : $"{disco.CapacidadGb} GB";
            lista.Add($"{capacidad} {disco.Interfaz}".Trim());
        }
        var fuente = a.Psu?.Especificacion?.CapacidadW;
        if (fuente is not null) lista.Add($"Fuente {fuente} W");
        return lista;
    }

    private static SeleccionPcDto Seleccion(ArmadoPc a) => new()
    {
        CpuId = a.Cpu?.Id,
        MotherboardId = a.Motherboard?.Id,
        RamId = a.Ram?.Id,
        GpuId = a.Gpu?.Id,
        CoolerId = a.Cooler?.Id,
        CaseId = a.Case?.Id,
        PsuId = a.Psu?.Id,
        AlmacenamientoIds = a.Almacenamiento.Select(s => s.Id).ToList()
    };

    private static string Dinero(decimal valor) => "$" + valor.ToString("N0", CultureInfo.GetCultureInfo("es-MX"));
}
