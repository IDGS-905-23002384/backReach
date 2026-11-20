using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using reachBack.Data;
using reachBack.Dtos;
using reachBack.Infrastructure;
using reachBack.Models;

namespace reachBack.Services;

public class PedidoService
{
    private static readonly TimeSpan ZonaMexico = TimeSpan.FromHours(-6);

    private readonly ApplicationDbContext _db;
    private readonly InventarioService _inventario;
    private readonly TiendaOptions _tienda;
    private readonly ILogger<PedidoService> _logger;
    private readonly NotificacionService _notificaciones;

    public PedidoService(ApplicationDbContext db, InventarioService inventario, IOptions<TiendaOptions> tienda, ILogger<PedidoService> logger, NotificacionService notificaciones)
    {
        _db = db;
        _inventario = inventario;
        _tienda = tienda.Value;
        _logger = logger;
        _notificaciones = notificaciones;
    }

    public async Task<PedidoDetalleDto> CrearAsync(int usuarioId, CheckoutDto dto)
    {
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == usuarioId && u.Activo)
            ?? throw new AppException("Sesión inválida.", StatusCodes.Status401Unauthorized);

        var metodo = await _db.MetodosEnvio.FirstOrDefaultAsync(m => m.Id == dto.MetodoEnvioId && m.Activo)
            ?? throw new AppException("Selecciona un método de envío válido.");

        Direccion? direccion = null;
        if (metodo.RequiereDireccion)
        {
            if (!dto.DireccionId.HasValue) throw new AppException("Selecciona una dirección de envío.");
            direccion = await _db.Direcciones.FirstOrDefaultAsync(d => d.Id == dto.DireccionId.Value && d.UsuarioId == usuarioId)
                ?? throw new AppException("La dirección seleccionada no es válida.");
        }

        var (marca, ultimos4) = ValidarPago(dto.Pago);

        await using var tx = await _db.Database.BeginTransactionAsync();

        var items = await _db.CarritoItems.Include(c => c.Producto).Where(c => c.UsuarioId == usuarioId).ToListAsync();
        if (items.Count == 0) throw new AppException("Tu carrito está vacío.");

        foreach (var grupo in items.GroupBy(i => i.ProductoId))
        {
            var p = grupo.First().Producto!;
            var requerido = grupo.Sum(i => i.Cantidad);
            if (!p.Activo) throw AppException.Conflicto($"{p.Nombre} ya no está disponible. Elimínalo de tu carrito.");
            if (p.Stock < requerido) throw AppException.Conflicto($"Stock insuficiente de {p.Nombre}. Disponible: {p.Stock}.");
        }

        var subtotal = items.Sum(i => i.Producto!.Precio * i.Cantidad);
        var ensambles = items.Where(i => i.EnsambleId != null).Select(i => i.EnsambleId).Distinct().Count();
        var manoObra = ensambles * _tienda.CostoEnsamble;
        var envio = metodo.AplicaEnvioGratis && subtotal >= _tienda.EnvioGratisDesde ? 0m : metodo.Costo;
        var total = subtotal + manoObra + envio;
        var iva = Math.Round(total - total / (1 + _tienda.TasaIva), 2, MidpointRounding.AwayFromZero);
        var ahora = DateTime.UtcNow;

        var pedido = new Pedido
        {
            Folio = await GenerarFolioAsync(),
            UsuarioId = usuarioId,
            Fecha = ahora,
            FechaActualizacion = ahora,
            Subtotal = subtotal,
            CostoManoObra = manoObra,
            CostoEnvio = envio,
            Iva = iva,
            Total = total,
            Estado = EstadosPedido.Pagado,
            IncluyeEnsamble = ensambles > 0,
            EstadoArmado = ensambles > 0 ? EstadosPedido.ArmadoAtendido : EstadosPedido.ArmadoNoAplica,
            EstadoEnvio = EstadosPedido.EnvioPreparando,
            MetodoEnvioId = metodo.Id,
            Paqueteria = metodo.RequiereDireccion ? metodo.Paqueteria : null,
            FechaEstimadaEntrega = ahora.AddDays(metodo.DiasMax + (ensambles > 0 ? 2 : 0)),
            EnvioNombreRecibe = direccion?.NombreRecibe ?? usuario.Nombre,
            EnvioTelefono = direccion?.Telefono ?? usuario.Telefono,
            EnvioDireccion = direccion is null ? null : FormatearDireccion(direccion),
            EnvioCiudad = direccion?.Ciudad,
            EnvioEstado = direccion?.Estado,
            EnvioCodigoPostal = direccion?.CodigoPostal,
            EnvioReferencias = direccion?.Referencias,
            MetodoPago = "Tarjeta",
            TarjetaMarca = marca,
            TarjetaUltimos4 = ultimos4,
            Notas = string.IsNullOrWhiteSpace(dto.Notas) ? null : dto.Notas.Trim()
        };

