using Confluent.Kafka;
using Serilog;
using System.Text.Json;
using Models;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/health");

// Statistics endpoint
var processedCount = 0;
var errorCount = 0;

app.MapGet("/api/stats", () => Results.Ok(new
{
    processed = processedCount,
    errors = errorCount,
    uptime = DateTime.UtcNow
}));

// Start Kafka consumer in background
var consumerTask = Task.Run(() =>
{
    var config = new ConsumerConfig
    {
        BootstrapServers = builder.Configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
        GroupId = "analytics-consumer-group",
        AutoOffsetReset = AutoOffsetReset.Earliest,
        EnableAutoCommit = true
    };

    using var consumer = new ConsumerBuilder<string, string>(config).Build();
    consumer.Subscribe("analytics-events");

    Log.Information("Kafka consumer started, listening to analytics-events topic");

    while (true)
    {
        try
        {
            var consumeResult = consumer.Consume(TimeSpan.FromSeconds(1));

            if (consumeResult != null)
            {
                var analyticsEvent = JsonSerializer.Deserialize<AnalyticsEvent>(consumeResult.Message.Value);

                if (analyticsEvent != null)
                {
                    // Process the event (in real system: save to DB, trigger alerts, etc.)
                    Log.Information("Processed event: {EventId} of type {EventType} from {Source}",
                        analyticsEvent.EventId, analyticsEvent.EventType, analyticsEvent.Source);

                    processedCount++;
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error consuming message");
            errorCount++;
        }
    }
});

app.Run();