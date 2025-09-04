using Confluent.Kafka;

using FHS_EventService.Models;
using FHS_EventService.Settings;
using FHS_EventService.Storage;

using Microsoft.Extensions.Options;

using System.Text.Json;

namespace FHS_EventService.Services
{
    public sealed class KafkaConsumerService : BackgroundService
    {
        private readonly ILogger<KafkaConsumerService> _log;
        private readonly IEventStore _store;
        private readonly KafkaSettings _opt;

        public KafkaConsumerService(ILogger<KafkaConsumerService> log,
                                    IEventStore store,
                                    IOptions<KafkaSettings> options)
        {
            _log = log;
            _store = store;
            _opt = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var cfg = new ConsumerConfig
            {
                BootstrapServers = _opt.BootstrapServers,
                GroupId = _opt.GroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = true
            };
        

            using var consumer = new ConsumerBuilder<Ignore, string>(cfg).Build();
            consumer.Subscribe(_opt.Topic);
            _log.LogInformation("Kafka consumer subscribed to {Topic}", _opt.Topic);

            await Task.Yield();  

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var cr = consumer.Consume(stoppingToken);
                    if (cr?.Message?.Value is null) continue;

                    var ev = JsonSerializer.Deserialize<UserEvent>(cr.Message.Value);
                    if (ev is not null)
                    {
                        _store.Append(ev);
                        _log.LogInformation("Processed event {Type} for {User}", ev.Type, ev.UserId);
                    }
                }
            }
            catch (OperationCanceledException) {}
            catch (ConsumeException ex)
            {
                _log.LogError(ex, "Kafka consume error: {Reason}", ex.Error.Reason);
            }
            finally
            {
                consumer.Close();
            }
        }
    }
}

