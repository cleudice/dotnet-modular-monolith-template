using System.Text.Json;
using BuildingBlocks.Application;
using BuildingBlocks.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Infrastructure;

/// <summary>
/// Background service that processes outbox messages for a specific module's DbContext.
/// Each module registers its own instance: <c>AddHostedService&lt;OutboxBackgroundService&lt;CatalogDbContext&gt;&gt;()</c>.
/// Shared logic lives here; per-module isolation via the generic TContext.
/// </summary>
public sealed class OutboxBackgroundService<TContext>(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxBackgroundService<TContext>> logger) : BackgroundService
    where TContext : AppDbContext
{
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(5);
    private const int BatchSize = 10;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Outbox processor started for {Context}", typeof(TContext).Name);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(PollingInterval, stoppingToken);
                await ProcessBatchAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled error in outbox processor for {Context}", typeof(TContext).Name);
            }
        }
    }

    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TContext>();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IDomainEventDispatcher>();

        var messages = await context
            .Set<OutboxMessage>()
            .Where(m => m.ProcessedOn == null)
            .OrderBy(m => m.OccurredOn)
            .Take(BatchSize)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        if (messages.Count == 0)
        {
            return;
        }

        foreach (var message in messages)
        {
            try
            {
                var eventType = Type.GetType(message.Type);
                if (eventType is null)
                {
                    message.MarkFailed($"Event type not found: {message.Type}");
                    continue;
                }

                var domainEvent = JsonSerializer.Deserialize(message.Payload, eventType) as IDomainEvent;
                if (domainEvent is null)
                {
                    message.MarkFailed($"Failed to deserialize event: {message.Type}");
                    continue;
                }

                await dispatcher.DispatchAsync([domainEvent], ct).ConfigureAwait(false);
                message.MarkProcessed();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process outbox message {MessageId}", message.Id);
                message.MarkFailed(ex.Message);
            }
        }

        await context.SaveChangesAsync(ct).ConfigureAwait(false);
    }
}
