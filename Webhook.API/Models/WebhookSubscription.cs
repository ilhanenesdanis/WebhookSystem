namespace Webhook.API.Models;

public sealed record WebhookSubscription(Guid Id, string EventType, string WebhokUrl, DateTime CreateOnUtc);

public sealed record CreateWebhookRequest(string EventType, string WebhokUrl);


