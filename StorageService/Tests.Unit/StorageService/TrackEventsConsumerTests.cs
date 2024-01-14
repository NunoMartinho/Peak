namespace Tests.Unit.StorageServiceTests
{
    using StorageService.Messaging;
    using StorageService.Application.Messaging.Handlers;
    using StorageService.Infrastructure.CrossCutting.Settings;

    using Microsoft.Extensions.Logging;
    using Moq;
    using Confluent.Kafka;
    using StorageService.Messaging.Tracks;

    public class TrackEventsConsumerTests
    {
        private readonly Mock<IConsumer<Ignore, TrackEvent>> consumerMock;
        private readonly Mock<ITrackEventHandler> trackEventHandlerMock;
        private readonly Mock<ILogger<TrackEventsConsumer>> logger;
        private readonly KafkaSettings kafkaSettings;

        private readonly TrackEventsConsumer trackEventsConsumer;

        public TrackEventsConsumerTests()
        {
            this.consumerMock = new Mock<IConsumer<Ignore, TrackEvent>>();
            this.trackEventHandlerMock = new Mock<ITrackEventHandler>();
            this.kafkaSettings = new KafkaSettings
            {
                Consumers = new Dictionary<string, ConsumerSettings>
                {
                    { typeof(TrackEvent).Name,
                      new ConsumerSettings
                      {
                        DefaultTopic = "test",
                      }
                    }
                }
            };

            this.logger = new Mock<ILogger<TrackEventsConsumer>>();

            this.trackEventsConsumer = new TrackEventsConsumer(
                this.consumerMock.Object,
                this.trackEventHandlerMock.Object,
                this.kafkaSettings,
                this.logger.Object);
        }

        [Fact]
        public async Task ExecuteAsync_MessageConsumed_HandlerCalled()
        {
            // Arrange
            var trackEvent = new TrackEvent
            {
                IpAddress = "8.8.8.8",
                Referrer = "google.com",
                UserAgent = "Postman"
            };

            var stoppingToken = new CancellationTokenSource(TimeSpan.FromSeconds(1)).Token;

            var consumeResult = new ConsumeResult<Ignore, TrackEvent>();
            consumeResult.Message = new Message<Ignore, TrackEvent>
            {
                Timestamp = new Timestamp(DateTime.UtcNow, TimestampType.CreateTime),
                Value = trackEvent
            };
            

            this.consumerMock.Setup(m => m.Subscribe(kafkaSettings.Consumers[typeof(TrackEvent).Name.ToString()].DefaultTopic)).Verifiable();
            this.consumerMock.Setup(m => m.Consume(CancellationToken.None)).Returns(consumeResult);
            this.consumerMock.Setup(m => m.Close()).Verifiable();

            this.trackEventHandlerMock.Setup(m => m.HandleAsync(consumeResult.Timestamp.UtcDateTime, It.IsAny<TrackEvent>())).Verifiable();

            // Act
            await this.trackEventsConsumer.StartAsync(stoppingToken);

            // Assert
            this.consumerMock.Verify();
            this.trackEventHandlerMock.Verify();
        }

        [Fact]
        public async Task ExecuteAsync_ConsumerThrowsException_HandlerNotCalled()
        {
            // Arrange
            var trackEvent = new TrackEvent
            {
                IpAddress = "8.8.8.8",
                Referrer = "google.com",
                UserAgent = "Postman"
            };

            var stoppingToken = new CancellationTokenSource(TimeSpan.FromSeconds(1)).Token;

            var consumeResult = new ConsumeResult<Ignore, TrackEvent>();
            consumeResult.Message = new Message<Ignore, TrackEvent>
            {
                Timestamp = new Timestamp(DateTime.UtcNow, TimestampType.CreateTime),
                Value = trackEvent
            };


            this.consumerMock.Setup(m => m.Subscribe(kafkaSettings.Consumers[typeof(TrackEvent).Name.ToString()].DefaultTopic)).Verifiable();
            this.consumerMock.Setup(m => m.Consume(CancellationToken.None)).Throws(new Exception());
            this.consumerMock.Setup(m => m.Close()).Verifiable();

            this.trackEventHandlerMock.Setup(m => m.HandleAsync(consumeResult.Timestamp.UtcDateTime, It.IsAny<TrackEvent>())).Verifiable();

            // Act
            await this.trackEventsConsumer.StartAsync(stoppingToken);

            // Assert
            this.consumerMock.Verify();
            this.trackEventHandlerMock.Verify(m => m.HandleAsync(consumeResult.Timestamp.UtcDateTime, It.IsAny<TrackEvent>()), Times.Never);
        }
    }
}
