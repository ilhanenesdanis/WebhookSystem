var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Webhook_API>("webhook-api");

builder.Build().Run();
