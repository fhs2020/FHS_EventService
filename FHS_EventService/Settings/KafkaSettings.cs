namespace FHS_EventService.Settings
{

    public class KafkaSettings
    {
        public string BootstrapServers { get; set; } = "localhost:9092";
        public string Topic { get; set; } = "user-events";
        public string GroupId { get; set; } = "user-events-consumer";
    }
}

