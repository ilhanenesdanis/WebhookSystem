using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Webhook.API.Data;
using Webhook.API.Models;

namespace Webhook.API.Services;

public sealed class WebhookDispatcher(IHttpClientFactory httpClientFactory, WebhookDbContext context)
{
    public async Task DispatchAsync<T>(string eventType, T payload)
    {
        var subscriptions = await context.WebhookSubscriptions.AsNoTracking().Where(x => x.EventType == eventType).ToListAsync();

        foreach (WebhookSubscription subscription in subscriptions)
        {
            using var httpclient = httpClientFactory.CreateClient();

            var request = new WebhookPayload<T>()
            {
                Id = Guid.NewGuid(),
                EventType = eventType,
                SubscriptionId = subscription.Id,
                TimeStamp = DateTime.UtcNow,
                Data = payload
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            try
            {
                HttpResponseMessage response = await httpclient.PostAsJsonAsync(subscription.WebhokUrl, request);
                var attempt = new WebhookDeliveryAttempt
                {
                    Id = Guid.NewGuid(),
                    WebhookSubscriptionId = subscription.Id,
                    Payload = jsonPayload,
                    ResponseStatusCode = (int)response.StatusCode,
                    Success = response.IsSuccessStatusCode,
                    Timestamp = DateTime.UtcNow
                };
                await context.WebhookDeliveryAttempts.AddAsync(attempt);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var attempt = new WebhookDeliveryAttempt
                {
                    Id = Guid.NewGuid(),
                    WebhookSubscriptionId = subscription.Id,
                    Payload = jsonPayload,
                    ResponseStatusCode = null,
                    Success = false,
                    Timestamp = DateTime.UtcNow
                };
                await context.WebhookDeliveryAttempts.AddAsync(attempt);
                await context.SaveChangesAsync();
            }

        }
    }
}

