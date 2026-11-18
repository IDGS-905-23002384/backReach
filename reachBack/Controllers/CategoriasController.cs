using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using reachBack.Dtos;
using reachBack.Models;
using reachBack.Services;

namespace reachBack.Controllers;

[ApiController]
[Route("api/categorias")]
public class CategoriasController : ControllerBase
{
    private readonly CatalogoAdminService _catalogo;

    public CategoriasController(CatalogoAdminService catalogo)
    {
        _catalogo = catalogo;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<CategoriaDto>>> Listar() => Ok(await _catalogo.CategoriasAsync());

    [HttpPost]
    [Authorize(Roles = Roles.Personal)]
    public async Task<ActionResult<CategoriaDto>> Crear(CategoriaGuardarDto dto) => Ok(await _catalogo.GuardarCategoriaAsync(null, dto));

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Personal)]
    public async Task<ActionResult<CategoriaDto>> Actualizar(int id, CategoriaGuardarDto dto) => Ok(await _catalogo.GuardarCategoriaAsync(id, dto));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _catalogo.EliminarCategoriaAsync(id);
        return NoContent();
    }
}
