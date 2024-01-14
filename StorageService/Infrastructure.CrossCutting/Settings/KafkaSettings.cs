namespace StorageService.Infrastructure.CrossCutting.Settings
{
    public class KafkaSettings
    {
        public IEnumerable<string> BootstrapServers { get; set; }

        public Dictionary<string, ConsumerSettings> Consumers { get; set; }
    }
}
