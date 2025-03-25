using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Channels;
using Webhook.API.Data;
using Webhook.API.Models;
using Webhook.API.OpenTelemtry;

namespace Webhook.API.Services;

public sealed class WebhookDispatcher(IHttpClientFactory httpClientFactory, WebhookDbContext context, Channel<WebhookDispatch> webhooksChannel)
{
    public async Task DispatchAsync<T>(string eventType, T data) where T : notnull
    {
        using Activity activity = DiagnosticConfig.Source.StartActivity($"{eventType} dispatch webhook");
        activity?.AddTag("event-type", eventType);

        await webhooksChannel.Writer.WriteAsync(new WebhookDispatch(eventType, data, activity?.Id));
    }

    public async Task ProccessAsync<T>(string eventType, T payload)
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

