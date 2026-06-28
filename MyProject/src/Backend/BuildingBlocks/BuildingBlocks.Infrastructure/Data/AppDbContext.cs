using BuildingBlocks.Application;
using BuildingBlocks.Domain;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Infrastructure;

public abstract class AppDbContext(DbContextOptions options) : DbContext(options), IUnitOfWork
{
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        CaptureOutboxMessages();
        return await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public override int SaveChanges()
    {
        CaptureOutboxMessages();
        return base.SaveChanges();
    }

    /// <summary>
    /// Collects domain events from tracked aggregate roots, serializes them
    /// into <see cref="OutboxMessage"/> entries, and clears the events.
    /// The outbox messages are persisted in the same transaction as the entity changes.
    /// </summary>
    private void CaptureOutboxMessages()
    {
        var aggregates = ChangeTracker
            .Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        foreach (var aggregate in aggregates)
        {
            foreach (var domainEvent in aggregate.DomainEvents)
            {
                Set<OutboxMessage>().Add(OutboxMessage.FromDomainEvent(domainEvent));
            }

            aggregate.ClearDomainEvents();
        }
    }
}
