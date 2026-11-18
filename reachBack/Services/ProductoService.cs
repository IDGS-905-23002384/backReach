using Microsoft.EntityFrameworkCore;
using reachBack.Data;
using reachBack.Dtos;
using reachBack.Infrastructure;
using reachBack.Models;

namespace reachBack.Services;

public class ProductoService
{
    private readonly ApplicationDbContext _db;
    private readonly InventarioService _inventario;

    public ProductoService(ApplicationDbContext db, InventarioService inventario)
    {
        _db = db;
        _inventario = inventario;
    }

    public async Task<PaginaDto<ProductoListaDto>> ListarAsync(ProductoFiltroDto f, bool esPersonal)
    {
        var query = _db.Productos.AsNoTracking().Include(p => p.Categoria).AsQueryable();

        if (!(esPersonal && f.IncluirInactivos)) query = query.Where(p => p.Activo);
        if (f.CategoriaId.HasValue) query = query.Where(p => p.CategoriaId == f.CategoriaId.Value);
        if (!string.IsNullOrWhiteSpace(f.Marca)) query = query.Where(p => p.Marca == f.Marca.Trim());
        if (f.SoloOfertas) query = query.Where(p => p.PrecioAnterior != null && p.PrecioAnterior > p.Precio);
        if (f.SoloDestacados) query = query.Where(p => p.Destacado);

        if (!string.IsNullOrWhiteSpace(f.Q))
        {
            var terminos = f.Q.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(5);
            foreach (var t in terminos)
            {
                var termino = t;
                query = query.Where(p => p.Nombre.Contains(termino) || p.Marca.Contains(termino) || p.Sku.Contains(termino) || p.Categoria!.Nombre.Contains(termino));
            }
        }

        query = f.Orden switch
        {
            "precio_asc" => query.OrderBy(p => p.Precio),
            "precio_desc" => query.OrderByDescending(p => p.Precio),
            "nombre" => query.OrderBy(p => p.Nombre),
            "recientes" => query.OrderByDescending(p => p.FechaAlta).ThenByDescending(p => p.Id),
            "descuento" => query.OrderByDescending(p => p.PrecioAnterior == null ? 0 : (p.PrecioAnterior.Value - p.Precio) / p.PrecioAnterior.Value),
            _ => query.OrderByDescending(p => p.Destacado).ThenByDescending(p => p.Stock > 0).ThenBy(p => p.Categoria!.Orden).ThenBy(p => p.Precio)
        };

        var total = await query.CountAsync();
        var items = await query.Skip((f.Pagina - 1) * f.Tamano).Take(f.Tamano).ToListAsync();
        var paginas = (int)Math.Ceiling(total / (double)f.Tamano);
        return new PaginaDto<ProductoListaDto>(items.Select(p => p.ALista()).ToList(), f.Pagina, f.Tamano, total, paginas);
    }

    public async Task<ProductoDetalleDto> ObtenerAsync(int id, bool esPersonal)
    {
        var p = await _db.Productos.AsNoTracking()
            .Include(x => x.Categoria)
            .Include(x => x.Proveedor)
            .Include(x => x.Especificacion)
            .FirstOrDefaultAsync(x => x.Id == id && (esPersonal || x.Activo));

        if (p is null) throw AppException.NoEncontrado("Producto no encontrado.");
        return Detalle(p);
    }

    public async Task<List<string>> MarcasAsync(int? categoriaId)
    {
        var query = _db.Productos.AsNoTracking().Where(p => p.Activo);
        if (categoriaId.HasValue) query = query.Where(p => p.CategoriaId == categoriaId.Value);
        return await query.Select(p => p.Marca).Distinct().OrderBy(m => m).ToListAsync();
    }

    public async Task<ProductoDetalleDto> CrearAsync(ProductoGuardarDto dto, string responsable)
    {
        await ValidarAsync(dto, null);
        var producto = new Producto();
        Aplicar(producto, dto);
        producto.Stock = 0;
        producto.FechaAlta = DateTime.UtcNow;
        _db.Productos.Add(producto);
        await _db.SaveChangesAsync();

        if (dto.Stock > 0)
        {
            await _inventario.RegistrarAsync(producto, "Entrada", dto.Stock, "Inventario inicial", responsable, null);
            await _db.SaveChangesAsync();
        }

        return await ObtenerAsync(producto.Id, true);
    }

