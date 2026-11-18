using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using reachBack.Dtos;
using reachBack.Infrastructure;
using reachBack.Models;
using reachBack.Services;

namespace reachBack.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = Roles.Personal)]
public class AdminController : ControllerBase
{
    private readonly PedidoService _pedidos;
    private readonly InventarioService _inventario;
    private readonly CatalogoAdminService _catalogo;

    public AdminController(PedidoService pedidos, InventarioService inventario, CatalogoAdminService catalogo)
    {
        _pedidos = pedidos;
        _inventario = inventario;
        _catalogo = catalogo;
    }

    [HttpGet("panel")]
    public async Task<ActionResult<ResumenPanelDto>> Panel() => Ok(await _pedidos.PanelAsync());

    [HttpGet("pedidos")]
    public async Task<ActionResult<PaginaDto<PedidoResumenDto>>> Pedidos([FromQuery] string? estado, [FromQuery] string? q,
        [FromQuery] int pagina = 1, [FromQuery] int tamano = 30) =>
        Ok(await _pedidos.ListarAdminAsync(estado, q?.Length > 80 ? q[..80] : q, pagina, tamano));

    [HttpGet("pedidos/{id:int}")]
    public async Task<ActionResult<PedidoDetalleDto>> Pedido(int id) => Ok(await _pedidos.DetalleAsync(id, 0, true));

    [HttpPut("pedidos/{id:int}/armado")]
    public async Task<ActionResult<PedidoDetalleDto>> Armado(int id, ActualizarArmadoDto dto) => Ok(await _pedidos.ActualizarArmadoAsync(id, dto));

    [HttpPut("pedidos/{id:int}/envio")]
    public async Task<ActionResult<PedidoDetalleDto>> Envio(int id, ActualizarEnvioDto dto) => Ok(await _pedidos.ActualizarEnvioAsync(id, dto));

    [HttpPost("pedidos/{id:int}/cancelar")]
    public async Task<ActionResult<PedidoDetalleDto>> Cancelar(int id) => Ok(await _pedidos.CancelarAsync(id, 0, true, User.Email()));

    [HttpGet("inventario/alertas")]
    public async Task<ActionResult<List<AlertaStockDto>>> Alertas() => Ok(await _inventario.AlertasAsync());

    [HttpGet("inventario/kardex")]
    public async Task<ActionResult<PaginaDto<MovimientoDto>>> Kardex([FromQuery] int? productoId, [FromQuery] int pagina = 1, [FromQuery] int tamano = 30) =>
        Ok(await _inventario.KardexAsync(productoId, pagina, tamano));

    [HttpPost("inventario/movimientos")]
    public async Task<ActionResult<MovimientoDto>> Movimiento(MovimientoCrearDto dto) => Ok(await _inventario.CrearMovimientoAsync(dto, User.Email()));

    [HttpGet("proveedores")]
    public async Task<ActionResult<List<ProveedorDto>>> Proveedores() => Ok(await _catalogo.ProveedoresAsync());

    [HttpPost("proveedores")]
    public async Task<ActionResult<ProveedorDto>> CrearProveedor(ProveedorGuardarDto dto) => Ok(await _catalogo.GuardarProveedorAsync(null, dto));

    [HttpPut("proveedores/{id:int}")]
    public async Task<ActionResult<ProveedorDto>> ActualizarProveedor(int id, ProveedorGuardarDto dto) => Ok(await _catalogo.GuardarProveedorAsync(id, dto));
}
