using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using reachBack.Dtos;
using reachBack.Infrastructure;
using reachBack.Services;

namespace reachBack.Controllers;

[ApiController]
[Route("api/notificaciones")]
[Authorize]
public class NotificacionesController : ControllerBase
{
    private readonly NotificacionService _notificaciones;

    public NotificacionesController(NotificacionService notificaciones)
    {
        _notificaciones = notificaciones;
    }

    [HttpGet]
    public async Task<ActionResult<PaginaDto<NotificacionDto>>> Listar([FromQuery] bool soloNoLeidas = false, [FromQuery] int pagina = 1, [FromQuery] int tamano = 30) =>
        Ok(await _notificaciones.ListarAsync(User.UsuarioId(), soloNoLeidas, pagina, tamano));

    [HttpGet("conteo")]
    public async Task<ActionResult<ConteoNotificacionesDto>> Conteo() => Ok(await _notificaciones.ConteoAsync(User.UsuarioId()));

    [HttpGet("nuevas")]
    public async Task<ActionResult<List<NotificacionDto>>> Nuevas([FromQuery] int despuesDe = 0) =>
        Ok(await _notificaciones.NuevasAsync(User.UsuarioId(), Math.Max(0, despuesDe)));

    [HttpPut("{id:int}/leida")]
    public async Task<IActionResult> MarcarLeida(int id)
    {
        await _notificaciones.MarcarLeidaAsync(User.UsuarioId(), id);
        return NoContent();
    }

    [HttpPut("leidas")]
    public async Task<IActionResult> MarcarTodas()
    {
        await _notificaciones.MarcarTodasAsync(User.UsuarioId());
        return NoContent();
    }
}
