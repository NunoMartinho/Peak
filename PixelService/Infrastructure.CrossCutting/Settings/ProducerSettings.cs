namespace PixelService.Infrastructure.CrossCutting.Settings
{
    using Confluent.Kafka;

    public class ProducerSettings
    {
        public Acks Acks { get; set; }

        public string DefaultTopic { get; set; }

        public int MessageSendMaxRetries { get; set; }
    }
}