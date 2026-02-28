using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace OrderProcessor;

/// <summary>
/// Processes OrderCreated events from Azure Service Bus.
/// Triggered automatically when a new message lands on the 'order-created' topic.
/// </summary>
public class OrderProcessorFunction(ILogger<OrderProcessorFunction> logger)
{
    [Function(nameof(OrderProcessorFunction))]
    public async Task Run(
        [ServiceBusTrigger(
            topicName: "order-created",
            subscriptionName: "notification-sub",
            Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message)
    {
        logger.LogInformation(
            "Processing OrderCreated message {MessageId} at {Timestamp}",
            message.MessageId,
            DateTimeOffset.UtcNow);

        // Deserialize the event payload
        var orderEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(
            message.Body.ToString());

        if (orderEvent is null)
        {
            logger.LogError("Failed to deserialize message {MessageId}", message.MessageId);
            return; // Dead-letter handled by Service Bus after max delivery count
        }

        logger.LogInformation(
            "Order {OrderId} created for customer {CustomerId} — Total: {Total:C}",
            orderEvent.OrderId,
            orderEvent.CustomerId,
            orderEvent.TotalAmount);

        // Business logic: send notification, update inventory, etc.
        await ProcessOrderAsync(orderEvent);
    }

    private async Task ProcessOrderAsync(OrderCreatedEvent orderEvent)
    {
        // Simulate async processing (replace with real notification/inventory logic)
        await Task.Delay(50);
        logger.LogInformation("Order {OrderId} processed successfully", orderEvent.OrderId);
    }
}

/// <summary>
/// Handles OrderStatusChanged events — updates audit log, triggers downstream workflows.
/// </summary>
public class OrderStatusProcessor(ILogger<OrderStatusProcessor> logger)
{
    [Function(nameof(OrderStatusProcessor))]
    public async Task Run(
        [ServiceBusTrigger(
            topicName: "order-status-changed",
            subscriptionName: "audit-sub",
            Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message)
    {
        var statusEvent = JsonSerializer.Deserialize<OrderStatusChangedEvent>(
            message.Body.ToString());

        if (statusEvent is null) return;

        logger.LogInformation(
            "Order {OrderId} status changed: {OldStatus} → {NewStatus}",
            statusEvent.OrderId,
            statusEvent.PreviousStatus,
            statusEvent.NewStatus);

        // Write to audit log (could use Table Storage output binding)
        await WriteAuditLogAsync(statusEvent);
    }

    private Task WriteAuditLogAsync(OrderStatusChangedEvent e)
    {
        // Extend: write to Azure Table Storage or SQL for audit trail
        logger.LogInformation("Audit log written for order {OrderId}", e.OrderId);
        return Task.CompletedTask;
    }
}

// ── Event contracts ────────────────────────────────────────────────────────

public record OrderCreatedEvent(
    Guid OrderId,
    string CustomerId,
    decimal TotalAmount,
    DateTime CreatedAt);

public record OrderStatusChangedEvent(
    Guid OrderId,
    string PreviousStatus,
    string NewStatus,
    DateTime ChangedAt);
