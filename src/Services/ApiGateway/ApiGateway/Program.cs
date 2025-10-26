using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();
builder.Services.AddHttpClient();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/health");

// Proxy to EventProducer
app.MapPost("/api/events", async (HttpContext context, IHttpClientFactory httpClientFactory) =>
{
    var client = httpClientFactory.CreateClient();
    var producerUrl = builder.Configuration["Services:EventProducer"] ?? "http://localhost:5001";

    var request = new HttpRequestMessage(HttpMethod.Post, $"{producerUrl}/api/events");
    request.Content = new StreamContent(context.Request.Body);
    request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

    var response = await client.SendAsync(request);
    var content = await response.Content.ReadAsStringAsync();

    return Results.Content(content, "application/json", statusCode: (int)response.StatusCode);
});

// System status
app.MapGet("/api/status", async (IHttpClientFactory httpClientFactory) =>
{
    var client = httpClientFactory.CreateClient();
    var services = new Dictionary<string, bool>();

    try
    {
        var producerUrl = builder.Configuration["Services:EventProducer"] ?? "http://localhost:5001";
        var producerResponse = await client.GetAsync($"{producerUrl}/health");
        services["event-producer"] = producerResponse.IsSuccessStatusCode;
    }
    catch { services["event-producer"] = false; }

    try
    {
        var consumerUrl = builder.Configuration["Services:EventConsumer"] ?? "http://localhost:5002";
        var consumerResponse = await client.GetAsync($"{consumerUrl}/health");
        services["event-consumer"] = consumerResponse.IsSuccessStatusCode;
    }
    catch { services["event-consumer"] = false; }

    return Results.Ok(new { services, timestamp = DateTime.UtcNow });
});

Log.Information("API Gateway started");

app.Run();