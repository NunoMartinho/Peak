namespace StorageService.Infrastructure.CrossCutting.Settings
{
    using Confluent.Kafka;

    public class ConsumerSettings
    {
        public AutoOffsetReset AutoOffsetReset { get; set; }

        public string DefaultTopic { get; set; }

        public string GroupId { get; set; }
    }
}