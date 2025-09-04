using Confluent.Kafka;

using FHS_EventService.Models;
using FHS_EventService.Services;
using FHS_EventService.Settings;
using FHS_EventService.Storage;

using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

using System.Reflection;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);


builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection("Kafka"));

builder.Services.AddSingleton<IEventStore>(sp =>
{
    int maxEvents = 1000; 
    return new InMemoryEventStore(maxEvents);
});

// Kafka producer
builder.Services.AddSingleton<IProducer<Null, string>>(sp =>
{
    var opt = sp.GetRequiredService<IOptions<KafkaSettings>>().Value;
    var cfg = new ProducerConfig
    {
        BootstrapServers = opt.BootstrapServers
    };

    return new ProducerBuilder<Null, string>(cfg).Build();
});

// Kafka consumer
builder.Services.AddHostedService<KafkaConsumerService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
o.SwaggerDoc("v1", new OpenApiInfo
{
    Title = "User Events Service with Kafka",
    Version = "v1",
    Description = @"A minimal event driven backend built with .NET8 and Kafka
                  - POST events publishes a user event to a Kafka topic
                  - GET events returns the last processed events from memory",

    Contact = new OpenApiContact { Name = "Flavio Sousa" }
});

    var xml = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var path = Path.Combine(AppContext.BaseDirectory, xml);
    if (File.Exists(path)) o.IncludeXmlComments(path, includeControllerXmlComments: true);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/events", async (UserEventRequest req,
                              IProducer<Null, string> producer,
                              IOptions<KafkaSettings> kopt) =>
{
    if (string.IsNullOrWhiteSpace(req.UserId) || string.IsNullOrWhiteSpace(req.Type))
        return Results.BadRequest(new { error = "userId and type are required" });

    var ev = new UserEvent(req.UserId!, req.Type!, req.Timestamp ?? DateTime.UtcNow, req.Data);
    var topic = kopt.Value.Topic;

    await producer.ProduceAsync(topic, new Message<Null, string>
    {
        Value = JsonSerializer.Serialize(ev)
    });

    return Results.Accepted();
});

app.MapGet("/events", (int? limit, IEventStore store) =>
{
    var take = Math.Clamp(limit ?? 100, 1, 1000);
    return Results.Ok(store.GetLatest(take));
});

app.Run();