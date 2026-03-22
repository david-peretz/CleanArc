using CleanArc.Domain.Common;

namespace CleanArc.WebApi.Middlewares;

public sealed class ExceptionMappingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMappingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainRuleException ex)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch (Exception)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new { error = "Unexpected error." });
        }
    }
}
