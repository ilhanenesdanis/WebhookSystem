using Webhook.API.Models;

namespace Webhook.API.Repositories;

public sealed class InMemoryOrderRepository
{
    private readonly List<Order> _orders = [];

    public void Add(Order order) => _orders.Add(order);

    public IReadOnlyList<Order> GetAll() => _orders.AsReadOnly();
}

public sealed class InMemorySubscriptionRepository
{
    private readonly List<WebhookSubscription> _subscriptions = [];
    public void Add(WebhookSubscription subscription) => _subscriptions.Add(subscription);
    public IReadOnlyList<WebhookSubscription> GetByEventType(string eventType) => _subscriptions.Where(x => x.EventType == eventType).ToList().AsReadOnly();
}
