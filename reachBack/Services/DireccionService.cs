using Microsoft.EntityFrameworkCore;
using reachBack.Data;
using reachBack.Dtos;
using reachBack.Infrastructure;
using reachBack.Models;

namespace reachBack.Services;

public class DireccionService
{
    private const int MaxDirecciones = 10;
    private readonly ApplicationDbContext _db;

    public DireccionService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<DireccionDto>> ListarAsync(int usuarioId)
    {
        var lista = await _db.Direcciones.AsNoTracking()
            .Where(d => d.UsuarioId == usuarioId)
            .OrderByDescending(d => d.EsPredeterminada).ThenBy(d => d.Id)
            .ToListAsync();
        return lista.Select(d => d.ADto()).ToList();
    }

    public async Task<DireccionDto> CrearAsync(int usuarioId, DireccionGuardarDto dto)
    {
        var total = await _db.Direcciones.CountAsync(d => d.UsuarioId == usuarioId);
        if (total >= MaxDirecciones) throw new AppException("Puedes guardar hasta 10 direcciones.");

        var direccion = new Direccion { UsuarioId = usuarioId };
        Aplicar(direccion, dto);
        if (total == 0 || dto.EsPredeterminada)
        {
            await QuitarPredeterminadaAsync(usuarioId);
            direccion.EsPredeterminada = true;
        }
        _db.Direcciones.Add(direccion);
        await _db.SaveChangesAsync();
        return direccion.ADto();
    }

    public async Task<DireccionDto> ActualizarAsync(int usuarioId, int id, DireccionGuardarDto dto)
    {
        var direccion = await ObtenerAsync(usuarioId, id);
        Aplicar(direccion, dto);
        if (dto.EsPredeterminada && !direccion.EsPredeterminada)
        {
            await QuitarPredeterminadaAsync(usuarioId);
            direccion.EsPredeterminada = true;
        }
        await _db.SaveChangesAsync();
        return direccion.ADto();
    }

    public async Task PredeterminarAsync(int usuarioId, int id)
    {
        var direccion = await ObtenerAsync(usuarioId, id);
        await QuitarPredeterminadaAsync(usuarioId);
        direccion.EsPredeterminada = true;
        await _db.SaveChangesAsync();
    }

    public async Task EliminarAsync(int usuarioId, int id)
    {
        var direccion = await ObtenerAsync(usuarioId, id);
        _db.Direcciones.Remove(direccion);
        if (direccion.EsPredeterminada)
        {
            var otra = await _db.Direcciones.Where(d => d.UsuarioId == usuarioId && d.Id != id).OrderBy(d => d.Id).FirstOrDefaultAsync();
            if (otra is not null) otra.EsPredeterminada = true;
        }
        await _db.SaveChangesAsync();
    }

    private async Task<Direccion> ObtenerAsync(int usuarioId, int id) =>
        await _db.Direcciones.FirstOrDefaultAsync(d => d.Id == id && d.UsuarioId == usuarioId)
        ?? throw AppException.NoEncontrado("Dirección no encontrada.");

    private async Task QuitarPredeterminadaAsync(int usuarioId)
    {
        var actuales = await _db.Direcciones.Where(d => d.UsuarioId == usuarioId && d.EsPredeterminada).ToListAsync();
        foreach (var d in actuales) d.EsPredeterminada = false;
    }

    private static void Aplicar(Direccion d, DireccionGuardarDto dto)
    {
        d.Alias = string.IsNullOrWhiteSpace(dto.Alias) ? "Casa" : dto.Alias.Trim();
        d.NombreRecibe = dto.NombreRecibe.Trim();
        d.Telefono = dto.Telefono.Trim();
        d.Calle = dto.Calle.Trim();
        d.NumeroExterior = dto.NumeroExterior.Trim();
        d.NumeroInterior = string.IsNullOrWhiteSpace(dto.NumeroInterior) ? null : dto.NumeroInterior.Trim();
        d.Colonia = dto.Colonia.Trim();
        d.Ciudad = dto.Ciudad.Trim();
        d.Estado = dto.Estado.Trim();
        d.CodigoPostal = dto.CodigoPostal.Trim();
        d.Referencias = string.IsNullOrWhiteSpace(dto.Referencias) ? null : dto.Referencias.Trim();
    }
}
