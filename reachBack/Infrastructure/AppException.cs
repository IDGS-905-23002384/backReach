namespace reachBack.Infrastructure;

public class AppException : Exception
{
    public int StatusCode { get; }

    public AppException(string message, int statusCode = StatusCodes.Status400BadRequest) : base(message)
    {
        StatusCode = statusCode;
    }

    public static AppException NoEncontrado(string message) => new(message, StatusCodes.Status404NotFound);
    public static AppException Conflicto(string message) => new(message, StatusCodes.Status409Conflict);
    public static AppException Prohibido(string message) => new(message, StatusCodes.Status403Forbidden);
}
