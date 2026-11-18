using System.Security.Claims;

namespace reachBack.Infrastructure;

public static class ClaimsExtensions
{
    public static int UsuarioId(this ClaimsPrincipal user)
    {
        var valor = user.FindFirst("sub")?.Value;
        if (!int.TryParse(valor, out var id))
        {
            throw new AppException("Sesión inválida.", StatusCodes.Status401Unauthorized);
        }
        return id;
    }

    public static string Email(this ClaimsPrincipal user) => user.FindFirst("email")?.Value ?? "sistema";
}
