namespace PixelService.Controllers
{
    using PixelService.Application.Messaging;
    using PixelService.Domain.Tracks;

    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("track")]
    public class TrackController : ControllerBase
    {
        private readonly ITrackMessagingService trackMessagingService;

        public TrackController(ITrackMessagingService trackMessagingService)
        {
            this.trackMessagingService = trackMessagingService;
        }

        [HttpGet(Name = "GetTrack")]
        [ProducesResponseType(typeof(ContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BadRequestResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAsync()
        {
            if(HttpContext?.Request?.Headers == null
               || HttpContext.Connection.RemoteIpAddress == null)
            {
                return BadRequest();
            }

            var track = new Track
            {
                Referrer = HttpContext.Request.Headers[Constants.REFERRER],
                UserAgent = HttpContext.Request.Headers[Constants.USERAGENT],
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };

            await this.trackMessagingService.SendTrackAsync(track);

            var result = new ContentResult
            {
                //1x1 transparent gif in base64
                Content = "R0lGODlhAQABAIAAAP///wAAAAAAIfkEAQAAAAAsAAAAAAEAAQAAAgJEAQA7",
                ContentType = "image/gif"
            };

            return Ok(result);
        }
    }
}