using System.Collections.Concurrent;
using System.Reflection;
using BuildingBlocks.Application;
using BuildingBlocks.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Infrastructure;

public sealed class DomainEventDispatcher(IServiceProvider serviceProvider, ILogger<DomainEventDispatcher> logger)
    : IDomainEventDispatcher
{
    private static readonly ConcurrentDictionary<Type, MethodInfo> HandlerMethodCache = new();

    public async Task DispatchAsync(
        IEnumerable<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            var eventType = domainEvent.GetType();
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);
            var handlers = serviceProvider.GetServices(handlerType);

            foreach (var handler in handlers)
            {
                try
                {
                    var method = HandlerMethodCache.GetOrAdd(eventType, t =>
                        handlerType.GetMethod("HandleAsync")
                        ?? throw new InvalidOperationException(
                            $"IDomainEventHandler<{t.Name}> does not have a HandleAsync method."));

                    await ((Task)method.Invoke(handler, [domainEvent, cancellationToken])!)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error dispatching domain event {EventType}", eventType.Name);
                }
            }
        }
    }
}
