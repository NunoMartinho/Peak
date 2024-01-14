namespace Tests.Unit.PixelServiceTests
{
    using System.Net;

    using PixelService;
    using PixelService.Application.Messaging;
    using PixelService.Controllers;
    using PixelService.Domain.Tracks;

    using FluentAssertions;

    using Moq;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Routing;
    using System.Text;

    public class TrackControllerTests
    {
        private readonly Mock<ITrackMessagingService> trackMessagingServiceMock;
        private readonly Mock<HttpContext> httpContextMock;

        private readonly TrackController trackController;

        public TrackControllerTests()
        {
            this.trackMessagingServiceMock = new Mock<ITrackMessagingService>();

            this.trackController = new TrackController(this.trackMessagingServiceMock.Object);

            this.httpContextMock = new Mock<HttpContext>();
            var actionContext = new ActionContext(
                this.httpContextMock.Object,
                new Mock<RouteData>().Object,
                new Mock<ControllerActionDescriptor>().Object,
                new Mock<ModelStateDictionary>().Object);
            var controllerContextMock = new Mock<ControllerContext>(actionContext);
            this.trackController.ControllerContext = controllerContextMock.Object;
        }

        [Fact]
        public async Task GetAsync_NullHeaders_BadRequest()
        {
            // Arrange
            var httpRequestMock = new Mock<HttpRequest>();
            
            httpRequestMock.Setup(req => req.Headers).Returns(null as HeaderDictionary);
            this.httpContextMock.Setup(ctx => ctx.Request).Returns(httpRequestMock.Object);

            // Act
            var result = await this.trackController.GetAsync();

            // Assert
            result.Should().BeOfType(typeof(BadRequestResult));

            this.trackMessagingServiceMock.Verify(m => m.SendTrackAsync(It.IsAny<Track>()), Times.Never());
        }

        [Fact]
        public async Task GetAsync_NullIpAddress_BadRequest()
        {
            // Arrange
            var httpRequestMock = new Mock<HttpRequest>();
            var headers = new HeaderDictionary();
            headers.Add(Constants.USERAGENT, "Postman");
            httpRequestMock.Setup(req => req.Headers).Returns(headers);
            this.httpContextMock.Setup(ctx => ctx.Request).Returns(httpRequestMock.Object);

            var httpConnectiontMock = new Mock<ConnectionInfo>();

            httpConnectiontMock.Setup(req => req.RemoteIpAddress).Returns(null as IPAddress);
            this.httpContextMock.Setup(ctx => ctx.Connection).Returns(httpConnectiontMock.Object);

            // Act
            var result = await this.trackController.GetAsync();

            // Assert
            result.Should().BeOfType(typeof(BadRequestResult));

            this.trackMessagingServiceMock.Verify(m => m.SendTrackAsync(It.IsAny<Track>()), Times.Never());
        }

        [Fact]
        public async Task GetAsync_ValidParameters_OkWithGif()
        {
            // Arrange
            var httpRequestMock = new Mock<HttpRequest>();
            var headers = new HeaderDictionary();
            headers.Add(Constants.USERAGENT, "Postman");
            httpRequestMock.Setup(req => req.Headers).Returns(headers);
            this.httpContextMock.Setup(ctx => ctx.Request).Returns(httpRequestMock.Object);

            var httpConnectiontMock = new Mock<ConnectionInfo>();
            var ip = "8.8.8.8";
            var ipAddress = IPAddress.Parse(ip);
            httpConnectiontMock.Setup(req => req.RemoteIpAddress).Returns(ipAddress);
            this.httpContextMock.Setup(ctx => ctx.Connection).Returns(httpConnectiontMock.Object);

            this.trackMessagingServiceMock.Setup(m => m.SendTrackAsync(
                    It.Is<Track>(t => t.Referrer == null && t.UserAgent == "Postman" && t.IpAddress == ip)))
                .Verifiable();
            // Act
            var result = await this.trackController.GetAsync();

            // Assert
            result.Should().BeOfType(typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            var resultObject = okResult.Value as ContentResult;
            resultObject.Should().NotBeNull();
            resultObject.Content.Should().Be("R0lGODlhAQABAIAAAP///wAAAAAAIfkEAQAAAAAsAAAAAAEAAQAAAgJEAQA7");
            resultObject.ContentType.Should().Be("image/gif");

            this.trackMessagingServiceMock.Verify();
        }
    }
}