        foreach (var i in items)
        {
            pedido.Detalles.Add(new PedidoDetalle
            {
                ProductoId = i.ProductoId,
                Sku = i.Producto!.Sku,
                NombreProducto = i.Producto.Nombre,
                ImagenUrl = i.Producto.ImagenUrl,
                Cantidad = i.Cantidad,
                PrecioUnitario = i.Producto.Precio,
                EsEnsamble = i.EnsambleId != null,
                EnsambleId = i.EnsambleId,
                EnsambleNombre = i.EnsambleNombre
            });
        }

        pedido.Seguimiento.Add(new PedidoSeguimiento { Titulo = "Pedido recibido", Descripcion = "Recibimos tu pedido y tu pago fue aprobado.", Fecha = ahora });
        if (pedido.IncluyeEnsamble)
        {
            pedido.Seguimiento.Add(new PedidoSeguimiento { Titulo = "Ensamble atendido", Descripcion = "Un técnico revisará tus componentes antes de armar tu equipo.", Fecha = ahora });
        }

        _db.Pedidos.Add(pedido);
        await _db.SaveChangesAsync();

        foreach (var grupo in items.GroupBy(i => i.ProductoId))
        {
            var p = grupo.First().Producto!;
            var motivo = grupo.Any(i => i.EnsambleId != null) ? $"Venta con ensamble {pedido.Folio}" : $"Venta {pedido.Folio}";
            await _inventario.RegistrarAsync(p, "Salida", grupo.Sum(i => i.Cantidad), motivo, usuario.Email, pedido.Id);
        }

