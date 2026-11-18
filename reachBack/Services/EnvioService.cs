using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using reachBack.Data;
using reachBack.Dtos;
using reachBack.Infrastructure;

namespace reachBack.Services;

public class EnvioService
{
    private readonly ApplicationDbContext _db;
    private readonly TiendaOptions _tienda;

    public EnvioService(ApplicationDbContext db, IOptions<TiendaOptions> tienda)
    {
        _db = db;
        _tienda = tienda.Value;
    }

    public async Task<List<MetodoEnvioDto>> MetodosAsync(decimal subtotal)
    {
        var metodos = await _db.MetodosEnvio.AsNoTracking().Where(m => m.Activo).OrderBy(m => m.Id).ToListAsync();
        return metodos.Select(m =>
        {
            var gratis = m.Costo == 0 || (m.AplicaEnvioGratis && subtotal >= _tienda.EnvioGratisDesde);
            return new MetodoEnvioDto(m.Id, m.Nombre, m.Descripcion, m.Paqueteria, m.Costo, gratis ? 0m : m.Costo,
                m.DiasMin, m.DiasMax, m.RequiereDireccion, gratis);
        }).ToList();
    }
}
