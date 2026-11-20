using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using reachBack.Data;
using reachBack.Dtos;
using reachBack.Infrastructure;
using reachBack.Models;

namespace reachBack.Services;

public class AuthService
{
    private const int CostoHash = 11;
    private static readonly string HashFicticio = BCrypt.Net.BCrypt.HashPassword("hash-de-relleno-para-tiempo-constante", CostoHash);

    private readonly ApplicationDbContext _db;
    private readonly TokenService _tokens;
    private readonly TiendaOptions _tienda;
    private readonly ILogger<AuthService> _logger;

    public AuthService(ApplicationDbContext db, TokenService tokens, IOptions<TiendaOptions> tienda, ILogger<AuthService> logger)
    {
        _db = db;
        _tokens = tokens;
        _tienda = tienda.Value;
        _logger = logger;
    }

    public static string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password, CostoHash);

    public async Task<AuthRespuestaDto> RegistrarAsync(RegistroDto dto)
    {
        var email = Seguridad.NormalizarEmail(dto.Email);
        if (await _db.Usuarios.AnyAsync(u => u.Email == email))
        {
            throw AppException.Conflicto("Ya existe una cuenta con ese correo.");
        }

        var usuario = new Usuario
        {
            Nombre = dto.Nombre.Trim(),
            Email = email,
            Telefono = dto.Telefono?.Trim() ?? string.Empty,
            PasswordHash = Hash(dto.Password),
            Rol = Roles.Comprador
        };

        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();
        _db.Notificaciones.Add(new Notificacion
        {
            UsuarioId = usuario.Id,
            Titulo = "¡Bienvenido a Reach!",
            Mensaje = "Arma tu PC con piezas 100% compatibles y recibe aquí cada avance de tus pedidos.",
            Tipo = TiposNotificacion.Sistema
        });
        await _db.SaveChangesAsync();
        _logger.LogInformation("Nuevo usuario registrado {UsuarioId}", usuario.Id);
        return Respuesta(usuario);
    }

    public async Task<AuthRespuestaDto> LoginAsync(LoginDto dto)
    {
        var email = Seguridad.NormalizarEmail(dto.Email);
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

        if (usuario is null)
        {
            BCrypt.Net.BCrypt.Verify(dto.Password, HashFicticio);
            throw new AppException("Correo o contraseña incorrectos.", StatusCodes.Status401Unauthorized);
        }

        if (!usuario.Activo)
        {
            throw new AppException("Tu cuenta está desactivada. Contacta a soporte.", StatusCodes.Status403Forbidden);
        }

        if (usuario.BloqueadoHasta.HasValue && usuario.BloqueadoHasta.Value > DateTime.UtcNow)
        {
            var minutos = (int)Math.Ceiling((usuario.BloqueadoHasta.Value - DateTime.UtcNow).TotalMinutes);
            throw new AppException($"Cuenta bloqueada temporalmente por intentos fallidos. Intenta en {minutos} min.", StatusCodes.Status423Locked);
        }

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash))
        {
            usuario.IntentosFallidos++;
            if (usuario.IntentosFallidos >= _tienda.MaxIntentosLogin)
            {
                usuario.BloqueadoHasta = DateTime.UtcNow.AddMinutes(_tienda.MinutosBloqueo);
                usuario.IntentosFallidos = 0;
                _logger.LogWarning("Cuenta {UsuarioId} bloqueada por intentos fallidos", usuario.Id);
            }
            await _db.SaveChangesAsync();
            throw new AppException("Correo o contraseña incorrectos.", StatusCodes.Status401Unauthorized);
        }

        usuario.IntentosFallidos = 0;
        usuario.BloqueadoHasta = null;
        await _db.SaveChangesAsync();
        return Respuesta(usuario);
    }

    public async Task<UsuarioDto> PerfilAsync(int usuarioId)
    {
        var usuario = await ObtenerAsync(usuarioId);
        return usuario.ADto();
    }

    public async Task<UsuarioDto> ActualizarPerfilAsync(int usuarioId, PerfilActualizarDto dto)
    {
        var usuario = await ObtenerAsync(usuarioId);
        usuario.Nombre = dto.Nombre.Trim();
        usuario.Telefono = dto.Telefono?.Trim() ?? string.Empty;
        await _db.SaveChangesAsync();
        return usuario.ADto();
    }

    public async Task CambiarPasswordAsync(int usuarioId, CambiarPasswordDto dto)
    {
        var usuario = await ObtenerAsync(usuarioId);
        if (!BCrypt.Net.BCrypt.Verify(dto.PasswordActual, usuario.PasswordHash))
        {
            throw new AppException("La contraseña actual no es correcta.");
        }
        usuario.PasswordHash = Hash(dto.PasswordNueva);
        await _db.SaveChangesAsync();
    }

    private async Task<Usuario> ObtenerAsync(int usuarioId)
    {
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == usuarioId && u.Activo);
        return usuario ?? throw new AppException("Sesión inválida.", StatusCodes.Status401Unauthorized);
    }

    private AuthRespuestaDto Respuesta(Usuario usuario)
    {
        var (token, expira) = _tokens.Crear(usuario);
        return new AuthRespuestaDto(token, expira, usuario.ADto());
    }
}
