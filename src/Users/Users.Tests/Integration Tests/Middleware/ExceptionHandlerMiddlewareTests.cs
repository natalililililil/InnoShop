using Microsoft.Extensions.Hosting;
using System.Net;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;

namespace Users.Tests.Integration_Tests.Middleware
{
    public class ExceptionHandlerMiddlewareTests
    {
        private readonly HttpClient _client;

        public ExceptionHandlerMiddlewareTests()
        {
            var hostBuilder = new HostBuilder()
                .ConfigureWebHost(webHost =>
                {
                    webHost.UseTestServer();
                    webHost.ConfigureServices(services =>
                    {
                        services.AddControllers().AddApplicationPart(typeof(ExceptionTestController).Assembly);
                    });
                    webHost.Configure(app =>
                    {
                        app.UseMiddleware<ExceptionHandlerMiddleware>();
                        app.UseRouting();
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapControllers();
                        });
                    });
                });

            var host = hostBuilder.Start();
            _client = host.GetTestClient();
        }

        [Fact]
        public async Task Middleware_HandlesValidationException_Returns400BadRequest()
        {
            var response = await _client.GetAsync("/api/test/validation");
            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("Validation failed message.", content);
            Assert.Contains("message", content);
        }

        [Fact]
        public async Task Middleware_HandlesNotFoundException_Returns404NotFound()
        {
            var response = await _client.GetAsync("/api/test/notfound");
            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Contains("Resource not found.", content);
            Assert.Contains("message", content);
        }

        [Fact]
        public async Task Middleware_HandlesGenericException_Returns500InternalServerError()
        {
            var response = await _client.GetAsync("/api/test/internal");
            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Contains("Произошла непредвиденная ошибка:", content);
            Assert.Contains("message", content);
        }
    }
}
