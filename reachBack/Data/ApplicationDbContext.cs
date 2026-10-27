using Microsoft.EntityFrameworkCore;
using reachBack.Models;

namespace reachBack.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<FichaTecnica> FichasTecnicas { get; set; }
        public DbSet<FichaTecnicaDetalle> FichaTecnicaDetalles { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<PedidoDetalle> PedidoDetalles { get; set; }
        public DbSet<CarritoItem> CarritoItems { get; set; }
        public DbSet<Movimiento> MovimientosKardex { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Datos iniciales para Categorías
            modelBuilder.Entity<Categoria>().HasData(
                new Categoria { Id = 1, Nombre = "Gráficas" },
                new Categoria { Id = 2, Nombre = "Procesadores" }
            );

            // 2. Datos iniciales para Productos
            modelBuilder.Entity<Producto>().HasData(
                new Producto
                {
                    Id = 1,
                    Sku = "RTX4070-SUP",
                    Nombre = "RTX 4070 SUPER",
                    Marca = "ASUS",
                    Descripcion = "Dual OC - 12 GB GDDR6X",
                    Precio = 13899.00m,
                    PrecioAnterior = 15499.00m,
                    Stock = 8,
                    StockMinimo = 5,
                    TipoInventario = "Almacen",
                    ImagenUrl = "https://images.unsplash.com/photo-1587202372775-e229f172b9d7",
                    EspecificacionesTecnicas = "12 GB GDDR6X, PCI Express 4.0",
                    Envio = true,
                    CategoriaId = 1
                }
            );
        }
    }
}