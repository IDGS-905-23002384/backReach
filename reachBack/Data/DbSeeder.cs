using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using reachBack.Dtos;
using reachBack.Models;
using reachBack.Services;

namespace reachBack.Data;

public static class DbSeeder
{
    public sealed class SeedCatalogo
    {
        public List<SeedCategoria> Categorias { get; set; } = new();
        public List<SeedProveedor> Proveedores { get; set; } = new();
        public List<SeedProducto> Productos { get; set; } = new();
        public List<SeedPrearmado> Prearmados { get; set; } = new();
    }

    public sealed class SeedCategoria
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? TipoComponente { get; set; }
        public int Orden { get; set; }
    }

    public sealed class SeedProveedor
    {
        public int Id { get; set; }
        public string NombreEmpresa { get; set; } = string.Empty;
        public string ContactoPersona { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public sealed class SeedProducto
    {
        public string Sku { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public decimal? PrecioAnterior { get; set; }
        public int Stock { get; set; }
        public int StockMinimo { get; set; }
        public string ImagenUrl { get; set; } = string.Empty;
        public string EspecificacionesTecnicas { get; set; } = string.Empty;
        public int CategoriaId { get; set; }
        public int ProveedorId { get; set; }
        public bool Destacado { get; set; }
        public EspecificacionComponenteDto? Especificacion { get; set; }
    }

    public sealed class SeedPrearmado
    {
        public string Nombre { get; set; } = string.Empty;
        public string Uso { get; set; } = string.Empty;
        public decimal ManoObra { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public List<string> Componentes { get; set; } = new();
    }

    public static async Task SeedAsync(ApplicationDbContext db, IWebHostEnvironment env, ILogger logger)
    {
        if (!await db.Categorias.AnyAsync())
        {
            await SembrarCatalogoAsync(db, env, logger);
        }

        if (!await db.MetodosEnvio.AnyAsync())
        {
            db.MetodosEnvio.AddRange(
                new MetodoEnvio { Nombre = "Envío estándar", Descripcion = "Entrega a domicilio en 3 a 5 días hábiles.", Paqueteria = "Paquetexpress", Costo = 149m, DiasMin = 3, DiasMax = 5, AplicaEnvioGratis = true, RequiereDireccion = true },
                new MetodoEnvio { Nombre = "Envío express", Descripcion = "Entrega a domicilio en 1 a 2 días hábiles.", Paqueteria = "Estafeta", Costo = 299m, DiasMin = 1, DiasMax = 2, AplicaEnvioGratis = false, RequiereDireccion = true },
                new MetodoEnvio { Nombre = "Recoger en tienda", Descripcion = "Blvd. Adolfo López Mateos 1502, León, Gto. Sin costo.", Paqueteria = "Tienda Reach León", Costo = 0m, DiasMin = 0, DiasMax = 1, AplicaEnvioGratis = false, RequiereDireccion = false });
            await db.SaveChangesAsync();
        }

        if (!await db.Usuarios.AnyAsync())
        {
            await SembrarUsuariosAsync(db, logger);
        }
    }

    private static async Task SembrarCatalogoAsync(ApplicationDbContext db, IWebHostEnvironment env, ILogger logger)
    {
        var ruta = Path.Combine(AppContext.BaseDirectory, "Data", "Seed", "catalogo.json");
        if (!File.Exists(ruta)) ruta = Path.Combine(env.ContentRootPath, "Data", "Seed", "catalogo.json");
        if (!File.Exists(ruta))
        {
            logger.LogWarning("No se encontró el archivo de catálogo inicial en {Ruta}", ruta);
            return;
        }

        await using var stream = File.OpenRead(ruta);
        var seed = await JsonSerializer.DeserializeAsync<SeedCatalogo>(stream, new JsonSerializerOptions(JsonSerializerDefaults.Web))
            ?? throw new InvalidOperationException("Catálogo inicial inválido.");

        var categorias = seed.Categorias.ToDictionary(c => c.Id, c => new Categoria { Nombre = c.Nombre, Slug = c.Slug, TipoComponente = c.TipoComponente, Orden = c.Orden });
        var proveedores = seed.Proveedores.ToDictionary(p => p.Id, p => new Proveedor { NombreEmpresa = p.NombreEmpresa, ContactoPersona = p.ContactoPersona, Telefono = p.Telefono, Email = p.Email });
        db.Categorias.AddRange(categorias.Values);
        db.Proveedores.AddRange(proveedores.Values);

        var fechaAlta = DateTime.UtcNow.AddDays(-30);
        var productos = new Dictionary<string, Producto>();
        var indice = 0;
        foreach (var s in seed.Productos)
        {
            var p = new Producto
            {
                Sku = s.Sku,
                Nombre = s.Nombre,
                Marca = s.Marca,
                Descripcion = s.Descripcion,
                Precio = s.Precio,
                PrecioAnterior = s.PrecioAnterior,
                Stock = s.Stock,
                StockMinimo = s.StockMinimo,
                ImagenUrl = s.ImagenUrl,
                EspecificacionesTecnicas = s.EspecificacionesTecnicas,
                Categoria = categorias[s.CategoriaId],
                Proveedor = proveedores.TryGetValue(s.ProveedorId, out var prov) ? prov : null,
                Destacado = s.Destacado,
                Envio = true,
                Activo = true,
                FechaAlta = fechaAlta.AddMinutes(indice++)
            };
            if (s.Especificacion is not null)
            {
                p.Especificacion = new EspecificacionComponente();
                p.Especificacion.Aplicar(s.Especificacion);
            }
            productos[s.Sku] = p;
            db.Productos.Add(p);
            db.MovimientosKardex.Add(new Movimiento
            {
                Producto = p,
                TipoMovimiento = "Entrada",
                Cantidad = s.Stock,
                StockResultante = s.Stock,
                Motivo = "Inventario inicial",
                AdministradorResponsable = "sistema",
                Fecha = fechaAlta
            });
        }

        foreach (var pa in seed.Prearmados)
        {
            var ficha = new FichaTecnica
            {
                NombreEquipoPrearmado = pa.Nombre,
                CategoriaUso = pa.Uso,
                Descripcion = pa.Descripcion,
                CostoManoObra = pa.ManoObra,
                Activo = true
            };
            foreach (var sku in pa.Componentes)
            {
                if (productos.TryGetValue(sku, out var prod))
                {
                    ficha.ComponentesRequeridos.Add(new FichaTecnicaDetalle { Producto = prod, Cantidad = 1 });
                }
            }
            ficha.PrecioSugerido = ficha.ComponentesRequeridos.Sum(c => c.Producto!.Precio) + ficha.CostoManoObra;
            db.FichasTecnicas.Add(ficha);
        }

        await db.SaveChangesAsync();
        logger.LogInformation("Catálogo inicial cargado: {Total} productos", productos.Count);
    }

    private static async Task SembrarUsuariosAsync(ApplicationDbContext db, ILogger logger)
    {
        var admin = new Usuario { Nombre = "Alex Moreno", Email = "admin@reach.mx", Telefono = "4771234567", Rol = Roles.Administrador, PasswordHash = AuthService.Hash("Admin2026!") };
        var empleado = new Usuario { Nombre = "Sofía Ramírez", Email = "empleado@reach.mx", Telefono = "4777654321", Rol = Roles.Empleado, PasswordHash = AuthService.Hash("Empleado2026!") };
        var cliente = new Usuario { Nombre = "Carlos Sánchez", Email = "cliente@reach.mx", Telefono = "4779876543", Rol = Roles.Comprador, PasswordHash = AuthService.Hash("Cliente2026!") };
        db.Usuarios.AddRange(admin, empleado, cliente);

        var direccion = new Direccion
        {
            Usuario = cliente,
            Alias = "Casa",
            NombreRecibe = "Carlos Sánchez",
            Telefono = "4779876543",
            Calle = "Blvd. Torres Landa",
            NumeroExterior = "1701",
            Colonia = "Jardines del Moral",
            Ciudad = "León",
            Estado = "Guanajuato",
            CodigoPostal = "37160",
            Referencias = "Portón negro, frente al parque",
            EsPredeterminada = true
        };
        db.Direcciones.Add(direccion);
        await db.SaveChangesAsync();

        var estandar = await db.MetodosEnvio.OrderBy(m => m.Id).FirstAsync();
        var mouse = await db.Productos.FirstOrDefaultAsync(p => p.Sku == "LOG-G502-HERO");
        var audifonos = await db.Productos.FirstOrDefaultAsync(p => p.Sku == "HYX-CLOUD-II");
        if (mouse is null || audifonos is null) return;

        var fecha = DateTime.UtcNow.AddDays(-9);
        var subtotal = mouse.Precio + audifonos.Precio;
        var total = subtotal;
        var pedido = new Pedido
        {
            Folio = $"RCH-{fecha:yyMMdd}-10001",
            Usuario = cliente,
            Fecha = fecha,
            FechaActualizacion = fecha.AddDays(4),
            Subtotal = subtotal,
            CostoEnvio = 0m,
            Total = total,
            Iva = Math.Round(total - total / 1.16m, 2),
            Estado = EstadosPedido.Completado,
            EstadoArmado = EstadosPedido.ArmadoNoAplica,
            EstadoEnvio = EstadosPedido.EnvioEntregado,
            MetodoEnvio = estandar,
            Paqueteria = estandar.Paqueteria,
            NumeroGuia = "PQX4471029384",
            FechaEstimadaEntrega = fecha.AddDays(5),
            EnvioNombreRecibe = direccion.NombreRecibe,
            EnvioTelefono = direccion.Telefono,
            EnvioDireccion = "Blvd. Torres Landa 1701, Col. Jardines del Moral",
            EnvioCiudad = direccion.Ciudad,
            EnvioEstado = direccion.Estado,
            EnvioCodigoPostal = direccion.CodigoPostal,
            EnvioReferencias = direccion.Referencias,
            TarjetaMarca = "Visa",
            TarjetaUltimos4 = "4242"
        };
        foreach (var p in new[] { mouse, audifonos })
        {
            pedido.Detalles.Add(new PedidoDetalle { Producto = p, Sku = p.Sku, NombreProducto = p.Nombre, ImagenUrl = p.ImagenUrl, Cantidad = 1, PrecioUnitario = p.Precio });
            p.Stock -= 1;
            db.MovimientosKardex.Add(new Movimiento { Producto = p, TipoMovimiento = "Salida", Cantidad = 1, StockResultante = p.Stock, Motivo = $"Venta {pedido.Folio}", AdministradorResponsable = cliente.Email, Fecha = fecha });
        }
        pedido.Seguimiento.Add(new PedidoSeguimiento { Titulo = "Pedido recibido", Descripcion = "Recibimos tu pedido y tu pago fue aprobado.", Fecha = fecha });
        pedido.Seguimiento.Add(new PedidoSeguimiento { Titulo = "Pedido enviado", Descripcion = "Tu paquete va en camino con Paquetexpress. Guía: PQX4471029384.", Fecha = fecha.AddDays(1) });
        pedido.Seguimiento.Add(new PedidoSeguimiento { Titulo = "Pedido entregado", Descripcion = "Tu pedido fue entregado. ¡Gracias por comprar en Reach!", Fecha = fecha.AddDays(4) });
        db.Pedidos.Add(pedido);
        await db.SaveChangesAsync();

        foreach (var m in await db.MovimientosKardex.Where(m => m.Motivo == $"Venta {pedido.Folio}").ToListAsync())
        {
            m.PedidoId = pedido.Id;
        }
        await db.SaveChangesAsync();

        db.Notificaciones.AddRange(
            new Notificacion { UsuarioId = cliente.Id, Titulo = "Pedido entregado", Mensaje = $"Tu pedido {pedido.Folio} fue entregado. ¡Gracias por comprar en Reach!", Tipo = TiposNotificacion.Pedido, PedidoId = pedido.Id, Leida = true, Fecha = fecha.AddDays(4) },
            new Notificacion { UsuarioId = cliente.Id, Titulo = "¡Bienvenido a Reach!", Mensaje = "Arma tu PC con piezas 100% compatibles y recibe aquí cada avance de tus pedidos.", Tipo = TiposNotificacion.Sistema, Fecha = DateTime.UtcNow });
        await db.SaveChangesAsync();
        logger.LogInformation("Usuarios de demostración creados");
    }
}
