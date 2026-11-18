using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using reachBack.Dtos;
using reachBack.Infrastructure;
using reachBack.Services;

namespace reachBack.Controllers;

[ApiController]
[Route("api/carrito")]
[Authorize]
public class CarritoController : ControllerBase
{
    private readonly CarritoService _carrito;

    public CarritoController(CarritoService carrito)
    {
        _carrito = carrito;
    }

    [HttpGet]
    public async Task<ActionResult<CarritoDto>> Obtener() => Ok(await _carrito.ObtenerAsync(User.UsuarioId()));

    [HttpPost("items")]
    public async Task<ActionResult<CarritoDto>> Agregar(AgregarCarritoDto dto) => Ok(await _carrito.AgregarAsync(User.UsuarioId(), dto));

    [HttpPut("items/{id:int}")]
    public async Task<ActionResult<CarritoDto>> Actualizar(int id, ActualizarCantidadDto dto) =>
        Ok(await _carrito.ActualizarCantidadAsync(User.UsuarioId(), id, dto));

    [HttpDelete("items/{id:int}")]
    public async Task<ActionResult<CarritoDto>> Eliminar(int id) => Ok(await _carrito.EliminarAsync(User.UsuarioId(), id));

    [HttpDelete]
    public async Task<IActionResult> Vaciar()
    {
        await _carrito.VaciarAsync(User.UsuarioId());
        return NoContent();
    }

    [HttpPost("ensamble")]
    public async Task<ActionResult<CarritoDto>> AgregarEnsamble(AgregarEnsambleDto dto) => Ok(await _carrito.AgregarEnsambleAsync(User.UsuarioId(), dto));

    [HttpPost("prearmado/{id:int}")]
    public async Task<ActionResult<CarritoDto>> AgregarPrearmado(int id) => Ok(await _carrito.AgregarPrearmadoAsync(User.UsuarioId(), id));

    [HttpDelete("ensamble/{ensambleId}")]
    public async Task<ActionResult<CarritoDto>> EliminarEnsamble(string ensambleId)
    {
        if (ensambleId.Length > 40) return BadRequest();
        return Ok(await _carrito.EliminarEnsambleAsync(User.UsuarioId(), ensambleId));
    }
}
