using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Products.Api.Middleware;
using System.Net;

namespace Products.Tests.Integration_Tests.Middleware
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
            Assert.Contains("error", content);
        }

        [Fact]
        public async Task Middleware_HandlesUnauthorizedAccessException_Returns403Forbidden()
        {
            var response = await _client.GetAsync("/api/test/unauthorized");
            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Contains("Access denied message.", content);
            Assert.Contains("error", content);
        }

        [Fact]
        public async Task Middleware_HandlesArgumentException_Returns400BadRequest()
        {
            var response = await _client.GetAsync("/api/test/argument");
            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("Invalid argument provided.", content);
            Assert.Contains("error", content);
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