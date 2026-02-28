using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace OrderProcessor;

/// <summary>
/// Handles events from Azure Event Grid.
/// Can be triggered by blob events, custom domain events, or Azure resource events.
/// </summary>
public class EventGridHandler(ILogger<EventGridHandler> logger)
{
    [Function(nameof(EventGridHandler))]
    public async Task Run(
        [EventGridTrigger] EventGridEvent gridEvent)
    {
        logger.LogInformation(
            "EventGrid event received — Type: {EventType} | Subject: {Subject} | Id: {Id}",
            gridEvent.EventType,
            gridEvent.Subject,
            gridEvent.Id);

        // Route to appropriate handler based on event type
        await (gridEvent.EventType switch
        {
            "Microsoft.Storage.BlobCreated"       => HandleBlobCreatedAsync(gridEvent),
            "Microsoft.Storage.BlobDeleted"       => HandleBlobDeletedAsync(gridEvent),
            "OrderService.Order.Completed"         => HandleOrderCompletedAsync(gridEvent),
            _                                      => HandleUnknownEventAsync(gridEvent)
        });
    }

    private Task HandleBlobCreatedAsync(EventGridEvent e)
    {
        var data = JsonSerializer.Deserialize<BlobEventData>(e.Data.ToString() ?? "{}");
        logger.LogInformation("Blob created at URL: {Url}", data?.Url);
        // Trigger downstream: image processing, virus scan, etc.
        return Task.CompletedTask;
    }

    private Task HandleBlobDeletedAsync(EventGridEvent e)
    {
        logger.LogInformation("Blob deleted — Subject: {Subject}", e.Subject);
        return Task.CompletedTask;
    }

    private Task HandleOrderCompletedAsync(EventGridEvent e)
    {
        logger.LogInformation("Order completed event — triggering fulfilment workflow");
        // E.g. kick off Logic App, send to partner API, etc.
        return Task.CompletedTask;
    }

    private Task HandleUnknownEventAsync(EventGridEvent e)
    {
        logger.LogWarning("Unhandled event type: {EventType}", e.EventType);
        return Task.CompletedTask;
    }
}

// ── Minimal Event Grid models ──────────────────────────────────────────────

public class EventGridEvent
{
    public string Id          { get; set; } = string.Empty;
    public string EventType   { get; set; } = string.Empty;
    public string Subject     { get; set; } = string.Empty;
    public string EventTime   { get; set; } = string.Empty;
    public object Data        { get; set; } = new();
}

public record BlobEventData(string Url, string Api, string BlobType, long ContentLength);
