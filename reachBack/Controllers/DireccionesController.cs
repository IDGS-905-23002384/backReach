using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using reachBack.Dtos;
using reachBack.Infrastructure;
using reachBack.Services;

namespace reachBack.Controllers;

[ApiController]
[Route("api/direcciones")]
[Authorize]
public class DireccionesController : ControllerBase
{
    private readonly DireccionService _direcciones;

    public DireccionesController(DireccionService direcciones)
    {
        _direcciones = direcciones;
    }

    [HttpGet]
    public async Task<ActionResult<List<DireccionDto>>> Listar() => Ok(await _direcciones.ListarAsync(User.UsuarioId()));

    [HttpPost]
    public async Task<ActionResult<DireccionDto>> Crear(DireccionGuardarDto dto) => Ok(await _direcciones.CrearAsync(User.UsuarioId(), dto));

    [HttpPut("{id:int}")]
    public async Task<ActionResult<DireccionDto>> Actualizar(int id, DireccionGuardarDto dto) =>
        Ok(await _direcciones.ActualizarAsync(User.UsuarioId(), id, dto));

    [HttpPut("{id:int}/predeterminada")]
    public async Task<IActionResult> Predeterminar(int id)
    {
        await _direcciones.PredeterminarAsync(User.UsuarioId(), id);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _direcciones.EliminarAsync(User.UsuarioId(), id);
        return NoContent();
    }
}
