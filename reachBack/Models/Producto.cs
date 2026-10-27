namespace reachBack.Models
{
    public class Producto
    {
        public int Id { get; set; }

        // Identificación y Datos Básicos
        public string Sku { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;

        // Precios (PrecioAnterior - nullable para cuando no hay descuento)
        public decimal Precio { get; set; }
        public decimal? PrecioAnterior { get; set; }

        // Control de Inventario y Stock
        public int Stock { get; set; }
        public int StockMinimo { get; set; } = 5;
        public string TipoInventario { get; set; } = "Almacen";

        // Multimedia y Especificaciones
        public string ImagenUrl { get; set; } = string.Empty;
        public string EspecificacionesTecnicas { get; set; } = string.Empty;

        public bool? Envio { get; set; }

        // Relación con Categoría
        public int CategoriaId { get; set; }
        public Categoria? Categoria { get; set; }

    }
}