using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using reachBack.Data;
using reachBack.Dtos;
using reachBack.Infrastructure;
using reachBack.Models;

namespace reachBack.Services;

public class CarritoService
{
    private const int MaxLineas = 60;

    private readonly ApplicationDbContext _db;
    private readonly ConfiguradorService _configurador;
    private readonly TiendaOptions _tienda;

    public CarritoService(ApplicationDbContext db, ConfiguradorService configurador, IOptions<TiendaOptions> tienda)
    {
        _db = db;
        _configurador = configurador;
        _tienda = tienda.Value;
    }

    public async Task<CarritoDto> ObtenerAsync(int usuarioId)
    {
        var items = await _db.CarritoItems.AsNoTracking()
            .Include(c => c.Producto)
            .Where(c => c.UsuarioId == usuarioId)
            .ToListAsync();

        items = items
            .OrderBy(c => c.EnsambleId is null)
            .ThenBy(c => c.FechaAgregado)
            .ThenBy(c => c.Id)
            .ToList();

        var avisos = new List<string>();
        var dtos = new List<CarritoItemDto>();
        foreach (var c in items)
        {
            var p = c.Producto!;
            if (!p.Activo) avisos.Add($"{p.Nombre} ya no está disponible. Elimínalo para continuar.");
            else if (p.Stock < c.Cantidad) avisos.Add(p.Stock == 0 ? $"{p.Nombre} está agotado." : $"Solo quedan {p.Stock} piezas de {p.Nombre}.");

            dtos.Add(new CarritoItemDto(c.Id, p.Id, p.Sku, p.Nombre, p.Marca, p.ImagenUrl, p.Precio, c.Cantidad, p.Stock,
                p.Precio * c.Cantidad, c.EsEnsamblePersonalizado, c.EnsambleId, c.EnsambleNombre));
        }

        var ensambles = items.Where(c => c.EnsambleId != null).Select(c => c.EnsambleId).Distinct().Count();
        var subtotal = dtos.Sum(d => d.Subtotal);
        var manoObra = ensambles * _tienda.CostoEnsamble;
        var falta = Math.Max(0m, _tienda.EnvioGratisDesde - subtotal);

        return new CarritoDto(dtos, dtos.Sum(d => d.Cantidad), ensambles, subtotal, manoObra, subtotal + manoObra,
            _tienda.EnvioGratisDesde, falta, avisos.Distinct().ToList());
    }

    public async Task<CarritoDto> AgregarAsync(int usuarioId, AgregarCarritoDto dto)
    {
        var producto = await _db.Productos.FirstOrDefaultAsync(p => p.Id == dto.ProductoId && p.Activo)
            ?? throw AppException.NoEncontrado("Producto no disponible.");

        var existente = await _db.CarritoItems.FirstOrDefaultAsync(c => c.UsuarioId == usuarioId && c.ProductoId == dto.ProductoId && c.EnsambleId == null);
        var enEnsambles = await _db.CarritoItems.Where(c => c.UsuarioId == usuarioId && c.ProductoId == dto.ProductoId && c.EnsambleId != null).SumAsync(c => c.Cantidad);
        var cantidadFinal = (existente?.Cantidad ?? 0) + dto.Cantidad;

        if (cantidadFinal > 20) throw new AppException("Puedes agregar hasta 20 piezas del mismo producto.");
        if (cantidadFinal + enEnsambles > producto.Stock)
        {
            throw AppException.Conflicto(producto.Stock == 0 ? "Producto agotado." : $"Solo hay {producto.Stock} piezas disponibles.");
        }

        if (existente is null)
        {
            await ValidarLimiteAsync(usuarioId, 1);
            _db.CarritoItems.Add(new CarritoItem { UsuarioId = usuarioId, ProductoId = producto.Id, Cantidad = dto.Cantidad });
        }
        else
        {
            existente.Cantidad = cantidadFinal;
        }

        await _db.SaveChangesAsync();
        return await ObtenerAsync(usuarioId);
    }