    public async Task<ProductoDetalleDto> ActualizarAsync(int id, ProductoGuardarDto dto, string responsable)
    {
        var producto = await _db.Productos.Include(p => p.Especificacion).FirstOrDefaultAsync(p => p.Id == id)
            ?? throw AppException.NoEncontrado("Producto no encontrado.");

        await ValidarAsync(dto, id);
        var stockAnterior = producto.Stock;
        Aplicar(producto, dto);
        producto.Stock = stockAnterior;

        var diferencia = dto.Stock - stockAnterior;
        if (diferencia != 0)
        {
            await _inventario.RegistrarAsync(producto, diferencia > 0 ? "Entrada" : "Salida", Math.Abs(diferencia), "Ajuste desde ficha de producto", responsable, null);
        }

        await _db.SaveChangesAsync();
        return await ObtenerAsync(producto.Id, true);
    }

    public async Task CambiarEstadoAsync(int id, bool activo)
    {
        var producto = await _db.Productos.FirstOrDefaultAsync(p => p.Id == id)
            ?? throw AppException.NoEncontrado("Producto no encontrado.");
        producto.Activo = activo;
        if (!activo)
        {
            var enCarritos = await _db.CarritoItems.Where(c => c.ProductoId == id).ToListAsync();
            _db.CarritoItems.RemoveRange(enCarritos);
        }
        await _db.SaveChangesAsync();
    }

    private async Task ValidarAsync(ProductoGuardarDto dto, int? id)
    {
        var sku = dto.Sku.Trim().ToUpperInvariant();
        if (await _db.Productos.AnyAsync(p => p.Sku == sku && p.Id != (id ?? 0)))
        {
            throw AppException.Conflicto("Ya existe un producto con ese SKU.");
        }
        if (!await _db.Categorias.AnyAsync(c => c.Id == dto.CategoriaId))
        {
            throw new AppException("La categoría seleccionada no existe.");
        }
        if (dto.ProveedorId.HasValue && !await _db.Proveedores.AnyAsync(p => p.Id == dto.ProveedorId.Value))
        {
            throw new AppException("El proveedor seleccionado no existe.");
        }
        if (dto.PrecioAnterior.HasValue && dto.PrecioAnterior.Value > 0 && dto.PrecioAnterior.Value <= dto.Precio)
        {
            throw new AppException("El precio anterior debe ser mayor al precio actual para mostrarse como oferta.");
        }
        if (!string.IsNullOrWhiteSpace(dto.ImagenUrl) && !dto.ImagenUrl.StartsWith('/') && !Uri.TryCreate(dto.ImagenUrl, UriKind.Absolute, out var uri))
        {
            throw new AppException("La URL de imagen no es válida.");
        }
        if (!string.IsNullOrWhiteSpace(dto.ImagenUrl) && Uri.TryCreate(dto.ImagenUrl, UriKind.Absolute, out var absoluta)
            && absoluta.Scheme != Uri.UriSchemeHttps && absoluta.Scheme != Uri.UriSchemeHttp)
        {
            throw new AppException("La imagen debe usar http o https.");
        }
    }

    private static void Aplicar(Producto p, ProductoGuardarDto dto)
    {
        p.Sku = dto.Sku.Trim().ToUpperInvariant();
        p.Nombre = dto.Nombre.Trim();
        p.Marca = dto.Marca.Trim();
        p.Descripcion = dto.Descripcion?.Trim() ?? string.Empty;
        p.Precio = dto.Precio;
        p.PrecioAnterior = dto.PrecioAnterior is > 0m ? dto.PrecioAnterior : null;
        p.StockMinimo = dto.StockMinimo;
        p.ImagenUrl = dto.ImagenUrl?.Trim() ?? string.Empty;
        p.EspecificacionesTecnicas = dto.EspecificacionesTecnicas?.Trim() ?? string.Empty;
        p.CategoriaId = dto.CategoriaId;
        p.ProveedorId = dto.ProveedorId;
        p.Envio = dto.Envio;
        p.Destacado = dto.Destacado;
        p.Activo = dto.Activo;

        if (dto.Especificacion is not null)
        {
            p.Especificacion ??= new EspecificacionComponente();
            p.Especificacion.Aplicar(dto.Especificacion);
        }
    }

    public static ProductoDetalleDto Detalle(Producto p) => new(
        p.Id,
        p.Sku,
        p.Nombre,
        p.Marca,
        p.Descripcion,
        p.Precio,
        p.PrecioAnterior,
        Mapeos.Descuento(p.Precio, p.PrecioAnterior),
        p.Stock,
        p.StockMinimo,
        p.ImagenUrl,
        p.CategoriaId,
        p.Categoria?.Nombre ?? string.Empty,
        p.Categoria?.TipoComponente,
        p.Destacado,
        p.Activo,
        p.Envio,
        p.ProveedorId,
        p.Proveedor?.NombreEmpresa,
        p.EspecificacionesTecnicas,
        Mapeos.Lineas(p.EspecificacionesTecnicas),
        p.Especificacion.ADto());
}
