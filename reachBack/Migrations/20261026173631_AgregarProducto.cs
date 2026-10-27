using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace reachBack.Migrations
{
    /// <inheritdoc />
    public partial class AgregarProducto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Categorias",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1, "Gráficas" },
                    { 2, "Procesadores" }
                });

            migrationBuilder.InsertData(
                table: "Productos",
                columns: new[] { "Id", "CategoriaId", "Descripcion", "Envio", "EspecificacionesTecnicas", "ImagenUrl", "Marca", "Nombre", "Precio", "PrecioAnterior", "ProveedorId", "Sku", "Stock", "StockMinimo", "TipoInventario" },
                values: new object[] { 1, 1, "Dual OC - 12 GB GDDR6X", true, "12 GB GDDR6X, PCI Express 4.0", "https://images.unsplash.com/photo-1587202372775-e229f172b9d7", "ASUS", "RTX 4070 SUPER", 13899.00m, 15499.00m, null, "RTX4070-SUP", 8, 5, "Almacen" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
