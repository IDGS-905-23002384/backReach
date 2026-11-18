using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using reachBack.Dtos;
using reachBack.Services;

namespace reachBack.Controllers;

[ApiController]
[Route("api/configurador")]
[AllowAnonymous]
public class ConfiguradorController : ControllerBase
{
    private readonly ConfiguradorService _configurador;

    public ConfiguradorController(ConfiguradorService configurador)
    {
        _configurador = configurador;
    }

    [HttpGet("pasos")]
    public async Task<ActionResult<List<PasoConfiguradorDto>>> Pasos() => Ok(await _configurador.PasosAsync());

    [HttpPost("opciones")]
    public async Task<ActionResult<List<OpcionComponenteDto>>> Opciones(OpcionesRequestDto request) => Ok(await _configurador.OpcionesAsync(request));

    [HttpPost("validar")]
    public async Task<ActionResult<ResumenConfiguracionDto>> Validar(SeleccionPcDto seleccion) => Ok(await _configurador.ValidarAsync(seleccion));

    [HttpGet("prearmados")]
    public async Task<ActionResult<List<PrearmadoDto>>> Prearmados() => Ok(await _configurador.PrearmadosAsync());
}
