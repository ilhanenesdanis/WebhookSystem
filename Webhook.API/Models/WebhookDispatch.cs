namespace Webhook.API.Models;

public sealed record WebhookDispatch(string EventType, object data, string? ParentActivityId);