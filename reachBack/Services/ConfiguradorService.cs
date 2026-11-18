using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using reachBack.Data;
using reachBack.Dtos;
using reachBack.Infrastructure;
using reachBack.Models;

namespace reachBack.Services;

public class ArmadoPc
{
    public Producto? Cpu { get; set; }
    public Producto? Motherboard { get; set; }
    public Producto? Ram { get; set; }
    public Producto? Gpu { get; set; }
    public Producto? Cooler { get; set; }
    public Producto? Case { get; set; }
    public Producto? Psu { get; set; }
    public List<Producto> Almacenamiento { get; set; } = new();

    public IEnumerable<(string Tipo, Producto Producto)> Componentes()
    {
        if (Cpu is not null) yield return (TiposComponente.Cpu, Cpu);
        if (Motherboard is not null) yield return (TiposComponente.TarjetaMadre, Motherboard);
        if (Ram is not null) yield return (TiposComponente.Ram, Ram);
        foreach (var s in Almacenamiento) yield return (TiposComponente.Almacenamiento, s);
        if (Gpu is not null) yield return (TiposComponente.Gpu, Gpu);
        if (Cooler is not null) yield return (TiposComponente.Enfriamiento, Cooler);
        if (Case is not null) yield return (TiposComponente.Gabinete, Case);
        if (Psu is not null) yield return (TiposComponente.Fuente, Psu);
    }

    public ArmadoPc Copiar() => new()
    {
        Cpu = Cpu,
        Motherboard = Motherboard,
        Ram = Ram,
        Gpu = Gpu,
        Cooler = Cooler,
        Case = Case,
        Psu = Psu,
        Almacenamiento = new List<Producto>(Almacenamiento)
    };

    public void Asignar(string tipo, Producto? producto)
    {
        switch (tipo)
        {
            case TiposComponente.Cpu: Cpu = producto; break;
            case TiposComponente.TarjetaMadre: Motherboard = producto; break;
            case TiposComponente.Ram: Ram = producto; break;
            case TiposComponente.Gpu: Gpu = producto; break;
            case TiposComponente.Enfriamiento: Cooler = producto; break;
            case TiposComponente.Gabinete: Case = producto; break;
            case TiposComponente.Fuente: Psu = producto; break;
            case TiposComponente.Almacenamiento:
                if (producto is not null && Almacenamiento.All(a => a.Id != producto.Id)) Almacenamiento.Add(producto);
                break;
        }
    }
}

public class ConfiguradorService
{
    private record Conflicto(string Clave, string Mensaje);

    private static readonly Dictionary<string, (string Nombre, bool Obligatorio, bool Varios, string Ayuda)> InfoPasos = new()
    {
        [TiposComponente.Cpu] = ("Procesador", true, false, "Elige primero tu procesador: define la plataforma del equipo."),
        [TiposComponente.TarjetaMadre] = ("Tarjeta madre", true, false, "Solo verás tarjetas con el mismo socket de tu procesador."),
        [TiposComponente.Ram] = ("Memoria RAM", true, false, "Mostramos memorias del tipo que soporta tu tarjeta madre."),
        [TiposComponente.Almacenamiento] = ("Almacenamiento", true, true, "Puedes agregar varias unidades según las ranuras disponibles."),
        [TiposComponente.Gpu] = ("Tarjeta de video", false, false, "Obligatoria si tu procesador no tiene gráficos integrados."),
        [TiposComponente.Enfriamiento] = ("Enfriamiento", false, false, "Obligatorio si tu procesador no incluye disipador."),
        [TiposComponente.Gabinete] = ("Gabinete", true, false, "Validamos formato de tarjeta madre, largo de GPU y altura del disipador."),
        [TiposComponente.Fuente] = ("Fuente de poder", true, false, "Calculamos la potencia necesaria con 30% de margen.")
    };

    private const int ConsumoBaseW = 50;

    private readonly ApplicationDbContext _db;
    private readonly TiendaOptions _tienda;

    public ConfiguradorService(ApplicationDbContext db, IOptions<TiendaOptions> tienda)
    {
        _db = db;
        _tienda = tienda.Value;
    }

    public static string NombrePaso(string tipo) => InfoPasos.TryGetValue(tipo, out var info) ? info.Nombre : tipo;

