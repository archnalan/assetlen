using Newtonsoft.Json;

namespace mowt.API.Middlewares;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;


    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // Only handle exceptions that have not been caught
            if (!context.Response.HasStarted)
            {
                // Log the exception
                _logger.LogError(ex, "An unhandled exception occurred.");

                // Handle uncaught exceptions only, leaving the original response intact for handled errors
                await HandleUncaughtExceptionAsync(context, ex);
            }
            else
            {
                // Log any exception for diagnostics even if the response has already started
                _logger.LogError(ex, "An exception occurred after the response started.");
                throw; // Re-throw to ensure proper handling
            }
        }
    }

    private Task HandleUncaughtExceptionAsync(HttpContext context, Exception exception)
    {
        // Custom logic to handle uncaught exceptions, without modifying the existing 500 responses.
        var statusCode = StatusCodes.Status500InternalServerError; // Default to 500 for uncaught exceptions
        var message = "An unexpected error occurred. Please try again later.";

        // Log the uncaught exception
        _logger.LogError(exception, "Handling uncaught exception with status code {StatusCode}", statusCode);

        // Set the response properties if the response has not already started
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        return context.Response.WriteAsync(JsonConvert.SerializeObject(new
        {
            StatusCode = statusCode,
            Message = message
        }));
    }
}
