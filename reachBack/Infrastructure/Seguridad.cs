namespace reachBack.Infrastructure;

public static class Seguridad
{
    public static void UseEncabezadosSeguridad(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            var headers = context.Response.Headers;
            headers["X-Content-Type-Options"] = "nosniff";
            headers["X-Frame-Options"] = "DENY";
            headers["Referrer-Policy"] = "no-referrer";
            headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
            headers["Content-Security-Policy"] = "default-src 'none'; img-src 'self'; frame-ancestors 'none'";
            await next();
        });
    }

    public static string NormalizarEmail(string email) => email.Trim().ToLowerInvariant();
}
