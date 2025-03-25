using System.Diagnostics;
using System.Threading.Channels;
using Webhook.API.Models;
using Webhook.API.OpenTelemtry;

namespace Webhook.API.Services;

public sealed class WebhookProccessor(IServiceScopeFactory scopeFactory, Channel<WebhookDispatch> webhookChannel) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (WebhookDispatch dispatch in webhookChannel.Reader.ReadAllAsync(stoppingToken))
        {
            using Activity activity = DiagnosticConfig.Source.StartActivity($"{dispatch.EventType} process webhook", ActivityKind.Internal, parentId: dispatch.ParentActivityId);

            using IServiceScope scope = scopeFactory.CreateScope();

            var dispatcher = scope.ServiceProvider.GetRequiredService<WebhookDispatcher>();

            await dispatcher.ProccessAsync(dispatch.EventType, dispatch.data);
        }
    }
}