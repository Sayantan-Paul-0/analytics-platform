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

builder.Services.AddSingleton<IProducer<string, string>>(sp =>
{
    var config = new ProducerConfig
    {
        BootstrapServers = builder.Configuration["Kafka:BootstrapServers"]?? "localhost:9092",
        ClientId = "event-producer"
    };
    return new ProducerBuilder<string, string>(config).Build();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/health");

//Endpoint to produce events
app.MapPost("/api/events",async (AnalyticsEvent analyticsEvent,IProducer<string,string> producer) =>
{
    try
    {
        var message = new Message<string,string>
        {
            Key=analyticsEvent.EventId,
            Value=JsonSerializer.Serialize(analyticsEvent)
        };

        var result = await producer.ProduceAsync("analytics-events",message);
        Log.Information("Event produced: {EventId} to partition {Partition}",analyticsEvent.EventId,result.Partition.Value);
        return Results.Ok(new { success = true, eventId = analyticsEvent.EventId, partition = result.Partition.Value });
    }
    catch (Exception ex)
    {
        Log.Error(ex,"Failed to produce event");
        return Results.Problem(ex.Message);
    }
});

// Endpoint to generate random events (for testing)
app.MapPost("/api/events/generate/{count}", async (int count, IProducer<string, string> producer) =>
{
    var random = new Random();
    var sources = new[] { EventSources.IoT, EventSources.Web, EventSources.Mobile };
    var eventTypes = new[] { EventTypes.PageView, EventTypes.ButtonClick, EventTypes.SensorReading };

    var produced = 0;

    for (int i = 0; i < count; i++)
    {
        var analyticsEvent = new AnalyticsEvent
        {
            EventType = eventTypes[random.Next(eventTypes.Length)],
            Source = sources[random.Next(sources.Length)],
            UserId = $"user_{random.Next(1000)}",
            SessionId = Guid.NewGuid().ToString(),
            Payload = new Dictionary<string, object>
            {
                ["value"] = random.Next(100),
                ["temperature"] = random.Next(20, 30),
                ["humidity"] = random.Next(40, 80)
            }
        };

        var message = new Message<string, string>
        {
            Key = analyticsEvent.EventId,
            Value = JsonSerializer.Serialize(analyticsEvent)
        };

        await producer.ProduceAsync("analytics-events", message);
        produced++;
    }

    Log.Information("Generated {Count} random events", produced);
    return Results.Ok(new { success = true, produced });
});

app.Run();