        _db.CarritoItems.RemoveRange(items);
        _notificaciones.Agregar(usuarioId, "Pedido confirmado",
            $"Recibimos tu pedido {pedido.Folio} por {NotificacionService.Dinero(pedido.Total)}. Te avisaremos cada avance.",
            TiposNotificacion.Pedido, pedido.Id);
        await _notificaciones.AgregarAPersonalAsync("Nuevo pedido",
            $"{usuario.Nombre} realizó el pedido {pedido.Folio} por {NotificacionService.Dinero(pedido.Total)}{(pedido.IncluyeEnsamble ? " con ensamble" : string.Empty)}.",
            TiposNotificacion.Pedido, pedido.Id);
        await _notificaciones.AvisarStockBajoAsync(items.Select(i => i.Producto!));
        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        _logger.LogInformation("Pedido {Folio} creado por usuario {UsuarioId}", pedido.Folio, usuarioId);
        return await DetalleAsync(pedido.Id, usuarioId, false);
    }

    public async Task<List<PedidoResumenDto>> MisPedidosAsync(int usuarioId)
    {
        var pedidos = await _db.Pedidos.AsNoTracking()
            .Include(p => p.Detalles)
            .Include(p => p.Usuario)
            .Where(p => p.UsuarioId == usuarioId)
            .OrderByDescending(p => p.Fecha)
            .ToListAsync();
        return pedidos.Select(Resumen).ToList();
    }

    public async Task<PedidoDetalleDto> DetalleAsync(int pedidoId, int usuarioId, bool esPersonal)
    {
        var p = await _db.Pedidos.AsNoTracking()
            .Include(x => x.Detalles)
            .Include(x => x.Seguimiento)
            .Include(x => x.MetodoEnvio)
            .Include(x => x.Usuario)
            .FirstOrDefaultAsync(x => x.Id == pedidoId && (esPersonal || x.UsuarioId == usuarioId))
            ?? throw AppException.NoEncontrado("Pedido no encontrado.");
        return Detalle(p);
    }

    public async Task<PedidoDetalleDto> CancelarAsync(int pedidoId, int usuarioId, bool esPersonal, string responsable)
    {
        await using var tx = await _db.Database.BeginTransactionAsync();
        var p = await _db.Pedidos.Include(x => x.Detalles).ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(x => x.Id == pedidoId && (esPersonal || x.UsuarioId == usuarioId))
            ?? throw AppException.NoEncontrado("Pedido no encontrado.");

        if (!PuedeCancelar(p)) throw AppException.Conflicto("Este pedido ya no se puede cancelar porque está en proceso de ensamble o envío.");

        foreach (var grupo in p.Detalles.GroupBy(d => d.ProductoId))
        {
            await _inventario.RegistrarAsync(grupo.First().Producto!, "Entrada", grupo.Sum(d => d.Cantidad), $"Cancelación {p.Folio}", responsable, p.Id);
        }

        p.Estado = EstadosPedido.Cancelado;
        p.FechaActualizacion = DateTime.UtcNow;
        _db.PedidoSeguimientos.Add(new PedidoSeguimiento
        {
            PedidoId = p.Id,
            Titulo = "Pedido cancelado",
            Descripcion = esPersonal ? "El pedido fue cancelado por la tienda. El reembolso se verá reflejado en 5 a 10 días hábiles." : "Cancelaste tu pedido. El reembolso se verá reflejado en 5 a 10 días hábiles.",
            Fecha = DateTime.UtcNow
        });
        if (esPersonal)
        {
            _notificaciones.Agregar(p.UsuarioId, "Pedido cancelado",
                $"La tienda canceló tu pedido {p.Folio}. El reembolso se verá reflejado en 5 a 10 días hábiles.",
                TiposNotificacion.Pedido, p.Id);
        }
        else
        {
            await _notificaciones.AgregarAPersonalAsync("Pedido cancelado por el cliente",
                $"El cliente canceló el pedido {p.Folio}. Las piezas regresaron al inventario.",
                TiposNotificacion.Pedido, p.Id);
        }
        await _db.SaveChangesAsync();
        await tx.CommitAsync();
        return await DetalleAsync(p.Id, usuarioId, esPersonal);
    }

    public async Task<PaginaDto<PedidoResumenDto>> ListarAdminAsync(string? estado, string? q, int pagina, int tamano)
    {
        pagina = Math.Max(1, pagina);
        tamano = Math.Clamp(tamano, 1, 100);
        var query = _db.Pedidos.AsNoTracking().Include(p => p.Detalles).Include(p => p.Usuario).AsQueryable();

        query = estado switch
        {
            "pendientes" => query.Where(p => p.Estado != EstadosPedido.Cancelado && p.EstadoEnvio == EstadosPedido.EnvioPreparando),
            "ensamble" => query.Where(p => p.Estado != EstadosPedido.Cancelado && p.IncluyeEnsamble && p.EstadoArmado != EstadosPedido.ArmadoTerminado),
            "enviados" => query.Where(p => p.Estado != EstadosPedido.Cancelado && p.EstadoEnvio == EstadosPedido.EnvioEnviado),
            "entregados" => query.Where(p => p.EstadoEnvio == EstadosPedido.EnvioEntregado),
            "cancelados" => query.Where(p => p.Estado == EstadosPedido.Cancelado),
            _ => query
        };

        if (!string.IsNullOrWhiteSpace(q))
        {
            var t = q.Trim();
            query = query.Where(p => p.Folio.Contains(t) || p.Usuario!.Nombre.Contains(t) || p.Usuario.Email.Contains(t));
        }

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(p => p.Fecha).Skip((pagina - 1) * tamano).Take(tamano).ToListAsync();
        return new PaginaDto<PedidoResumenDto>(items.Select(Resumen).ToList(), pagina, tamano, total, (int)Math.Ceiling(total / (double)tamano));
    }

    public async Task<PedidoDetalleDto> ActualizarArmadoAsync(int pedidoId, ActualizarArmadoDto dto)
    {
        var p = await _db.Pedidos.FirstOrDefaultAsync(x => x.Id == pedidoId) ?? throw AppException.NoEncontrado("Pedido no encontrado.");
        if (p.Estado == EstadosPedido.Cancelado) throw AppException.Conflicto("El pedido está cancelado.");
        if (!p.IncluyeEnsamble) throw new AppException("Este pedido no incluye servicio de ensamble.");

        var actual = Array.IndexOf(EstadosPedido.Armado, p.EstadoArmado);
        var nuevo = Array.IndexOf(EstadosPedido.Armado, dto.Estado);
        if (nuevo <= actual) throw new AppException($"El ensamble ya está en estado {p.EstadoArmado}.");

        p.EstadoArmado = dto.Estado;
        p.FechaActualizacion = DateTime.UtcNow;
        var (titulo, descripcion) = dto.Estado == EstadosPedido.ArmadoEnsamblando
            ? ("Ensamblando tu equipo", "Nuestro técnico está armando y gestionando el cableado de tu PC.")
            : ("Ensamble terminado", "Tu equipo pasó las pruebas de encendido, temperatura y estabilidad.");
        _db.PedidoSeguimientos.Add(new PedidoSeguimiento { PedidoId = p.Id, Titulo = titulo, Descripcion = descripcion, Fecha = DateTime.UtcNow });
        _notificaciones.Agregar(p.UsuarioId, titulo, $"Pedido {p.Folio}: {descripcion}", TiposNotificacion.Pedido, p.Id);
        await _db.SaveChangesAsync();
        return await DetalleAsync(p.Id, 0, true);
    }

    public async Task<PedidoDetalleDto> ActualizarEnvioAsync(int pedidoId, ActualizarEnvioDto dto)
    {
        var p = await _db.Pedidos.Include(x => x.MetodoEnvio).FirstOrDefaultAsync(x => x.Id == pedidoId)
            ?? throw AppException.NoEncontrado("Pedido no encontrado.");
        if (p.Estado == EstadosPedido.Cancelado) throw AppException.Conflicto("El pedido está cancelado.");

        var actual = Array.IndexOf(EstadosPedido.Envio, p.EstadoEnvio);
        var nuevo = Array.IndexOf(EstadosPedido.Envio, dto.Estado);
        if (nuevo <= actual) throw new AppException($"El envío ya está en estado {p.EstadoEnvio}.");
        if (p.IncluyeEnsamble && p.EstadoArmado != EstadosPedido.ArmadoTerminado)
        {
            throw new AppException("Termina el ensamble antes de enviar el pedido.");
        }

        var requiereEnvio = p.MetodoEnvio?.RequiereDireccion ?? true;
        if (dto.Estado == EstadosPedido.EnvioEnviado || (dto.Estado == EstadosPedido.EnvioEntregado && actual < 1))
        {
            if (requiereEnvio)
            {
                var guia = string.IsNullOrWhiteSpace(dto.NumeroGuia) ? p.NumeroGuia : dto.NumeroGuia.Trim().ToUpperInvariant();
                if (string.IsNullOrWhiteSpace(guia)) throw new AppException("Captura el número de guía para marcar el pedido como enviado.");
                p.NumeroGuia = guia;
                p.Paqueteria = string.IsNullOrWhiteSpace(dto.Paqueteria) ? (p.Paqueteria ?? p.MetodoEnvio?.Paqueteria) : dto.Paqueteria.Trim();
            }
            _db.PedidoSeguimientos.Add(new PedidoSeguimiento
            {
                PedidoId = p.Id,
                Titulo = requiereEnvio ? "Pedido enviado" : "Listo para recoger",
                Descripcion = requiereEnvio ? $"Tu paquete va en camino con {p.Paqueteria}. Guía: {p.NumeroGuia}." : "Tu pedido está listo. Presenta tu folio e identificación en tienda.",
                Fecha = DateTime.UtcNow
            });
        }

        if (dto.Estado == EstadosPedido.EnvioEntregado)
        {
            p.Estado = EstadosPedido.Completado;
            _db.PedidoSeguimientos.Add(new PedidoSeguimiento
            {
                PedidoId = p.Id,
                Titulo = "Pedido entregado",
                Descripcion = requiereEnvio ? "Tu pedido fue entregado. ¡Gracias por comprar en Reach!" : "Recogiste tu pedido en tienda. ¡Gracias por comprar en Reach!",
                Fecha = DateTime.UtcNow.AddSeconds(1)
            });
        }

        if (dto.Estado == EstadosPedido.EnvioEntregado)
        {
            _notificaciones.Agregar(p.UsuarioId, "Pedido entregado",
                $"Tu pedido {p.Folio} fue entregado. ¡Gracias por comprar en Reach!", TiposNotificacion.Pedido, p.Id);
        }
        else if (requiereEnvio)
        {
            _notificaciones.Agregar(p.UsuarioId, "Tu pedido va en camino",
                $"El pedido {p.Folio} salió con {p.Paqueteria}. Guía: {p.NumeroGuia}.", TiposNotificacion.Pedido, p.Id);
        }
        else
        {
            _notificaciones.Agregar(p.UsuarioId, "Listo para recoger",
                $"Tu pedido {p.Folio} ya te espera en tienda. Presenta tu folio e identificación.", TiposNotificacion.Pedido, p.Id);
        }

        p.EstadoEnvio = dto.Estado;
        p.FechaActualizacion = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return await DetalleAsync(p.Id, 0, true);
    }

    public async Task<ResumenPanelDto> PanelAsync()
    {
        var ahoraMx = DateTime.UtcNow.Add(ZonaMexico);
        var inicioHoy = ahoraMx.Date.Subtract(ZonaMexico);
        var inicioMes = new DateTime(ahoraMx.Year, ahoraMx.Month, 1, 0, 0, 0, DateTimeKind.Utc).Subtract(ZonaMexico);
        inicioHoy = DateTime.SpecifyKind(inicioHoy, DateTimeKind.Utc);
        inicioMes = DateTime.SpecifyKind(inicioMes, DateTimeKind.Utc);

        var validos = _db.Pedidos.AsNoTracking().Where(p => p.Estado != EstadosPedido.Cancelado);
        var pedidosHoy = await validos.CountAsync(p => p.Fecha >= inicioHoy);
        var ventasHoy = await validos.Where(p => p.Fecha >= inicioHoy).SumAsync(p => (decimal?)p.Total) ?? 0m;
        var ventasMes = await validos.Where(p => p.Fecha >= inicioMes).SumAsync(p => (decimal?)p.Total) ?? 0m;
        var pendientes = await validos.CountAsync(p => p.EstadoEnvio == EstadosPedido.EnvioPreparando);
        var ensamble = await validos.CountAsync(p => p.IncluyeEnsamble && p.EstadoArmado != EstadosPedido.ArmadoTerminado);
        var enviados = await validos.CountAsync(p => p.EstadoEnvio == EstadosPedido.EnvioEnviado);
        var stockBajo = await _db.Productos.CountAsync(p => p.Activo && p.Stock <= p.StockMinimo);
        var activos = await _db.Productos.CountAsync(p => p.Activo);
        var clientes = await _db.Usuarios.CountAsync(u => u.Rol == Roles.Comprador);
        var ultimos = await _db.Pedidos.AsNoTracking().Include(p => p.Detalles).Include(p => p.Usuario)
            .OrderByDescending(p => p.Fecha).Take(5).ToListAsync();

        return new ResumenPanelDto(pedidosHoy, ventasHoy, ventasMes, pendientes, ensamble, enviados, stockBajo, activos, clientes,
            ultimos.Select(Resumen).ToList());
    }

    private static bool PuedeCancelar(Pedido p) =>
        p.Estado == EstadosPedido.Pagado
        && p.EstadoEnvio == EstadosPedido.EnvioPreparando
        && (p.EstadoArmado == EstadosPedido.ArmadoNoAplica || p.EstadoArmado == EstadosPedido.ArmadoAtendido);

    private static PedidoResumenDto Resumen(Pedido p) => new(
        p.Id,
        p.Folio,
        p.Fecha,
        p.Total,
        p.Estado,
        p.EstadoArmado,
        p.EstadoEnvio,
        p.IncluyeEnsamble,
        p.Detalles.Sum(d => d.Cantidad),
        p.Detalles.OrderByDescending(d => d.PrecioUnitario).FirstOrDefault()?.ImagenUrl ?? string.Empty,
        p.Usuario?.Nombre ?? string.Empty);

    private static PedidoDetalleDto Detalle(Pedido p) => new(
        p.Id,
        p.Folio,
        p.Fecha,
        p.FechaActualizacion,
        p.Estado,
        p.EstadoArmado,
        p.EstadoEnvio,
        p.IncluyeEnsamble,
        p.Subtotal,
        p.CostoManoObra,
        p.CostoEnvio,
        p.Iva,
        p.Total,
        p.MetodoEnvio?.Nombre ?? string.Empty,
        p.MetodoEnvio?.RequiereDireccion ?? true,
        p.Paqueteria,
        p.NumeroGuia,
        p.FechaEstimadaEntrega,
        p.EnvioNombreRecibe,
        p.EnvioTelefono,
        p.EnvioDireccion,
        p.EnvioCiudad,
        p.EnvioEstado,
        p.EnvioCodigoPostal,
        p.EnvioReferencias,
        p.MetodoPago,
        p.TarjetaUltimos4 is null ? null : $"{p.TarjetaMarca} •••• {p.TarjetaUltimos4}",
        p.Notas,
        p.Usuario?.Nombre ?? string.Empty,
        p.Usuario?.Email ?? string.Empty,
        PuedeCancelar(p),
        p.Detalles
            .OrderBy(d => d.EnsambleId is null)
            .ThenBy(d => d.EnsambleId)
            .ThenBy(d => d.Id)
            .Select(d => new PedidoDetalleItemDto(d.ProductoId, d.Sku, d.NombreProducto, d.ImagenUrl, d.Cantidad, d.PrecioUnitario,
                d.PrecioUnitario * d.Cantidad, d.EsEnsamble, d.EnsambleId, d.EnsambleNombre))
            .ToList(),
        p.Seguimiento.OrderBy(s => s.Fecha).ThenBy(s => s.Id).Select(s => new SeguimientoDto(s.Titulo, s.Descripcion, s.Fecha)).ToList());

    private static string FormatearDireccion(Direccion d)
    {
        var interior = string.IsNullOrWhiteSpace(d.NumeroInterior) ? string.Empty : $" Int. {d.NumeroInterior}";
        return $"{d.Calle} {d.NumeroExterior}{interior}, Col. {d.Colonia}";
    }

    private async Task<string> GenerarFolioAsync()
    {
        for (var i = 0; i < 5; i++)
        {
            var folio = $"RCH-{DateTime.UtcNow.Add(ZonaMexico):yyMMdd}-{RandomNumberGenerator.GetInt32(10000, 100000)}";
            if (!await _db.Pedidos.AnyAsync(p => p.Folio == folio)) return folio;
        }
        return $"RCH-{Guid.NewGuid().ToString("N")[..12].ToUpperInvariant()}";
    }

    public static (string Marca, string Ultimos4) ValidarPago(PagoDto pago)
    {
        var numero = pago.NumeroTarjeta.Trim();
        if (!Luhn(numero)) throw new AppException("El número de tarjeta no es válido.");

        var partes = pago.Vencimiento.Split('/');
        var mes = int.Parse(partes[0]);
        var anio = 2000 + int.Parse(partes[1]);
        var ultimoDia = new DateTime(anio, mes, DateTime.DaysInMonth(anio, mes), 23, 59, 59, DateTimeKind.Utc);
        if (ultimoDia < DateTime.UtcNow) throw new AppException("La tarjeta está vencida.");

        var marca = numero switch
        {
            _ when numero.StartsWith('4') => "Visa",
            _ when numero.Length == 15 && (numero.StartsWith("34") || numero.StartsWith("37")) => "Amex",
            _ when numero.StartsWith('5') || numero.StartsWith('2') => "Mastercard",
            _ => "Tarjeta"
        };

        if (marca == "Amex" ? pago.Cvv.Length != 4 : pago.Cvv.Length != 3) throw new AppException("El CVV no es válido.");
        if (numero.EndsWith("0002")) throw new AppException("El banco emisor rechazó el pago. Intenta con otra tarjeta.", StatusCodes.Status402PaymentRequired);

        return (marca, numero[^4..]);
    }

    private static bool Luhn(string numero)
    {
        var suma = 0;
        var doble = false;
        for (var i = numero.Length - 1; i >= 0; i--)
        {
            if (!char.IsDigit(numero[i])) return false;
            var d = numero[i] - '0';
            if (doble)
            {
                d *= 2;
                if (d > 9) d -= 9;
            }
            suma += d;
            doble = !doble;
        }
        return suma % 10 == 0;
    }
}
