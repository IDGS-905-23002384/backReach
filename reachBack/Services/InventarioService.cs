using Microsoft.EntityFrameworkCore;
using reachBack.Data;
using reachBack.Dtos;
using reachBack.Infrastructure;
using reachBack.Models;

namespace reachBack.Services;

public class InventarioService
{
    private readonly ApplicationDbContext _db;
    private readonly NotificacionService _notificaciones;

    public InventarioService(ApplicationDbContext db, NotificacionService notificaciones)
    {
        _db = db;
        _notificaciones = notificaciones;
    }

    public Task RegistrarAsync(Producto producto, string tipo, int cantidad, string motivo, string responsable, int? pedidoId)
    {
        if (cantidad <= 0) throw new AppException("La cantidad debe ser mayor a cero.");
        if (tipo == "Salida")
        {
            if (producto.Stock < cantidad)
            {
                throw AppException.Conflicto($"Stock insuficiente de {producto.Nombre}. Disponible: {producto.Stock}.");
            }
            producto.Stock -= cantidad;
        }
        else
        {
            producto.Stock += cantidad;
        }

        _db.MovimientosKardex.Add(new Movimiento
        {
            Producto = producto,
            TipoMovimiento = tipo,
            Cantidad = cantidad,
            StockResultante = producto.Stock,
            Motivo = motivo,
            PedidoId = pedidoId,
            AdministradorResponsable = responsable,
            Fecha = DateTime.UtcNow
        });
        return Task.CompletedTask;
    }

    public async Task<MovimientoDto> CrearMovimientoAsync(MovimientoCrearDto dto, string responsable)
    {
        var producto = await _db.Productos.FirstOrDefaultAsync(p => p.Id == dto.ProductoId)
            ?? throw AppException.NoEncontrado("Producto no encontrado.");
        await RegistrarAsync(producto, dto.TipoMovimiento, dto.Cantidad, dto.Motivo.Trim(), responsable, null);
        if (dto.TipoMovimiento == "Salida") await _notificaciones.AvisarStockBajoAsync(new[] { producto });
        await _db.SaveChangesAsync();

        var mov = await _db.MovimientosKardex.AsNoTracking().Include(m => m.Producto)
            .Where(m => m.ProductoId == producto.Id).OrderByDescending(m => m.Id).FirstAsync();
        return ADto(mov);
    }

    public async Task<PaginaDto<MovimientoDto>> KardexAsync(int? productoId, int pagina, int tamano)
    {
        pagina = Math.Max(1, pagina);
        tamano = Math.Clamp(tamano, 1, 100);
        var query = _db.MovimientosKardex.AsNoTracking().Include(m => m.Producto).AsQueryable();
        if (productoId.HasValue) query = query.Where(m => m.ProductoId == productoId.Value);
        var total = await query.CountAsync();
        var items = await query.OrderByDescending(m => m.Fecha).ThenByDescending(m => m.Id)
            .Skip((pagina - 1) * tamano).Take(tamano).ToListAsync();
        return new PaginaDto<MovimientoDto>(items.Select(ADto).ToList(), pagina, tamano, total, (int)Math.Ceiling(total / (double)tamano));
    }

    public async Task<List<AlertaStockDto>> AlertasAsync()
    {
        return await _db.Productos.AsNoTracking()
            .Where(p => p.Activo && p.Stock <= p.StockMinimo)
            .OrderBy(p => p.Stock)
            .Select(p => new AlertaStockDto(p.Id, p.Sku, p.Nombre, p.ImagenUrl, p.Stock, p.StockMinimo, p.Categoria!.Nombre))
            .ToListAsync();
    }

    private static MovimientoDto ADto(Movimiento m) => new(
        m.Id, m.ProductoId, m.Producto?.Nombre ?? string.Empty, m.Producto?.Sku ?? string.Empty,
        m.TipoMovimiento, m.Cantidad, m.StockResultante, m.Motivo, m.PedidoId, m.AdministradorResponsable, m.Fecha);
}