    public async Task<List<PasoConfiguradorDto>> PasosAsync()
    {
        var categorias = await _db.Categorias.AsNoTracking().Where(c => c.TipoComponente != null).ToListAsync();
        var pasos = new List<PasoConfiguradorDto>();
        foreach (var tipo in TiposComponente.Orden)
        {
            var categoria = categorias.FirstOrDefault(c => c.TipoComponente == tipo);
            if (categoria is null) continue;
            var info = InfoPasos[tipo];
            pasos.Add(new PasoConfiguradorDto(tipo, info.Nombre, categoria.Id, info.Obligatorio, info.Varios, info.Ayuda));
        }
        return pasos;
    }

    public async Task<ArmadoPc> CargarAsync(SeleccionPcDto s)
    {
        var ids = new List<int>();
        void Agregar(int? id) { if (id.HasValue) ids.Add(id.Value); }
        Agregar(s.CpuId); Agregar(s.MotherboardId); Agregar(s.RamId); Agregar(s.GpuId);
        Agregar(s.CoolerId); Agregar(s.CaseId); Agregar(s.PsuId);
        ids.AddRange(s.AlmacenamientoIds ?? new List<int>());

        var productos = await _db.Productos
            .Include(p => p.Categoria)
            .Include(p => p.Especificacion)
            .Where(p => ids.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        var armado = new ArmadoPc();
        Producto? Resolver(int? id, string tipo)
        {
            if (!id.HasValue) return null;
            if (!productos.TryGetValue(id.Value, out var p) || !p.Activo)
            {
                throw new AppException($"El componente seleccionado para {NombrePaso(tipo)} ya no está disponible.");
            }
            if (p.Categoria?.TipoComponente != tipo)
            {
                throw new AppException($"{p.Nombre} no corresponde al paso {NombrePaso(tipo)}.");
            }
            return p;
        }

        armado.Cpu = Resolver(s.CpuId, TiposComponente.Cpu);
        armado.Motherboard = Resolver(s.MotherboardId, TiposComponente.TarjetaMadre);
        armado.Ram = Resolver(s.RamId, TiposComponente.Ram);
        armado.Gpu = Resolver(s.GpuId, TiposComponente.Gpu);
        armado.Cooler = Resolver(s.CoolerId, TiposComponente.Enfriamiento);
        armado.Case = Resolver(s.CaseId, TiposComponente.Gabinete);
        armado.Psu = Resolver(s.PsuId, TiposComponente.Fuente);
        foreach (var id in (s.AlmacenamientoIds ?? new List<int>()).Distinct())
        {
            armado.Almacenamiento.Add(Resolver(id, TiposComponente.Almacenamiento)!);
        }
        return armado;
    }

    public async Task<List<OpcionComponenteDto>> OpcionesAsync(OpcionesRequestDto req)
    {
        var armado = await CargarAsync(req.Seleccion);
        var sinSlot = armado.Copiar();
        if (req.Tipo != TiposComponente.Almacenamiento) sinSlot.Asignar(req.Tipo, null);
        var clavesBase = Evaluar(sinSlot).Select(c => c.Clave).ToHashSet();

        var candidatos = await _db.Productos.AsNoTracking()
            .Include(p => p.Categoria)
            .Include(p => p.Especificacion)
            .Where(p => p.Activo && p.Categoria!.TipoComponente == req.Tipo)
            .OrderBy(p => p.Precio)
            .ToListAsync();

        var opciones = new List<OpcionComponenteDto>();
        foreach (var c in candidatos)
        {
            var prueba = sinSlot.Copiar();
            prueba.Asignar(req.Tipo, c);
            var motivos = Evaluar(prueba).Where(x => !clavesBase.Contains(x.Clave)).Select(x => x.Mensaje).Distinct().ToList();
            if (c.Stock <= 0) motivos.Add("Sin existencias por el momento.");
            opciones.Add(new OpcionComponenteDto(c.ALista(), motivos.Count == 0, motivos, ResumenSpec(req.Tipo, Spec(c))));
        }

        return opciones.OrderByDescending(o => o.Compatible).ThenBy(o => o.Producto.Precio).ToList();
    }

    public async Task<ResumenConfiguracionDto> ValidarAsync(SeleccionPcDto seleccion)
    {
        var armado = await CargarAsync(seleccion);
        return Resumir(armado);
    }

    public ResumenConfiguracionDto Resumir(ArmadoPc a)
    {
        var errores = Evaluar(a).Select(c => c.Mensaje).Distinct().ToList();
        var advertencias = new List<string>();
        var pendientes = new List<string>();

        if (a.Cpu is null) pendientes.Add("Selecciona un procesador.");
        if (a.Motherboard is null) pendientes.Add("Selecciona una tarjeta madre.");
        if (a.Ram is null) pendientes.Add("Selecciona la memoria RAM.");
        if (a.Almacenamiento.Count == 0) pendientes.Add("Agrega al menos una unidad de almacenamiento.");
        if (a.Case is null) pendientes.Add("Selecciona un gabinete.");
        if (a.Psu is null) pendientes.Add("Selecciona una fuente de poder.");

        var cpu = Spec(a.Cpu);
        if (a.Cpu is not null && a.Gpu is null)
        {
            if (cpu.GraficosIntegrados == true) advertencias.Add("Usarás los gráficos integrados del procesador.");
            else pendientes.Add("Tu procesador no tiene gráficos integrados: agrega una tarjeta de video.");
        }
        if (a.Cpu is not null && a.Cooler is null)
        {
            if (cpu.IncluyeDisipador == true) advertencias.Add("Se usará el disipador incluido con el procesador.");
            else pendientes.Add("Tu procesador no incluye disipador: agrega un sistema de enfriamiento.");
        }

        foreach (var (_, p) in a.Componentes())
        {
            if (p.Stock <= 0) errores.Add($"{p.Nombre} está agotado.");
        }

        var almacenamientoRepetido = a.Almacenamiento.Count;
        if (almacenamientoRepetido > 4) errores.Add("Puedes agregar hasta 4 unidades de almacenamiento.");

        var consumo = Consumo(a);
        var recomendada = FuenteRecomendada(a);
        var componentes = a.Componentes().Select(c => new ComponenteSeleccionadoDto(c.Tipo, NombrePaso(c.Tipo), c.Producto.ALista())).ToList();
        var subtotal = componentes.Sum(c => c.Producto.Precio);
        var ensamble = componentes.Count > 0 ? _tienda.CostoEnsamble : 0m;

        return new ResumenConfiguracionDto(
            componentes,
            errores,
            advertencias,
            pendientes,
            consumo,
            recomendada,
            subtotal,
            ensamble,
            subtotal + ensamble,
            errores.Count == 0 && pendientes.Count == 0);
    }

    private static EspecificacionComponente Spec(Producto? p) => p?.Especificacion ?? new EspecificacionComponente();

    public static string ResumenSpec(string tipo, EspecificacionComponente e)
    {
        var partes = new List<string?>();
        switch (tipo)
        {
            case TiposComponente.Cpu:
                partes.Add(e.Socket is null ? null : $"Socket {e.Socket}");
                partes.Add(e.ConsumoW is null ? null : $"{e.ConsumoW} W");
                partes.Add(e.GraficosIntegrados == true ? "Gráficos integrados" : "Requiere GPU");
                partes.Add(e.IncluyeDisipador == true ? "Incluye disipador" : null);
                break;
            case TiposComponente.TarjetaMadre:
                partes.Add(e.Socket);
                partes.Add(e.FormFactor);
                partes.Add(e.TipoMemoria);
                partes.Add(e.RanurasM2 is null ? null : $"{e.RanurasM2} x M.2");
                break;
            case TiposComponente.Ram:
                partes.Add(e.TipoMemoria);
                partes.Add(e.CapacidadGb is null ? null : $"{e.CapacidadGb} GB");
                partes.Add(e.ModulosRam is null ? null : $"{e.ModulosRam} módulos");
                break;
            case TiposComponente.Almacenamiento:
                partes.Add(e.Interfaz);
                partes.Add(e.CapacidadGb is null ? null : (e.CapacidadGb >= 1000 ? $"{e.CapacidadGb / 1000} TB" : $"{e.CapacidadGb} GB"));
                break;
            case TiposComponente.Gpu:
                partes.Add(e.LongitudMm is null ? null : $"{e.LongitudMm} mm");
                partes.Add(e.ConsumoW is null ? null : $"{e.ConsumoW} W");
                partes.Add(e.FuenteRecomendadaW is null ? null : $"Fuente {e.FuenteRecomendadaW} W");
                break;
            case TiposComponente.Enfriamiento:
                partes.Add(e.RadiadorMm is null ? (e.AlturaMm is null ? null : $"Aire · {e.AlturaMm} mm") : $"Líquido · {e.RadiadorMm} mm");
                partes.Add(e.TdpMaxW is null ? null : $"Hasta {e.TdpMaxW} W");
                break;
            case TiposComponente.Gabinete:
                partes.Add(e.FormFactorsSoportados?.Replace(",", "/"));
                partes.Add(e.LongitudMaxGpuMm is null ? null : $"GPU {e.LongitudMaxGpuMm} mm");
                partes.Add(e.RadiadorMaxMm is null ? null : $"Radiador {e.RadiadorMaxMm} mm");
                break;
            case TiposComponente.Fuente:
                partes.Add(e.CapacidadW is null ? null : $"{e.CapacidadW} W");
                break;
        }
        return string.Join(" · ", partes.Where(x => !string.IsNullOrWhiteSpace(x)));
    }

    private static bool Contiene(string? lista, string? valor)
    {
        if (string.IsNullOrWhiteSpace(lista) || string.IsNullOrWhiteSpace(valor)) return true;
        return lista.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(x => string.Equals(x, valor.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    private static int Consumo(ArmadoPc a)
    {
        var componentes = a.Componentes().ToList();
        if (componentes.Count == 0) return 0;
        return componentes.Sum(c => Spec(c.Producto).ConsumoW ?? 0) + ConsumoBaseW;
    }

    private static int FuenteRecomendada(ArmadoPc a)
    {
        var consumo = Consumo(a);
        if (consumo == 0) return 0;
        var conMargen = (int)(Math.Ceiling(consumo * 1.3 / 50.0) * 50);
        var gpu = Spec(a.Gpu).FuenteRecomendadaW ?? 0;
        return Math.Max(conMargen, gpu);
    }

    private static List<Conflicto> Evaluar(ArmadoPc a)
    {
        var lista = new List<Conflicto>();
        var cpu = Spec(a.Cpu);
        var mb = Spec(a.Motherboard);
        var ram = Spec(a.Ram);
        var gpu = Spec(a.Gpu);
        var cooler = Spec(a.Cooler);
        var gabinete = Spec(a.Case);
        var psu = Spec(a.Psu);

        if (a.Cpu is not null && a.Motherboard is not null && cpu.Socket is not null && mb.Socket is not null
            && !string.Equals(cpu.Socket, mb.Socket, StringComparison.OrdinalIgnoreCase))
        {
            lista.Add(new("CPU-MB", $"Socket incompatible: el procesador usa {cpu.Socket} y la tarjeta madre {mb.Socket}."));
        }

        if (a.Motherboard is not null && a.Ram is not null)
        {
            if (mb.TipoMemoria is not null && ram.TipoMemoria is not null
                && !string.Equals(mb.TipoMemoria, ram.TipoMemoria, StringComparison.OrdinalIgnoreCase))
            {
                lista.Add(new("MB-RAM-TIPO", $"La tarjeta madre usa memoria {mb.TipoMemoria} y la memoria elegida es {ram.TipoMemoria}."));
            }
            if (mb.SlotsRam.HasValue && ram.ModulosRam.HasValue && ram.ModulosRam > mb.SlotsRam)
            {
                lista.Add(new("MB-RAM-SLOTS", $"La memoria tiene {ram.ModulosRam} módulos y la tarjeta madre solo {mb.SlotsRam} ranuras."));
            }
        }

        if (a.Motherboard is not null && a.Case is not null && !Contiene(gabinete.FormFactorsSoportados, mb.FormFactor))
        {
            lista.Add(new("MB-CASE", $"El gabinete no admite tarjetas madre {mb.FormFactor}."));
        }

        if (a.Gpu is not null && a.Case is not null && gpu.LongitudMm.HasValue && gabinete.LongitudMaxGpuMm.HasValue
            && gpu.LongitudMm > gabinete.LongitudMaxGpuMm)
        {
            lista.Add(new("GPU-CASE", $"La tarjeta de video mide {gpu.LongitudMm} mm y el gabinete admite hasta {gabinete.LongitudMaxGpuMm} mm."));
        }

        if (a.Cpu is not null && a.Cooler is not null)
        {
            if (!Contiene(cooler.SocketsSoportados, cpu.Socket))
            {
                lista.Add(new("CPU-COOLER-SOCKET", $"El sistema de enfriamiento no es compatible con el socket {cpu.Socket}."));
            }
            if (cooler.TdpMaxW.HasValue && cpu.ConsumoW.HasValue && cpu.ConsumoW > cooler.TdpMaxW)
            {
                lista.Add(new("CPU-COOLER-TDP", $"El enfriamiento soporta hasta {cooler.TdpMaxW} W y el procesador genera {cpu.ConsumoW} W."));
            }
        }

        if (a.Cooler is not null && a.Case is not null)
        {
            if (cooler.AlturaMm.HasValue && gabinete.AlturaMaxCoolerMm.HasValue && cooler.AlturaMm > gabinete.AlturaMaxCoolerMm)
            {
                lista.Add(new("COOLER-CASE", $"El disipador mide {cooler.AlturaMm} mm de alto y el gabinete admite hasta {gabinete.AlturaMaxCoolerMm} mm."));
            }
            if (cooler.RadiadorMm.HasValue && gabinete.RadiadorMaxMm.HasValue && cooler.RadiadorMm > gabinete.RadiadorMaxMm)
            {
                lista.Add(new("COOLER-CASE-RAD", $"El radiador de {cooler.RadiadorMm} mm no cabe: el gabinete admite hasta {gabinete.RadiadorMaxMm} mm."));
            }
        }

        if (a.Motherboard is not null && mb.RanurasM2.HasValue)
        {
            var m2 = a.Almacenamiento.Count(s => (Spec(s).Interfaz ?? string.Empty).Contains("M.2", StringComparison.OrdinalIgnoreCase));
            if (m2 > mb.RanurasM2)
            {
                lista.Add(new("MB-M2", $"La tarjeta madre tiene {mb.RanurasM2} ranuras M.2 y seleccionaste {m2} unidades M.2."));
            }
        }

        if (a.Psu is not null && psu.CapacidadW.HasValue)
        {
            var recomendada = FuenteRecomendada(a);
            if (psu.CapacidadW < recomendada)
            {
                lista.Add(new("PSU", $"La fuente de {psu.CapacidadW} W es insuficiente: este equipo requiere al menos {recomendada} W."));
            }
        }

        return lista;
    }

    public async Task<List<PrearmadoDto>> PrearmadosAsync()
    {
        var fichas = await _db.FichasTecnicas.AsNoTracking()
            .Where(f => f.Activo)
            .Include(f => f.ComponentesRequeridos).ThenInclude(c => c.Producto).ThenInclude(p => p!.Categoria)
            .OrderBy(f => f.Id)
            .ToListAsync();
        return fichas.Select(APrearmado).ToList();
    }

    public async Task<FichaTecnica> FichaAsync(int id)
    {
        return await _db.FichasTecnicas.AsNoTracking()
            .Include(f => f.ComponentesRequeridos).ThenInclude(c => c.Producto).ThenInclude(p => p!.Categoria)
            .FirstOrDefaultAsync(f => f.Id == id && f.Activo)
            ?? throw AppException.NoEncontrado("Equipo prearmado no encontrado.");
    }

    public static SeleccionPcDto SeleccionDe(FichaTecnica ficha)
    {
        var s = new SeleccionPcDto();
        foreach (var c in ficha.ComponentesRequeridos)
        {
            var p = c.Producto;
            if (p?.Categoria?.TipoComponente is null) continue;
            switch (p.Categoria.TipoComponente)
            {
                case TiposComponente.Cpu: s.CpuId = p.Id; break;
                case TiposComponente.TarjetaMadre: s.MotherboardId = p.Id; break;
                case TiposComponente.Ram: s.RamId = p.Id; break;
                case TiposComponente.Gpu: s.GpuId = p.Id; break;
                case TiposComponente.Enfriamiento: s.CoolerId = p.Id; break;
                case TiposComponente.Gabinete: s.CaseId = p.Id; break;
                case TiposComponente.Fuente: s.PsuId = p.Id; break;
                case TiposComponente.Almacenamiento: s.AlmacenamientoIds.Add(p.Id); break;
            }
        }
        return s;
    }

    private PrearmadoDto APrearmado(FichaTecnica f)
    {
        var componentes = f.ComponentesRequeridos
            .Where(c => c.Producto is not null)
            .OrderBy(c => Array.IndexOf(TiposComponente.Orden, c.Producto!.Categoria?.TipoComponente ?? string.Empty))
            .Select(c => new PrearmadoComponenteDto(c.Producto!.Categoria?.TipoComponente ?? string.Empty, c.Producto.ALista(), c.Cantidad))
            .ToList();
        var precio = componentes.Sum(c => c.Producto.Precio * c.Cantidad);
        var disponible = f.ComponentesRequeridos.All(c => c.Producto is { Activo: true } && c.Producto.Stock >= c.Cantidad);
        var imagen = componentes.FirstOrDefault(c => c.Tipo == TiposComponente.Gabinete)?.Producto.ImagenUrl
            ?? componentes.FirstOrDefault()?.Producto.ImagenUrl ?? string.Empty;
        return new PrearmadoDto(f.Id, f.NombreEquipoPrearmado, f.CategoriaUso, f.Descripcion, imagen, precio,
            _tienda.CostoEnsamble, precio + _tienda.CostoEnsamble, disponible, componentes);
    }
}
