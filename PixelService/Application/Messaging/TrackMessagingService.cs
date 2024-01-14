namespace PixelService.Application.Messaging
{
    using PixelService.Domain.Tracks;
    using PixelService.Infrastructure.CrossCutting.Settings;
    using PixelService.Messaging.Events.Tracks;

    using Confluent.Kafka;

    using Microsoft.Extensions.Logging;

    using Messaging = PixelService.Messaging.Events;

    public class TrackMessagingService : ITrackMessagingService
    {
        private readonly IProducer<Null, Messaging.Tracks.TrackEvent> producer;
        private readonly ProducerSettings producerSettings;
        private readonly ILogger logger;

        public TrackMessagingService(
            IProducer<Null, Messaging.Tracks.TrackEvent> producer,
            KafkaSettings kafkaSettings,
            ILogger<TrackMessagingService> logger)
        {
            this.producerSettings = kafkaSettings.Producers[typeof(TrackEvent).Name.ToString()];
            this.producer = producer;
            this.logger = logger;
        }

        public async Task SendTrackAsync(Track track)
        {
            var trackEvent = new Messaging.Tracks.TrackEvent
            {
                IpAddress = track.IpAddress,
                Referrer = track.Referrer,
                UserAgent = track.UserAgent
            };

            var kafkaMessage = new Message<Null, Messaging.Tracks.TrackEvent>
            {
                Timestamp = new Timestamp(DateTime.UtcNow, TimestampType.CreateTime),
                Value = trackEvent
            };

            try
            {
                var report = await this.producer.ProduceAsync(this.producerSettings.DefaultTopic, kafkaMessage);
                
                if(report.Status == PersistenceStatus.NotPersisted)
                {
                    this.logger.LogError("Track message could not be produced");
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError($"Track message could not be produced - {ex.Message}");
            }
        }
    }
}
