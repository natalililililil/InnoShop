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
using Products.Api;
using Products.Infrastructure.Persistence;

namespace Products.Tests.Integration_Tests.API
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private string _databaseName;
        private SqliteConnection? _connection;

        public CustomWebApplicationFactory()
        {
            _databaseName = Guid.NewGuid().ToString();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                var dbContextRelatedDescriptors = services
                    .Where(d => d.ServiceType.FullName!.Contains("DbContext") || d.ServiceType.FullName!.Contains("IHistoryRepository") || 
                        d.ServiceType.FullName!.Contains("IDbContextOptions") || d.ServiceType.FullName!.Contains("IModelCacheKeyFactory") || 
                        d.ServiceType.FullName!.Contains("IDbSetSource") || d.ServiceType.FullName!.Contains("DatabaseProvider")).ToList();


                foreach (var descriptor in dbContextRelatedDescriptors)
                {
                    services.Remove(descriptor);
                }

                var userDbContextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(ProductsDbContext));
                if (userDbContextDescriptor != null) services.Remove(userDbContextDescriptor);


                if (_connection == null)
                {
                    _connection = new SqliteConnection($"DataSource={_databaseName};Mode=Memory;Cache=Shared");
                    _connection.Open();
                }

                var configuration = new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        {"JwtSettings:ValidIssuer", "TestIssuer"},
                        {"JwtSettings:ValidAudience", "TestAudience"},
                        {"ApiKeys:InternalServiceKey", "test-internal-key-value"}
                    })
                    .Build();

                services.AddSingleton<IConfiguration>(configuration);


                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.AuthenticationScheme;
                    options.DefaultChallengeScheme = TestAuthHandler.AuthenticationScheme;
                }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.AuthenticationScheme, options => { });

                services.AddAuthorization(options =>
                {
                    options.DefaultPolicy = new AuthorizationPolicyBuilder()
                        .AddAuthenticationSchemes(TestAuthHandler.AuthenticationScheme)
                        .RequireAuthenticatedUser()
                        .Build();
                });

                services.AddDbContext<ProductsDbContext>(options =>
                {
                    options.UseSqlite(_connection);
                    options.EnableServiceProviderCaching(false);
                });

                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<ProductsDbContext>();
                    db.Database.EnsureCreated();
                }
            });
        }

        protected override void Dispose(bool disposing)
        {
            _connection?.Close();
            _connection?.Dispose();
            base.Dispose(disposing);
        }
    }
}