using Webhook.API.Models;
using Webhook.API.Repositories;
using Webhook.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSingleton<InMemoryOrderRepository>();
builder.Services.AddSingleton<InMemorySubscriptionRepository>();

builder.Services.AddHttpClient<WebhookDispatcher>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("webhooks/subsriptions", (CreateWebhookRequest request, InMemorySubscriptionRepository subscriptionRepository) =>
{
    WebhookSubscription subscription = new WebhookSubscription(Guid.NewGuid(), request.EventType, request.WebhokUrl, DateTime.UtcNow);

    subscriptionRepository.Add(subscription);

    return Results.Ok(subscription);

});

app.MapPost("/orders", async (CreateOrderRequest request, InMemoryOrderRepository orderRepository, WebhookDispatcher webhookDispatcher) =>
{
    var order = new Order(Guid.NewGuid(), request.CustomerName, request.Amount, DateTime.UtcNow);

    orderRepository.Add(order);

    await webhookDispatcher.DispatchAsync("order.created", order);

    return Results.Ok(order);
}).WithTags("Order");


app.MapPost("/orders", (InMemoryOrderRepository orderRepository) =>
{
    return Results.Ok(orderRepository.GetAll());
}).WithTags("order");


app.Run();