namespace Tests.Unit.Application
{
    using StorageService.Application.Messaging.Handlers;
    using StorageService.Infrastructure.CrossCutting.Settings;
    using StorageService.Messaging.Tracks;

    using Microsoft.Extensions.Logging;

    using Moq;
    using Xunit;

    public class TrackEventHandlerTests : IClassFixture<FileTestsFixture>
    {
        private readonly ServiceSettings serviceSettings;
        private readonly Mock<ILogger<TrackEventHandler>> logger;
        private readonly FileTestsFixture fileTestsFixture;

        private readonly TrackEventHandler trackEventHandler;

        public TrackEventHandlerTests(FileTestsFixture fileTestsFixture)
        {
            this.fileTestsFixture = fileTestsFixture;
            this.serviceSettings = new ServiceSettings
            {
                StorageFilePath = this.fileTestsFixture.TestFilePath
            };
            this.logger = new Mock<ILogger<TrackEventHandler>>();

            this.trackEventHandler = new TrackEventHandler(
                this.serviceSettings,
                this.logger.Object);

        }

        [Theory]
        [InlineData("Postman", "google.com", "8.8.8.8")]
        [InlineData(null, "google.com", "8.8.8.8")]
        [InlineData(null, null, "8.8.8.8")]
        public async Task HandleAsync_ValidData_DataSavedToLogFile(string userAgent, string referrer, string ip)
        {
            // Arrange
            var track = new TrackEvent
            {
                IpAddress = ip,
                Referrer = referrer,
                UserAgent = userAgent
            };

            var date = DateTime.Now;

            var expectedTrackLog = $"{date.ToString("o")} | {track.Referrer ?? "null"} | {track.UserAgent ?? "null"} | {track.IpAddress}";

            // Act
            await this.trackEventHandler.HandleAsync(date, track);

            // Assert
            Assert.True(this.CheckIfLogExists(expectedTrackLog));
        }

        [Fact]
        public async Task HandleAsync_InvalidData_DataNotSaved()
        {
            // Arrange
            var track = new TrackEvent
            {
                IpAddress = null,
                Referrer = "google.com",
                UserAgent = "Postman"
            };

            var date = DateTime.Now;

            var expectedTrackLog = $"{date.ToString("o")} | {track.Referrer ?? "null"} | {track.UserAgent ?? "null"} | {track.IpAddress}";

            // Act
            await this.trackEventHandler.HandleAsync(date, track);

            // Assert
            Assert.False(this.CheckIfLogExists(expectedTrackLog));
        }

        private bool CheckIfLogExists(string log)
        {
            using (StreamReader reader = new StreamReader(this.serviceSettings.StorageFilePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Trim() == log.Trim())
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
