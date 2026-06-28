using System.Text.Json;
using BuildingBlocks.Domain;

namespace BuildingBlocks.Infrastructure;

/// <summary>
/// Outbox message persisted in the same transaction as the aggregate changes.
/// Processed asynchronously by <see cref="OutboxBackgroundService{TContext}"/>.
/// Each module has its own OutboxMessages table via its DbContext.
/// </summary>
public sealed class OutboxMessage
{
    public Guid Id { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public DateTime OccurredOn { get; private set; }
    public DateTime? ProcessedOn { get; private set; }
    public string? Error { get; private set; }

    public void MarkProcessed()
    {
        ProcessedOn = DateTime.UtcNow;
    }

    public void MarkFailed(string error)
    {
        Error = error;
        ProcessedOn = DateTime.UtcNow;
    }

    public static OutboxMessage FromDomainEvent(IDomainEvent domainEvent)
    {
        var eventType = domainEvent.GetType();

        return new OutboxMessage
        {
            Id = Guid.CreateVersion7(),
            Type = eventType.FullName!,
            Payload = JsonSerializer.Serialize(domainEvent, eventType),
            OccurredOn = domainEvent.OccurredOn
        };
    }
}
