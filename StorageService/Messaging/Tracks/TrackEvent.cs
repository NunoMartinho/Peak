namespace StorageService.Messaging.Tracks
{
    public class TrackEvent
    {
        public string Referrer { get; set; }

        public string UserAgent { get; set; }

        public string IpAddress { get; set; }
    }
}