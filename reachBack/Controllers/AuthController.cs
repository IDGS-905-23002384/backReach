using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using reachBack.Dtos;
using reachBack.Infrastructure;
using reachBack.Services;

namespace reachBack.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _auth;

    public AuthController(AuthService auth)
    {
        _auth = auth;
    }

    [HttpPost("registro")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<AuthRespuestaDto>> Registro(RegistroDto dto) => Ok(await _auth.RegistrarAsync(dto));

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<AuthRespuestaDto>> Login(LoginDto dto) => Ok(await _auth.LoginAsync(dto));

    [HttpGet("perfil")]
    [Authorize]
    public async Task<ActionResult<UsuarioDto>> Perfil() => Ok(await _auth.PerfilAsync(User.UsuarioId()));

    [HttpPut("perfil")]
    [Authorize]
    public async Task<ActionResult<UsuarioDto>> ActualizarPerfil(PerfilActualizarDto dto) => Ok(await _auth.ActualizarPerfilAsync(User.UsuarioId(), dto));

    [HttpPut("password")]
    [Authorize]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> CambiarPassword(CambiarPasswordDto dto)
    {
        await _auth.CambiarPasswordAsync(User.UsuarioId(), dto);
        return NoContent();
    }
}