    public async Task<CarritoDto> ActualizarCantidadAsync(int usuarioId, int itemId, ActualizarCantidadDto dto)
    {
        var item = await _db.CarritoItems.Include(c => c.Producto).FirstOrDefaultAsync(c => c.Id == itemId && c.UsuarioId == usuarioId)
            ?? throw AppException.NoEncontrado("Artículo no encontrado en tu carrito.");
        if (item.EnsambleId is not null) throw new AppException("Las piezas de un ensamble no se pueden modificar por separado.");
        if (dto.Cantidad > item.Producto!.Stock)
        {
            throw AppException.Conflicto($"Solo hay {item.Producto.Stock} piezas disponibles.");
        }
        item.Cantidad = dto.Cantidad;
        await _db.SaveChangesAsync();
        return await ObtenerAsync(usuarioId);
    }

    public async Task<CarritoDto> EliminarAsync(int usuarioId, int itemId)
    {
        var item = await _db.CarritoItems.FirstOrDefaultAsync(c => c.Id == itemId && c.UsuarioId == usuarioId)
            ?? throw AppException.NoEncontrado("Artículo no encontrado en tu carrito.");
        if (item.EnsambleId is not null)
        {
            var grupo = await _db.CarritoItems.Where(c => c.UsuarioId == usuarioId && c.EnsambleId == item.EnsambleId).ToListAsync();
            _db.CarritoItems.RemoveRange(grupo);
        }
        else
        {
            _db.CarritoItems.Remove(item);
        }
        await _db.SaveChangesAsync();
        return await ObtenerAsync(usuarioId);
    }

    public async Task<CarritoDto> EliminarEnsambleAsync(int usuarioId, string ensambleId)
    {
        var grupo = await _db.CarritoItems.Where(c => c.UsuarioId == usuarioId && c.EnsambleId == ensambleId).ToListAsync();
        if (grupo.Count == 0) throw AppException.NoEncontrado("Ensamble no encontrado.");
        _db.CarritoItems.RemoveRange(grupo);
        await _db.SaveChangesAsync();
        return await ObtenerAsync(usuarioId);
    }

    public async Task VaciarAsync(int usuarioId)
    {
        var items = await _db.CarritoItems.Where(c => c.UsuarioId == usuarioId).ToListAsync();
        _db.CarritoItems.RemoveRange(items);
        await _db.SaveChangesAsync();
    }

    public async Task<CarritoDto> AgregarEnsambleAsync(int usuarioId, AgregarEnsambleDto dto)
    {
        var nombre = string.IsNullOrWhiteSpace(dto.Nombre) ? "PC personalizada" : dto.Nombre.Trim();
        return await AgregarArmadoAsync(usuarioId, dto.Seleccion, nombre);
    }

    public async Task<CarritoDto> AgregarPrearmadoAsync(int usuarioId, int fichaId)
    {
        var ficha = await _configurador.FichaAsync(fichaId);
        return await AgregarArmadoAsync(usuarioId, ConfiguradorService.SeleccionDe(ficha), ficha.NombreEquipoPrearmado);
    }

    private async Task<CarritoDto> AgregarArmadoAsync(int usuarioId, SeleccionPcDto seleccion, string nombre)
    {
        var armado = await _configurador.CargarAsync(seleccion);
        var resumen = _configurador.Resumir(armado);
        if (!resumen.Completo)
        {
            var problemas = resumen.Errores.Concat(resumen.Pendientes).Take(3);
            throw new AppException("Tu configuración no está lista: " + string.Join(" ", problemas));
        }

        var piezas = armado.Componentes().Select(c => c.Producto).ToList();
        await ValidarLimiteAsync(usuarioId, piezas.Count);

        var ensambleId = Guid.NewGuid().ToString("N");
        foreach (var p in piezas)
        {
            _db.CarritoItems.Add(new CarritoItem
            {
                UsuarioId = usuarioId,
                ProductoId = p.Id,
                Cantidad = 1,
                EsEnsamblePersonalizado = true,
                EnsambleId = ensambleId,
                EnsambleNombre = nombre
            });
        }
        await _db.SaveChangesAsync();
        return await ObtenerAsync(usuarioId);
    }

    private async Task ValidarLimiteAsync(int usuarioId, int nuevas)
    {
        var actuales = await _db.CarritoItems.CountAsync(c => c.UsuarioId == usuarioId);
        if (actuales + nuevas > MaxLineas) throw new AppException("Tu carrito alcanzó el máximo de artículos.");
    }
}
