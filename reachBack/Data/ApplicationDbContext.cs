using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using reachBack.Models;

namespace reachBack.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<EspecificacionComponente> Especificaciones => Set<EspecificacionComponente>();
    public DbSet<FichaTecnica> FichasTecnicas => Set<FichaTecnica>();
    public DbSet<FichaTecnicaDetalle> FichaTecnicaDetalles => Set<FichaTecnicaDetalle>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<PedidoDetalle> PedidoDetalles => Set<PedidoDetalle>();
    public DbSet<PedidoSeguimiento> PedidoSeguimientos => Set<PedidoSeguimiento>();
    public DbSet<CarritoItem> CarritoItems => Set<CarritoItem>();
    public DbSet<Direccion> Direcciones => Set<Direccion>();
    public DbSet<MetodoEnvio> MetodosEnvio => Set<MetodoEnvio>();
    public DbSet<Movimiento> MovimientosKardex => Set<Movimiento>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
        configurationBuilder.Properties<DateTime?>().HaveConversion<UtcNullableDateTimeConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>(e =>
        {
            e.Property(x => x.Nombre).HasMaxLength(120).IsRequired();
            e.Property(x => x.Email).HasMaxLength(160).IsRequired();
            e.Property(x => x.PasswordHash).HasMaxLength(100).IsRequired();
            e.Property(x => x.Telefono).HasMaxLength(20);
            e.Property(x => x.Rol).HasMaxLength(20).IsRequired();
            e.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<Categoria>(e =>
        {
            e.Property(x => x.Nombre).HasMaxLength(80).IsRequired();
            e.Property(x => x.Slug).HasMaxLength(80).IsRequired();
            e.Property(x => x.TipoComponente).HasMaxLength(20);
            e.HasIndex(x => x.Slug).IsUnique();
        });

        modelBuilder.Entity<Proveedor>(e =>
        {
            e.Property(x => x.NombreEmpresa).HasMaxLength(150).IsRequired();
            e.Property(x => x.ContactoPersona).HasMaxLength(120);
            e.Property(x => x.Telefono).HasMaxLength(20);
            e.Property(x => x.Email).HasMaxLength(160);
        });

        modelBuilder.Entity<Producto>(e =>
        {
            e.Property(x => x.Sku).HasMaxLength(40).IsRequired();
            e.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
            e.Property(x => x.Marca).HasMaxLength(80).IsRequired();
            e.Property(x => x.Descripcion).HasMaxLength(2000);
            e.Property(x => x.TipoInventario).HasMaxLength(30);
            e.Property(x => x.ImagenUrl).HasMaxLength(500);
            e.Property(x => x.EspecificacionesTecnicas).HasMaxLength(4000);
            e.HasIndex(x => x.Sku).IsUnique();
            e.HasIndex(x => new { x.CategoriaId, x.Activo });
            e.HasOne(x => x.Categoria).WithMany(c => c.Productos).HasForeignKey(x => x.CategoriaId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Proveedor).WithMany(p => p.Productos).HasForeignKey(x => x.ProveedorId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Especificacion).WithOne(s => s.Producto).HasForeignKey<EspecificacionComponente>(s => s.ProductoId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EspecificacionComponente>(e =>
        {
            e.ToTable("EspecificacionesComponente");
            e.HasKey(x => x.ProductoId);
            e.Property(x => x.Socket).HasMaxLength(20);
            e.Property(x => x.SocketsSoportados).HasMaxLength(100);
            e.Property(x => x.TipoMemoria).HasMaxLength(10);
            e.Property(x => x.FormFactor).HasMaxLength(20);
            e.Property(x => x.FormFactorsSoportados).HasMaxLength(60);
            e.Property(x => x.Interfaz).HasMaxLength(20);
        });

        modelBuilder.Entity<CarritoItem>(e =>
        {
            e.Property(x => x.EnsambleId).HasMaxLength(40);
            e.Property(x => x.EnsambleNombre).HasMaxLength(120);
            e.HasOne(x => x.Usuario).WithMany(u => u.CarritoItems).HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Producto).WithMany().HasForeignKey(x => x.ProductoId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Direccion>(e =>
        {
            e.ToTable("Direcciones");
            e.Property(x => x.Alias).HasMaxLength(40);
            e.Property(x => x.NombreRecibe).HasMaxLength(120).IsRequired();
            e.Property(x => x.Telefono).HasMaxLength(20).IsRequired();
            e.Property(x => x.Calle).HasMaxLength(150).IsRequired();
            e.Property(x => x.NumeroExterior).HasMaxLength(20).IsRequired();
            e.Property(x => x.NumeroInterior).HasMaxLength(20);
            e.Property(x => x.Colonia).HasMaxLength(120).IsRequired();
            e.Property(x => x.Ciudad).HasMaxLength(120).IsRequired();
            e.Property(x => x.Estado).HasMaxLength(80).IsRequired();
            e.Property(x => x.CodigoPostal).HasMaxLength(5).IsRequired();
            e.Property(x => x.Referencias).HasMaxLength(250);
            e.HasOne(x => x.Usuario).WithMany(u => u.Direcciones).HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MetodoEnvio>(e =>
        {
            e.ToTable("MetodosEnvio");
            e.Property(x => x.Nombre).HasMaxLength(80).IsRequired();
            e.Property(x => x.Descripcion).HasMaxLength(250);
            e.Property(x => x.Paqueteria).HasMaxLength(80);
        });

        modelBuilder.Entity<Pedido>(e =>
        {
            e.Property(x => x.Folio).HasMaxLength(30).IsRequired();
            e.Property(x => x.Estado).HasMaxLength(30);
            e.Property(x => x.EstadoArmado).HasMaxLength(30);
            e.Property(x => x.EstadoEnvio).HasMaxLength(30);
            e.Property(x => x.Paqueteria).HasMaxLength(80);
            e.Property(x => x.NumeroGuia).HasMaxLength(60);
            e.Property(x => x.EnvioNombreRecibe).HasMaxLength(120);
            e.Property(x => x.EnvioTelefono).HasMaxLength(20);
            e.Property(x => x.EnvioDireccion).HasMaxLength(400);
            e.Property(x => x.EnvioCiudad).HasMaxLength(120);
            e.Property(x => x.EnvioEstado).HasMaxLength(80);
            e.Property(x => x.EnvioCodigoPostal).HasMaxLength(5);
            e.Property(x => x.EnvioReferencias).HasMaxLength(250);
            e.Property(x => x.MetodoPago).HasMaxLength(30);
            e.Property(x => x.TarjetaMarca).HasMaxLength(20);
            e.Property(x => x.TarjetaUltimos4).HasMaxLength(4);
            e.Property(x => x.Notas).HasMaxLength(500);
            e.HasIndex(x => x.Folio).IsUnique();
            e.HasIndex(x => x.Fecha);
            e.HasOne(x => x.Usuario).WithMany(u => u.Pedidos).HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.MetodoEnvio).WithMany().HasForeignKey(x => x.MetodoEnvioId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PedidoDetalle>(e =>
        {
            e.Property(x => x.Sku).HasMaxLength(40);
            e.Property(x => x.NombreProducto).HasMaxLength(200);
            e.Property(x => x.ImagenUrl).HasMaxLength(500);
            e.Property(x => x.EnsambleId).HasMaxLength(40);
            e.Property(x => x.EnsambleNombre).HasMaxLength(120);
            e.HasOne(x => x.Pedido).WithMany(p => p.Detalles).HasForeignKey(x => x.PedidoId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Producto).WithMany().HasForeignKey(x => x.ProductoId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PedidoSeguimiento>(e =>
        {
            e.ToTable("PedidoSeguimientos");
            e.Property(x => x.Titulo).HasMaxLength(80);
            e.Property(x => x.Descripcion).HasMaxLength(300);
            e.HasOne(x => x.Pedido).WithMany(p => p.Seguimiento).HasForeignKey(x => x.PedidoId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Movimiento>(e =>
        {
            e.Property(x => x.TipoMovimiento).HasMaxLength(10);
            e.Property(x => x.Motivo).HasMaxLength(200);
            e.Property(x => x.AdministradorResponsable).HasMaxLength(160);
            e.HasIndex(x => x.Fecha);
            e.HasOne(x => x.Producto).WithMany().HasForeignKey(x => x.ProductoId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FichaTecnica>(e =>
        {
            e.Property(x => x.NombreEquipoPrearmado).HasMaxLength(120).IsRequired();
            e.Property(x => x.CategoriaUso).HasMaxLength(60);
            e.Property(x => x.Descripcion).HasMaxLength(500);
        });

        modelBuilder.Entity<FichaTecnicaDetalle>(e =>
        {
            e.HasOne(x => x.FichaTecnica).WithMany(f => f.ComponentesRequeridos).HasForeignKey(x => x.FichaTecnicaId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Producto).WithMany().HasForeignKey(x => x.ProductoId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}

public class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter()
        : base(v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(), v => DateTime.SpecifyKind(v, DateTimeKind.Utc)) { }
}

public class UtcNullableDateTimeConverter : ValueConverter<DateTime?, DateTime?>
{
    public UtcNullableDateTimeConverter()
        : base(
            v => v.HasValue ? (v.Value.Kind == DateTimeKind.Utc ? v.Value : v.Value.ToUniversalTime()) : v,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v) { }
}
