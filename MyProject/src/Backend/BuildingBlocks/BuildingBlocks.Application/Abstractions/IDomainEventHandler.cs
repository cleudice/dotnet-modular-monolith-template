using BuildingBlocks.Domain;

namespace BuildingBlocks.Application;

/// <summary>
/// Handles a specific domain event type. Implement this interface
/// to react to domain events in-process (no MediatR dependency).
/// </summary>
public interface IDomainEventHandler<in T> where T : IDomainEvent
{
    Task HandleAsync(T domainEvent, CancellationToken cancellationToken = default);
}
