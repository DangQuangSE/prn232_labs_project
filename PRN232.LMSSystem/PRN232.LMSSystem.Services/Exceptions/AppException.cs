namespace PRN232.LMSSystem.Services.Exceptions;

public abstract class AppException : Exception
{
    public int StatusCode { get; }
    protected AppException(string message, int statusCode) : base(message)
        => StatusCode = statusCode;
}

public class NotFoundException : AppException
{
    public NotFoundException(string resourceName, object key)
        : base($"{resourceName} with ID '{key}' was not found.", 404) { }
}

public class BadRequestException : AppException
{
    public BadRequestException(string message) : base(message, 400) { }
}
