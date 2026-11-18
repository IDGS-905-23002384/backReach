using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using reachBack.Dtos;
using reachBack.Services;

namespace reachBack.Controllers;

[ApiController]
[Route("api/envios")]
[AllowAnonymous]
public class EnviosController : ControllerBase
{
    private readonly EnvioService _envios;

    public EnviosController(EnvioService envios)
    {
        _envios = envios;
    }

    [HttpGet("metodos")]
    public async Task<ActionResult<List<MetodoEnvioDto>>> Metodos([FromQuery] decimal subtotal = 0) =>
        Ok(await _envios.MetodosAsync(Math.Max(0, subtotal)));
}
