namespace StorageService.Messaging
{
    using StorageService.Application.Messaging.Handlers;
    using StorageService.Infrastructure.CrossCutting.Settings;
    using StorageService.Messaging.Tracks;

    using Confluent.Kafka;

    public class TrackEventsConsumer : BackgroundService
    {
        private readonly IConsumer<Ignore, TrackEvent> consumer;
        private readonly ITrackEventHandler trackEventHandler;
        private readonly ConsumerSettings consumerSettings;
        private readonly ILogger logger;

        public TrackEventsConsumer(
            IConsumer<Ignore, TrackEvent> consumer,
            ITrackEventHandler trackEventHandler,
            KafkaSettings kafkaSettings,
            ILogger<TrackEventsConsumer> logger)
        {
            this.consumer = consumer;
            this.trackEventHandler = trackEventHandler;
            this.consumerSettings = kafkaSettings.Consumers[typeof(TrackEvent).Name.ToString()];
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            consumer.Subscribe(consumerSettings.DefaultTopic);

            while (!stoppingToken.IsCancellationRequested)
            {
                ProcessKafkaMessage();
            }

            consumer.Close();
        }

        private void ProcessKafkaMessage()
        {
            try
            {
                var consumeResult = consumer.Consume();

                var message = consumeResult.Message.Value;

                this.trackEventHandler.HandleAsync(consumeResult.Message.Timestamp.UtcDateTime, message);

                this.logger.LogInformation($"Received Kafka message: {message}");
            }
            catch (Exception ex)
            {
                this.logger.LogError($"Error processing Kafka message: {ex.Message}");
            }
        }
    }
}
