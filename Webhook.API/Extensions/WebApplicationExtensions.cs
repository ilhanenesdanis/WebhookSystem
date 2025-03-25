using Microsoft.EntityFrameworkCore;
using Webhook.API.Data;

namespace Webhook.API.Extensions;

public static class WebApplicationExtensions
{
    public static async Task ApplyMigrationAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WebhookDbContext>();
        await context.Database.MigrateAsync();
    }
}
