using Microsoft.EntityFrameworkCore;
using Webhook.API.Data;
using Webhook.API.Extensions;
using Webhook.API.Models;
using Webhook.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddHttpClient();
builder.Services.AddScoped<WebhookDispatcher>();

builder.Services.AddDbContext<WebhookDbContext>(opt =>
{
    opt.UseNpgsql(builder.Configuration.GetConnectionString("webhooks"));
});

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    await app.ApplyMigrationAsync();
}

app.UseHttpsRedirection();

app.MapPost("webhooks/subsriptions", async (CreateWebhookRequest request, WebhookDbContext context) =>
{
    WebhookSubscription subscription = new WebhookSubscription(Guid.NewGuid(), request.EventType, request.WebhokUrl, DateTime.UtcNow);

    await context.WebhookSubscriptions.AddAsync(subscription);

    await context.SaveChangesAsync();

    return Results.Ok(subscription);

});

app.MapPost("/orders", async (CreateOrderRequest request, WebhookDbContext context, WebhookDispatcher webhookDispatcher) =>
{
    var order = new Order(Guid.NewGuid(), request.CustomerName, request.Amount, DateTime.UtcNow);

    await context.Orders.AddAsync(order);

    await context.SaveChangesAsync();

    await webhookDispatcher.DispatchAsync("order.created", order);

    return Results.Ok(order);
}).WithTags("Order");


app.MapPost("/orders", async (WebhookDbContext context) =>
{
    return Results.Ok(await context.Orders.ToListAsync());
}).WithTags("order");


app.Run();