using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using reachBack.Data;
using reachBack.Dtos;
using reachBack.Infrastructure;
using reachBack.Models;

namespace reachBack.Services;

public class CatalogoAdminService
{
    private readonly ApplicationDbContext _db;

    public CatalogoAdminService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<CategoriaDto>> CategoriasAsync()
    {
        return await _db.Categorias.AsNoTracking()
            .OrderBy(c => c.Orden).ThenBy(c => c.Nombre)
            .Select(c => new CategoriaDto(c.Id, c.Nombre, c.Slug, c.TipoComponente, c.Orden, c.Productos.Count(p => p.Activo),
                c.Productos.Where(p => p.Activo).OrderByDescending(p => p.Destacado).ThenBy(p => p.Precio).Select(p => p.ImagenUrl).FirstOrDefault()))
            .ToListAsync();
    }

    public async Task<CategoriaDto> GuardarCategoriaAsync(int? id, CategoriaGuardarDto dto)
    {
        var slug = Slug(dto.Nombre);
        if (await _db.Categorias.AnyAsync(c => c.Slug == slug && c.Id != (id ?? 0)))
        {
            throw AppException.Conflicto("Ya existe una categoría con ese nombre.");
        }
        if (dto.TipoComponente is not null && await _db.Categorias.AnyAsync(c => c.TipoComponente == dto.TipoComponente && c.Id != (id ?? 0)))
        {
            throw AppException.Conflicto("Ya existe una categoría asignada a ese paso del configurador.");
        }

        Categoria categoria;
        if (id.HasValue)
        {
            categoria = await _db.Categorias.FirstOrDefaultAsync(c => c.Id == id.Value) ?? throw AppException.NoEncontrado("Categoría no encontrada.");
        }
        else
        {
            categoria = new Categoria();
            _db.Categorias.Add(categoria);
        }

        categoria.Nombre = dto.Nombre.Trim();
        categoria.Slug = slug;
        categoria.TipoComponente = dto.TipoComponente;
        categoria.Orden = dto.Orden;
        await _db.SaveChangesAsync();
        return new CategoriaDto(categoria.Id, categoria.Nombre, categoria.Slug, categoria.TipoComponente, categoria.Orden,
            await _db.Productos.CountAsync(p => p.CategoriaId == categoria.Id && p.Activo), null);
    }

    public async Task EliminarCategoriaAsync(int id)
    {
        var categoria = await _db.Categorias.FirstOrDefaultAsync(c => c.Id == id) ?? throw AppException.NoEncontrado("Categoría no encontrada.");
        if (await _db.Productos.AnyAsync(p => p.CategoriaId == id))
        {
            throw AppException.Conflicto("No puedes eliminar una categoría que tiene productos.");
        }
        _db.Categorias.Remove(categoria);
        await _db.SaveChangesAsync();
    }

    public async Task<List<ProveedorDto>> ProveedoresAsync()
    {
        return await _db.Proveedores.AsNoTracking()
            .OrderBy(p => p.NombreEmpresa)
            .Select(p => new ProveedorDto(p.Id, p.NombreEmpresa, p.ContactoPersona, p.Telefono, p.Email, p.Activo, p.Productos.Count))
            .ToListAsync();
    }

    public async Task<ProveedorDto> GuardarProveedorAsync(int? id, ProveedorGuardarDto dto)
    {
        Proveedor proveedor;
        if (id.HasValue)
        {
            proveedor = await _db.Proveedores.FirstOrDefaultAsync(p => p.Id == id.Value) ?? throw AppException.NoEncontrado("Proveedor no encontrado.");
        }
        else
        {
            proveedor = new Proveedor();
            _db.Proveedores.Add(proveedor);
        }

        proveedor.NombreEmpresa = dto.NombreEmpresa.Trim();
        proveedor.ContactoPersona = dto.ContactoPersona?.Trim() ?? string.Empty;
        proveedor.Telefono = dto.Telefono?.Trim() ?? string.Empty;
        proveedor.Email = string.IsNullOrWhiteSpace(dto.Email) ? string.Empty : Seguridad.NormalizarEmail(dto.Email);
        proveedor.Activo = dto.Activo;
        await _db.SaveChangesAsync();
        return new ProveedorDto(proveedor.Id, proveedor.NombreEmpresa, proveedor.ContactoPersona, proveedor.Telefono, proveedor.Email,
            proveedor.Activo, await _db.Productos.CountAsync(p => p.ProveedorId == proveedor.Id));
    }

    public static string Slug(string texto)
    {
        var normalizado = texto.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalizado)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) sb.Append(c);
        }
        return Regex.Replace(sb.ToString(), "[^a-z0-9]+", "-").Trim('-');
    }
}
