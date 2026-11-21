using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using reachBack.Dtos;
using reachBack.Services;

namespace reachBack.Controllers;

[ApiController]
[Route("api/sucursales")]
[AllowAnonymous]
public class SucursalesController : ControllerBase
{
    private readonly SucursalService _sucursales;

    public SucursalesController(SucursalService sucursales)
    {
        _sucursales = sucursales;
    }

    [HttpGet]
    public async Task<ActionResult<List<SucursalDto>>> Listar() => Ok(await _sucursales.ListarAsync());
}
