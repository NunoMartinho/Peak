namespace StorageService.Application.Messaging.Handlers
{
    using StorageService.Infrastructure.CrossCutting.Settings;
    using StorageService.Messaging.Tracks;

    using Microsoft.Extensions.Logging;

    public class TrackEventHandler : ITrackEventHandler
    {
        private readonly ServiceSettings serviceSettings;
        private readonly ILogger logger;

        public TrackEventHandler(
            ServiceSettings serviceSettings,
            ILogger<TrackEventHandler> logger)
        {
            this.serviceSettings = serviceSettings;
            this.logger = logger;
        }

        public async Task HandleAsync(DateTime messageDate, TrackEvent track)
        {
            if(string.IsNullOrEmpty(track.IpAddress))
            {
                this.logger.LogWarning("Track message not processed. IpAddress could not be null");
                return;
            }

            try
            {
                var filePath = this.serviceSettings.StorageFilePath;

                var dataEntry = $"{messageDate.ToString("o")} | {track.Referrer ?? "null"} | {track.UserAgent ?? "null"} | {track.IpAddress}";

                var fileLock = new object();
                lock (fileLock)
                {
                    using (FileStream fileStream = new FileStream(filePath, FileMode.Append, FileAccess.Write))
                    {
                        using (StreamWriter writer = new StreamWriter(fileStream))
                        {
                            writer.WriteLine(dataEntry);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError($"Error saving Track info to log file: {ex.Message}");
            }
        }
    }
}
