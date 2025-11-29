using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using Users.Api;
using Users.Application.Services;
using Users.Infrastructure.Persistence;

namespace Users.Tests.Integration_Tests.API
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private string _databaseName;
        private SqliteConnection? _connection;

        public Mock<IEmailService> MockEmailService { get; private set; }
        public Mock<ITokenService> MockTokenService { get; private set; }

        public CustomWebApplicationFactory()
        {
            _databaseName = Guid.NewGuid().ToString();
            MockEmailService = new Mock<IEmailService>();
            MockTokenService = new Mock<ITokenService>();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                var dbContextOptionsDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<UserDbContext>));
                if (dbContextOptionsDescriptor != null)
                {
                    services.Remove(dbContextOptionsDescriptor);
                }

                var userDbContextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(UserDbContext));
                if (userDbContextDescriptor != null)
                {
                    services.Remove(userDbContextDescriptor);
                }

                var descriptorsToRemove = services
                    .Where(d => d.ServiceType.FullName!.Contains("DbContext") ||
                                d.ServiceType.FullName!.Contains("IHistoryRepository") ||
                                d.ServiceType.FullName!.Contains("IDbContextOptions"))
                    .ToList();

                foreach (var descriptor in descriptorsToRemove)
                {
                    services.Remove(descriptor);
                }

                if (_connection == null)
                {
                    _connection = new SqliteConnection($"DataSource={_databaseName};Mode=Memory;Cache=Shared");
                    _connection.Open();
                }

                services.AddDbContext<UserDbContext>(options =>
                {
                    options.UseSqlite(_connection);
                });


                var emailServiceDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IEmailService));
                if (emailServiceDescriptor != null)
                {
                    services.Remove(emailServiceDescriptor);
                }
                services.AddSingleton(MockEmailService.Object);

                var tokenServiceDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(ITokenService));
                if (tokenServiceDescriptor != null)
                {
                    services.Remove(tokenServiceDescriptor);
                }
                services.AddSingleton(MockTokenService.Object);

                var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
                mockHttpMessageHandler
                    .Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.NoContent,
                        Content = new StringContent("")
                    });

                var clientFactoryDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IHttpClientFactory));
                if (clientFactoryDescriptor != null)
                {
                    services.Remove(clientFactoryDescriptor);
                }

                services.AddHttpClient("ProductsApi", client =>
                {
                    client.BaseAddress = new Uri("http://dummy-products-api.com");
                })
                .ConfigurePrimaryHttpMessageHandler(() => mockHttpMessageHandler.Object);


                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.AuthenticationScheme;
                    options.DefaultChallengeScheme = TestAuthHandler.AuthenticationScheme;
                    options.DefaultScheme = TestAuthHandler.AuthenticationScheme;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.AuthenticationScheme, options => { });

                services.AddAuthorization(options =>
                {
                    options.DefaultPolicy = new AuthorizationPolicyBuilder()
                        .AddAuthenticationSchemes(TestAuthHandler.AuthenticationScheme)
                        .RequireAuthenticatedUser()
                        .Build();
                });

                var configuration = new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        {"JwtSettings:ValidIssuer", "TestIssuer"},
                        {"JwtSettings:ValidAudience", "TestAudience"},
                        {"JwtSettings:ExpiryMinutes", "60"},
                        {"SECRET", "this_is_a_very_long_secret_key_for_testing_purposes_1234567890"}
                    })
                    .Build();

                services.AddSingleton<IConfiguration>(configuration);

                var sp = services.BuildServiceProvider();

                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<UserDbContext>();
                    var logger = scopedServices
                        .GetRequiredService<ILogger<CustomWebApplicationFactory>>();

                    try
                    {
                        db.Database.EnsureCreated();
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "An error occurred during database initialization. Error: {Message}", ex.Message);
                    }
                }
            });
        }

        public TService GetService<TService>() where TService : notnull
        {
            var scope = Server.Services.CreateScope();
            return scope.ServiceProvider.GetRequiredService<TService>();
        }

        protected override void Dispose(bool disposing)
        {
            _connection?.Close();
            _connection?.Dispose();
            base.Dispose(disposing);
        }
    }
}