namespace StorageService.Application.Messaging.Handlers
{
    using StorageService.Messaging.Tracks;

    public interface ITrackEventHandler
    {
        Task HandleAsync(DateTime messageDate, TrackEvent track);
    }
}
