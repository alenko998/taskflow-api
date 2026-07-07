using MediatR;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.Common.Models;

namespace TaskFlow.Application.Behaviors;

public class ExceptionHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : class
{
    private readonly ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> _logger;

    public ExceptionHandlingBehavior(ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (FluentValidation.ValidationException ex)
        {
            _logger.LogWarning("Validation failed for {RequestName}: {Errors}",
                typeof(TRequest).Name,
                string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for {RequestName}", typeof(TRequest).Name);
            throw;
        }
    }
}