using Microsoft.EntityFrameworkCore;
using Webhook.API.Models;

namespace Webhook.API.Data;

public sealed class WebhookDbContext(DbContextOptions<WebhookDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<WebhookSubscription> WebhookSubscriptions { get; set; }
    public DbSet<WebhookDeliveryAttempt> WebhookDeliveryAttempts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(builder =>
        {
            builder.ToTable("orders");
            builder.HasKey(p => p.Id);
        });

        modelBuilder.Entity<WebhookSubscription>(builder =>
        {
            builder.ToTable("subscriptions", "webhooks");
            builder.HasKey(p => p.Id);
        });
        modelBuilder.Entity<WebhookDeliveryAttempt>(builder =>
        {
            builder.ToTable("delivery_attempts", "webhooks");
            builder.HasKey(p => p.Id);

            builder.HasOne<WebhookSubscription>().WithMany().HasForeignKey(p => p.WebhookSubscriptionId);
        });
    }
}

