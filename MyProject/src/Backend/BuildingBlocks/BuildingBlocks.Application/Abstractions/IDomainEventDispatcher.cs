using BuildingBlocks.Domain;

namespace BuildingBlocks.Application;

/// <summary>
/// Dispatches domain events to registered handlers in-process.
/// </summary>
public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
