using Microsoft.EntityFrameworkCore;
using reachBack.Data;
using reachBack.Dtos;
using reachBack.Infrastructure;
using reachBack.Models;

namespace reachBack.Services;

public class NotificacionService
{
    private const int MaxPorConsulta = 50;
    private readonly ApplicationDbContext _db;

    public NotificacionService(ApplicationDbContext db)
    {
        _db = db;
    }

    public void Agregar(int usuarioId, string titulo, string mensaje, string tipo, int? pedidoId)
    {
        _db.Notificaciones.Add(new Notificacion
        {
            UsuarioId = usuarioId,
            Titulo = Recortar(titulo, 120),
            Mensaje = Recortar(mensaje, 400),
            Tipo = tipo,
            PedidoId = pedidoId,
            Fecha = DateTime.UtcNow
        });
    }

    public async Task AgregarAPersonalAsync(string titulo, string mensaje, string tipo, int? pedidoId)
    {
        var personal = await _db.Usuarios
            .Where(u => u.Activo && (u.Rol == Roles.Empleado || u.Rol == Roles.Administrador))
            .Select(u => u.Id)
            .ToListAsync();
        foreach (var id in personal) Agregar(id, titulo, mensaje, tipo, pedidoId);
    }

    public async Task AvisarStockBajoAsync(IEnumerable<Producto> productos)
    {
        foreach (var p in productos.Where(p => p.Activo && p.Stock <= p.StockMinimo).DistinctBy(p => p.Id))
        {
            var mensaje = p.Stock == 0
                ? $"{p.Nombre} ({p.Sku}) se agotó. Registra una entrada de mercancía."
                : $"{p.Nombre} ({p.Sku}) tiene {p.Stock} piezas; el mínimo es {p.StockMinimo}.";
            await AgregarAPersonalAsync(p.Stock == 0 ? "Producto agotado" : "Stock bajo", mensaje, TiposNotificacion.Inventario, null);
        }
    }

    public async Task<PaginaDto<NotificacionDto>> ListarAsync(int usuarioId, bool soloNoLeidas, int pagina, int tamano)
    {
        pagina = Math.Max(1, pagina);
        tamano = Math.Clamp(tamano, 1, MaxPorConsulta);
        var query = _db.Notificaciones.AsNoTracking().Where(n => n.UsuarioId == usuarioId);
        if (soloNoLeidas) query = query.Where(n => !n.Leida);
        var total = await query.CountAsync();
        var items = await query.OrderByDescending(n => n.Id)
            .Skip((pagina - 1) * tamano).Take(tamano)
            .Select(n => new NotificacionDto(n.Id, n.Titulo, n.Mensaje, n.Tipo, n.PedidoId, n.Leida, n.Fecha))
            .ToListAsync();
        return new PaginaDto<NotificacionDto>(items, pagina, tamano, total, (int)Math.Ceiling(total / (double)tamano));
    }

    public async Task<List<NotificacionDto>> NuevasAsync(int usuarioId, int despuesDe)
    {
        return await _db.Notificaciones.AsNoTracking()
            .Where(n => n.UsuarioId == usuarioId && n.Id > despuesDe && !n.Leida)
            .OrderBy(n => n.Id)
            .Take(MaxPorConsulta)
            .Select(n => new NotificacionDto(n.Id, n.Titulo, n.Mensaje, n.Tipo, n.PedidoId, n.Leida, n.Fecha))
            .ToListAsync();
    }

    public async Task<ConteoNotificacionesDto> ConteoAsync(int usuarioId)
    {
        var noLeidas = await _db.Notificaciones.CountAsync(n => n.UsuarioId == usuarioId && !n.Leida);
        var ultimo = await _db.Notificaciones.Where(n => n.UsuarioId == usuarioId).MaxAsync(n => (int?)n.Id) ?? 0;
        return new ConteoNotificacionesDto(noLeidas, ultimo);
    }

    public async Task MarcarLeidaAsync(int usuarioId, int id)
    {
        var n = await _db.Notificaciones.FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == usuarioId)
            ?? throw AppException.NoEncontrado("Notificación no encontrada.");
        n.Leida = true;
        await _db.SaveChangesAsync();
    }

    public async Task MarcarTodasAsync(int usuarioId)
    {
        await _db.Notificaciones
            .Where(n => n.UsuarioId == usuarioId && !n.Leida)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.Leida, true));
    }

    public static string Dinero(decimal valor) => "$" + valor.ToString("N2", System.Globalization.CultureInfo.InvariantCulture);

    private static string Recortar(string texto, int max) => texto.Length <= max ? texto : texto[..max];
}
