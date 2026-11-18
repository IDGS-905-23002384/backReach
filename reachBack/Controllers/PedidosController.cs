using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using reachBack.Dtos;
using reachBack.Infrastructure;
using reachBack.Services;

namespace reachBack.Controllers;

[ApiController]
[Route("api/pedidos")]
[Authorize]
public class PedidosController : ControllerBase
{
    private readonly PedidoService _pedidos;

    public PedidosController(PedidoService pedidos)
    {
        _pedidos = pedidos;
    }

    [HttpPost]
    [EnableRateLimiting("checkout")]
    public async Task<ActionResult<PedidoDetalleDto>> Crear(CheckoutDto dto)
    {
        var pedido = await _pedidos.CrearAsync(User.UsuarioId(), dto);
        return CreatedAtAction(nameof(Obtener), new { id = pedido.Id }, pedido);
    }

    [HttpGet]
    public async Task<ActionResult<List<PedidoResumenDto>>> MisPedidos() => Ok(await _pedidos.MisPedidosAsync(User.UsuarioId()));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PedidoDetalleDto>> Obtener(int id) => Ok(await _pedidos.DetalleAsync(id, User.UsuarioId(), false));

    [HttpPost("{id:int}/cancelar")]
    public async Task<ActionResult<PedidoDetalleDto>> Cancelar(int id) =>
        Ok(await _pedidos.CancelarAsync(id, User.UsuarioId(), false, User.Email()));
}
