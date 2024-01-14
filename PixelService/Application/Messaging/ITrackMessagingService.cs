namespace PixelService.Application.Messaging
{
    using Domain.Tracks;

    public interface ITrackMessagingService
    {
        Task SendTrackAsync(Track track);
    }
}
