using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using reachBack.Dtos;
using reachBack.Services;

namespace reachBack.Controllers;

[ApiController]
[Route("api/configurador")]
[AllowAnonymous]
public class ConfiguradorController : ControllerBase
{
    private readonly ConfiguradorService _configurador;
    private readonly RecomendadorService _recomendador;

    public ConfiguradorController(ConfiguradorService configurador, RecomendadorService recomendador)
    {
        _configurador = configurador;
        _recomendador = recomendador;
    }

    [HttpGet("pasos")]
    public async Task<ActionResult<List<PasoConfiguradorDto>>> Pasos() => Ok(await _configurador.PasosAsync());

    [HttpPost("opciones")]
    public async Task<ActionResult<List<OpcionComponenteDto>>> Opciones(OpcionesRequestDto request) => Ok(await _configurador.OpcionesAsync(request));

    [HttpPost("validar")]
    public async Task<ActionResult<ResumenConfiguracionDto>> Validar(SeleccionPcDto seleccion) => Ok(await _configurador.ValidarAsync(seleccion));

    [HttpGet("prearmados")]
    public async Task<ActionResult<List<PrearmadoDto>>> Prearmados() => Ok(await _configurador.PrearmadosAsync());

    [HttpGet("usos")]
    public ActionResult<List<UsoEquipoDto>> Usos() => Ok(RecomendadorService.Usos());

    [HttpPost("recomendar")]
    [EnableRateLimiting("recomendador")]
    public async Task<ActionResult<RecomendacionesDto>> Recomendar(RecomendacionRequestDto request) => Ok(await _recomendador.RecomendarAsync(request));
}
