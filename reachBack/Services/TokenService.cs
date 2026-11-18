using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using reachBack.Infrastructure;
using reachBack.Models;

namespace reachBack.Services;

public class TokenService
{
    private readonly JwtOptions _opciones;

    public TokenService(IOptions<JwtOptions> opciones)
    {
        _opciones = opciones.Value;
    }

    public (string Token, DateTime Expira) Crear(Usuario usuario)
    {
        var expira = DateTime.UtcNow.AddHours(_opciones.ExpiracionHoras);
        var claims = new List<Claim>
        {
            new("sub", usuario.Id.ToString()),
            new("email", usuario.Email),
            new("name", usuario.Nombre),
            new("role", usuario.Rol),
            new("jti", Guid.NewGuid().ToString("N"))
        };

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = _opciones.Issuer,
            Audience = _opciones.Audience,
            IssuedAt = DateTime.UtcNow,
            NotBefore = DateTime.UtcNow,
            Expires = expira,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opciones.Key)),
                SecurityAlgorithms.HmacSha256)
        };

        var token = new JsonWebTokenHandler().CreateToken(descriptor);
        return (token, expira);
    }
}
