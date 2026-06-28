using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Modules.Catalog.Domain;

namespace Host.Api;

public sealed class DomainExceptionHandler(ILogger<DomainExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            CatalogNotFoundException => (
                StatusCodes.Status404NotFound,
                exception.Message),

            CatalogForbiddenException => (
                StatusCodes.Status403Forbidden,
                "Access denied. You do not own this resource."),

            CatalogValidationException => (
                StatusCodes.Status400BadRequest,
                exception.Message),

            _ => default
        };

        if (status == 0)
        {
            return false;
        }

        logger.LogWarning(exception, "Domain exception handled: {Title}", title);

        httpContext.Response.StatusCode = status;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = status,
                Title = title,
                Type = $"https://httpstatuses.com/{status}"
            }, cancellationToken);

        return true;
    }
}
