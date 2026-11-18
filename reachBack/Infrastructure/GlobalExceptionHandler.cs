using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace reachBack.Infrastructure;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IProblemDetailsService _problemDetails;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IProblemDetailsService problemDetails)
    {
        _logger = logger;
        _problemDetails = problemDetails;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        int status;
        string detalle;

        switch (exception)
        {
            case AppException app:
                status = app.StatusCode;
                detalle = app.Message;
                break;
            case DbUpdateConcurrencyException:
                status = StatusCodes.Status409Conflict;
                detalle = "La información cambió mientras se procesaba tu solicitud. Intenta de nuevo.";
                break;
            default:
                _logger.LogError(exception, "Error no controlado en {Path}", httpContext.Request.Path);
                status = StatusCodes.Status500InternalServerError;
                detalle = "Ocurrió un error inesperado. Intenta más tarde.";
                break;
        }

        httpContext.Response.StatusCode = status;
        return await _problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = detalle,
                Detail = detalle
            },
            Exception = exception
        });
    }
}
