using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Products.Domain.Entities;
using Products.Infrastructure.Persistence;

namespace Products.Tests.Integration_Tests.API
{
    public abstract class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>, IDisposable
    {
        protected readonly CustomWebApplicationFactory _factory;
        protected readonly HttpClient _client;
        protected readonly ProductsDbContext _context;

        public IntegrationTestBase(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            ClearAuthentication();

            var scope = _factory.Services.CreateScope();
            _context = scope.ServiceProvider.GetRequiredService<ProductsDbContext>();

            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
        }

        protected StringContent GetStringContent(object obj)
        {
            var json = JsonSerializer.Serialize(obj);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        protected void AuthenticateClient(Guid userId, string role = "User")
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TestAuthHandler.AuthenticationScheme);

            var claimsData = new { UserId = userId.ToString(), Role = role };
            var jsonClaims = JsonSerializer.Serialize(claimsData);
            var base64Claims = Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonClaims));

            _client.DefaultRequestHeaders.Remove("X-Test-Claims");
            _client.DefaultRequestHeaders.Add("X-Test-Claims", base64Claims);
        }

        protected void ClearAuthentication()
        {
            _client.DefaultRequestHeaders.Remove("Authorization");
            _client.DefaultRequestHeaders.Remove("X-Test-Claims");
        }

        protected async Task<Product> SeedProduct(Guid ownerId, string name, decimal price, bool isAvailable = true, bool isDeleted = false)
        {
            var productId = Guid.NewGuid();
            var product = new Product(productId, name, "Description", price, ownerId);

            product.SetAvailability(isAvailable);

            if (isDeleted)
            {
                product.SoftDelete();
            }
            else
            {
                product.Restore();
            }

            await _context.Set<Product>().AddAsync(product);
            await _context.SaveChangesAsync();

            _context.Entry(product).State = Microsoft.EntityFrameworkCore.EntityState.Detached;

            return product;
        }

        public void Dispose()
        {
            _client.Dispose();
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}