using Microsoft.EntityFrameworkCore;
using reachBack.Data;
using reachBack.Dtos;
using reachBack.Models;

namespace reachBack.Services;

public class SucursalService
{
    private readonly ApplicationDbContext _db;

    public SucursalService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<SucursalDto>> ListarAsync()
    {
        var lista = await _db.Sucursales.AsNoTracking().Where(s => s.Activa).OrderBy(s => s.Id).ToListAsync();
        return lista.Select(ADto).ToList();
    }

    public static SucursalDto ADto(Sucursal s) =>
        new(s.Id, s.Nombre, s.Direccion, s.Ciudad, s.Telefono, s.Horario, s.Latitud, s.Longitud);
}
