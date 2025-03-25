using System.Diagnostics;

namespace Webhook.API.OpenTelemtry;

public static class DiagnosticConfig
{
    public static readonly ActivitySource Source = new("webhook-api");
}
