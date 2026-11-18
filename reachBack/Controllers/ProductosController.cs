using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using reachBack.Dtos;
using reachBack.Infrastructure;
using reachBack.Models;
using reachBack.Services;

namespace reachBack.Controllers;

[ApiController]
[Route("api/productos")]
public class ProductosController : ControllerBase
{
    private readonly ProductoService _productos;

    public ProductosController(ProductoService productos)
    {
        _productos = productos;
    }

    private bool EsPersonal => User.IsInRole(Roles.Empleado) || User.IsInRole(Roles.Administrador);

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PaginaDto<ProductoListaDto>>> Listar([FromQuery] ProductoFiltroDto filtro) =>
        Ok(await _productos.ListarAsync(filtro, EsPersonal));

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<ProductoDetalleDto>> Obtener(int id) => Ok(await _productos.ObtenerAsync(id, EsPersonal));

    [HttpGet("marcas")]
    [AllowAnonymous]
    public async Task<ActionResult<List<string>>> Marcas([FromQuery] int? categoriaId) => Ok(await _productos.MarcasAsync(categoriaId));

    [HttpPost]
    [Authorize(Roles = Roles.Personal)]
    public async Task<ActionResult<ProductoDetalleDto>> Crear(ProductoGuardarDto dto)
    {
        var creado = await _productos.CrearAsync(dto, User.Email());
        return CreatedAtAction(nameof(Obtener), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Personal)]
    public async Task<ActionResult<ProductoDetalleDto>> Actualizar(int id, ProductoGuardarDto dto) =>
        Ok(await _productos.ActualizarAsync(id, dto, User.Email()));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Personal)]
    public async Task<IActionResult> Desactivar(int id)
    {
        await _productos.CambiarEstadoAsync(id, false);
        return NoContent();
    }

    [HttpPatch("{id:int}/activar")]
    [Authorize(Roles = Roles.Personal)]
    public async Task<IActionResult> Activar(int id)
    {
        await _productos.CambiarEstadoAsync(id, true);
        return NoContent();
    }
}
