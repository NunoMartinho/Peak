namespace PixelService.Tests.Unit.Application
{
    using PixelService.Application.Messaging;
    using PixelService.Domain.Tracks;
    using PixelService.Infrastructure.CrossCutting.Settings;
    using PixelService.Messaging.Events.Tracks;

    using Confluent.Kafka;

    using Microsoft.Extensions.Logging;
    using Moq;

    public class TrackMessaginServiceTests
    {
        private readonly Mock<IProducer<Null, TrackEvent>> producerMock;
        private readonly KafkaSettings kafkaSettings;
        private readonly Mock<ILogger<TrackMessagingService>> logger;

        private readonly TrackMessagingService trackMessagingService;

        public TrackMessaginServiceTests()
        {
            this.logger = new Mock<ILogger<TrackMessagingService>>();
            this.producerMock = new Mock<IProducer<Null, TrackEvent>>();
            this.kafkaSettings = new KafkaSettings
            {
                Producers = new Dictionary<string, ProducerSettings>
                {
                    { typeof(TrackEvent).Name
                      , new ProducerSettings
                        {
                          DefaultTopic = "test"
                        }
                    }
                }
            };

            this.trackMessagingService = new TrackMessagingService(
                this.producerMock.Object,
                this.kafkaSettings,
                this.logger.Object);
        }

        [Fact]
        public async Task SendTrackAsync_DeliveryReportPersisted_MessageProduced()
        {
            // Arrange
            var track = new Track
            {
                IpAddress = "8.8.8.8",
                Referrer = "google.com",
                UserAgent = "Postman"
            };

            var deliveryReport = new DeliveryReport<Null, TrackEvent>();
            deliveryReport.Status = PersistenceStatus.Persisted;

            this.producerMock.Setup(m => m.ProduceAsync(
                    this.kafkaSettings.Producers[typeof(TrackEvent).Name].DefaultTopic,
                    It.IsAny<Message<Null, TrackEvent>>(),
                    CancellationToken.None))
                .ReturnsAsync(deliveryReport);

            // Act
            await this.trackMessagingService.SendTrackAsync(track);

            // Assert
            this.producerMock.Verify();
            this.logger.Verify(m => m.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>())
            , Times.Never);
        }

        [Fact]
        public async Task SendTrackAsync_DeliveryReportNotPersisted_MessageNotProduced()
        {
            // Arrange
            var track = new Track
            {
                IpAddress = "8.8.8.8",
                Referrer = "google.com",
                UserAgent = "Postman"
            };

            var deliveryReport = new DeliveryReport<Null, TrackEvent>();
            deliveryReport.Status = PersistenceStatus.NotPersisted;

            this.producerMock.Setup(m => m.ProduceAsync(
                    this.kafkaSettings.Producers[typeof(TrackEvent).Name].DefaultTopic,
                    It.IsAny<Message<Null, TrackEvent>>(),
                    CancellationToken.None))
                .ReturnsAsync(deliveryReport);

            // Act
            await this.trackMessagingService.SendTrackAsync(track);

            // Assert
            this.producerMock.Verify();
            this.logger.Verify(m => m.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>())
            , Times.Once);
        }

        [Fact]
        public async Task SendTrackAsync_ProducerThrowsException_MessageNotProduced()
        {
            // Arrange
            var track = new Track
            {
                IpAddress = "8.8.8.8",
                Referrer = "google.com",
                UserAgent = "Postman"
            };

            var deliveryReport = new DeliveryReport<Null, TrackEvent>();
            deliveryReport.Status = PersistenceStatus.NotPersisted;

            this.producerMock.Setup(m => m.ProduceAsync(
                    this.kafkaSettings.Producers[typeof(TrackEvent).Name].DefaultTopic,
                    It.IsAny<Message<Null, TrackEvent>>(),
                    CancellationToken.None))
                .ThrowsAsync(new Exception());

            // Act
            await this.trackMessagingService.SendTrackAsync(track);

            // Assert
            this.producerMock.Verify();
            this.logger.Verify(m => m.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>())
            , Times.Once);
        }
    }
}
