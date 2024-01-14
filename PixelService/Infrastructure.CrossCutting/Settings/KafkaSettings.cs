namespace PixelService.Infrastructure.CrossCutting.Settings
{
    public class KafkaSettings
    {
        public IEnumerable<string> BootstrapServers { get; set; }

        public Dictionary<string, ProducerSettings> Producers { get; set; }
    }
}
