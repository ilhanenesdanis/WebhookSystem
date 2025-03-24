using Webhook.API.Models;
using Webhook.API.Repositories;

namespace Webhook.API.Services;

public sealed class WebhookDispatcher(HttpClient httpClient, InMemorySubscriptionRepository subscriptionRepository)
{
    public async Task DispatchAsync(string eventType, object payload)
    {
        var subscriptions = subscriptionRepository.GetByEventType(eventType);

        foreach (WebhookSubscription subscription in subscriptions)
        {
            var request = new
            {
                Id = Guid.NewGuid(),
                subscription.EventType,
                SubscritionId = subscription.Id,
                Timestamp = DateTime.UtcNow,
                Data = payload
            };

            await httpClient.PostAsJsonAsync(subscription.WebhokUrl, request);
        }
    }
}