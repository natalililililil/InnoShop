using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text;
using System.Text.Json;
using Products.Application.DTOs;
using Products.Domain.Entities;

namespace Products.Tests.Integration_Tests.API
{
    public class ProductsControllerTests : IntegrationTestBase
    {
        private const string BaseUrl = "api/products";
        private readonly Guid _testOwnerId = new Guid("4b3356e9-20f7-4147-9753-3e7401a755d9");
        private const string AdminRole = "Admin";
        private const string ApiKeyHeaderName = "X-Internal-Api-Key";
        private const string UserRole = "User";
        private const string TestApiKey = "test-internal-key-value";

        public ProductsControllerTests(CustomWebApplicationFactory factory) : base(factory)
        {
            _client.DefaultRequestHeaders.Add(ApiKeyHeaderName, TestApiKey);
        }

        private StringContent GetStringContent<T>(T obj)
        {
            var json = JsonSerializer.Serialize(obj);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        [Fact]
        public async Task GetAllProducts_ReturnsListOfActiveProducts()
        {
            await SeedProduct(_testOwnerId, "Product 1", 10.00m, isAvailable: true, isDeleted: false);
            await SeedProduct(_testOwnerId, "Product 2 (Deleted)", 20.00m, isAvailable: true, isDeleted: true);

            var response = await _client.GetAsync(BaseUrl);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var products = JsonSerializer.Deserialize<List<ProductDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(products);
            Assert.Contains(products, p => p.Name == "Product 1");
            Assert.DoesNotContain(products, p => p.Name == "Product 2 (Deleted)");
        }

        [Fact]
        public async Task GetById_ProductExists_ReturnsProduct()
        {
            var seededProduct = await SeedProduct(_testOwnerId, "Unique Product", 15.50m);

            var response = await _client.GetAsync($"{BaseUrl}/{seededProduct.Id}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var product = JsonSerializer.Deserialize<ProductDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(product);
            Assert.Equal(seededProduct.Id, product.Id);
            Assert.Equal("Unique Product", product.Name);
        }

        [Fact]
        public async Task GetById_ProductNotExists_Returns404NotFound()
        {
            var nonExistentId = Guid.NewGuid();

            var response = await _client.GetAsync($"{BaseUrl}/{nonExistentId}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Create_AsAuthenticatedUser_ReturnsCreatedAndProductExists()
        {
            AuthenticateClient(_testOwnerId, UserRole);
            var newProductDto = new CreateProductDto
            {
                Name = "New Test Product",
                Description = "A product for testing.",
                Price = 99.99m,
                IsAvailable = true
            };

            var response = await _client.PostAsync(BaseUrl, GetStringContent(newProductDto));

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(response.Headers.Location);

            var content = await response.Content.ReadAsStringAsync();
            var createdProductId = JsonSerializer.Deserialize<Guid>(content);

            var createdProduct = await _context.Set<Product>().SingleOrDefaultAsync(p => p.Id == createdProductId);
            Assert.NotNull(createdProduct);
            Assert.Equal("New Test Product", createdProduct.Name);
            Assert.Equal(_testOwnerId, createdProduct.OwnerId);
        }

        [Fact]
        public async Task Create_AsUnauthenticated_Returns401Unauthorized()
        {
            ClearAuthentication();
            var newProductDto = new CreateProductDto
            {
                Name = "Unauthorized Product",
                Description = "Fail.",
                Price = 1.00m,
                IsAvailable = true
            };

            var response = await _client.PostAsync(BaseUrl, GetStringContent(newProductDto));

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Update_ProductExists_ReturnsNoContentAndUpdates()
        {
            var productToUpdate = await SeedProduct(_testOwnerId, "Old Name", 100.00m);
            AuthenticateClient(_testOwnerId, UserRole);

            var updateDto = new UpdateProductDto
            {
                Name = "Updated Name",
                Description = "New Description",
                Price = 150.00m,
                IsAvailable = false
            };

            var response = await _client.PutAsync($"{BaseUrl}/{productToUpdate.Id}", GetStringContent(updateDto));

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            var updatedProduct = await _context.Set<Product>().AsNoTracking().FirstOrDefaultAsync(p => p.Id == productToUpdate.Id);

            Assert.NotNull(updatedProduct);
            Assert.Equal("Updated Name", updatedProduct.Name);
            Assert.Equal(150.00m, updatedProduct.Price);
            Assert.False(updatedProduct.IsAvailable);
        }

        [Fact]
        public async Task Update_NonExistentProduct_Returns404NotFound()
        {
            AuthenticateClient(_testOwnerId, UserRole);
            var nonExistentId = Guid.NewGuid();
            var updateDto = new UpdateProductDto
            {
                Name = "Fails",
                Description = "Test",
                Price = 1.00m,
                IsAvailable = true
            };

            var response = await _client.PutAsync($"{BaseUrl}/{nonExistentId}", GetStringContent(updateDto));

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Delete_ProductExists_ReturnsNoContentAndDeletes()
        {
            var productToDelete = await SeedProduct(_testOwnerId, "Delete Me", 5.00m);
            AuthenticateClient(_testOwnerId, UserRole);

            var response = await _client.DeleteAsync($"{BaseUrl}/{productToDelete.Id}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            var deletedProduct = await _context.Set<Product>().AsNoTracking().FirstOrDefaultAsync(p => p.Id == productToDelete.Id);

            Assert.Null(deletedProduct);
        }

        [Fact]
        public async Task SoftDelete_ExistingOwner_ReturnsNoContentAndMarksAsDeleted()
        {
            var ownerId = Guid.NewGuid();
            await SeedProduct(ownerId, "Owner Product 1", 10m);
            await SeedProduct(ownerId, "Owner Product 2", 20m);

            var response = await _client.PatchAsync($"{BaseUrl}/owner/{ownerId}/soft-delete", null);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            var products = await _context.Set<Product>().AsNoTracking().Where(p => p.OwnerId == ownerId).ToListAsync();

            Assert.True(products.All(p => p.GetType().GetProperty("IsDeleted")!.GetValue(p)!.Equals(true)));
        }

        [Fact]
        public async Task SoftRestore_ExistingOwner_ReturnsNoContentAndRestores()
        {
            var ownerId = Guid.NewGuid();
            await SeedProduct(ownerId, "Deleted Product 1", 10m, isDeleted: true);
            await SeedProduct(ownerId, "Deleted Product 2", 20m, isDeleted: true);

            var response = await _client.PatchAsync($"{BaseUrl}/owner/{ownerId}/soft-restore", null);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var products = await _context.Set<Product>()
                .AsNoTracking()
                .Where(p => p.OwnerId == ownerId)
                .ToListAsync();

            Assert.True(products.All(p => !p.IsDeleted));
        }

        [Fact]
        public async Task Search_ByKeyword_ReturnsMatchingProducts()
        {
            var keyword = "UniqueBook";
            await SeedProduct(_testOwnerId, keyword + " - The Best", 10.00m, isDeleted: false);
            await SeedProduct(_testOwnerId, "Another UniqueBook", 20.00m, isDeleted: false);
            await SeedProduct(_testOwnerId, "NonMatching Item", 30.00m, isDeleted: false);
            await SeedProduct(_testOwnerId, "Deleted " + keyword, 40.00m, isDeleted: true);

            var response = await _client.GetAsync($"{BaseUrl}/search?query={keyword}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var products = JsonSerializer.Deserialize<List<ProductDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(products);
            Assert.Equal(2, products.Count);
            Assert.Contains(products, p => p.Name.Contains("The Best"));
            Assert.Contains(products, p => p.Name.Contains("Another"));
            Assert.DoesNotContain(products, p => p.Name.Contains("NonMatching"));
            Assert.DoesNotContain(products, p => p.Name.Contains("Deleted"));
        }

        [Fact]
        public async Task Filter_ByPriceAndAvailability_ReturnsCorrectProducts()
        {
            await SeedProduct(_testOwnerId, "Cheap Available", 10m, isAvailable: true);
            await SeedProduct(_testOwnerId, "Expensive Unavailable", 60m, isAvailable: false);
            await SeedProduct(_testOwnerId, "Medium Available (Match)", 50m, isAvailable: true);
            await SeedProduct(_testOwnerId, "High Available (Match)", 120m, isAvailable: true);
            await SeedProduct(_testOwnerId, "Deleted High", 150m, isAvailable: true, isDeleted: true);

            var minPrice = 50m;
            var maxPrice = 130m;
            var isAvailable = true;

            var response = await _client.GetAsync($"{BaseUrl}/filter?minPrice={minPrice}&maxPrice={maxPrice}&isAvailable={isAvailable}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var products = JsonSerializer.Deserialize<List<ProductDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(products);
            Assert.Equal(2, products.Count);
            Assert.Contains(products, p => p.Name.Contains("Medium Available (Match)"));
            Assert.Contains(products, p => p.Name.Contains("High Available (Match)"));
            Assert.DoesNotContain(products, p => p.Name.Contains("Cheap Available"));
            Assert.DoesNotContain(products, p => p.Name.Contains("Expensive Unavailable"));
            Assert.DoesNotContain(products, p => p.Name.Contains("Deleted High"));
        }

        [Fact]
        public async Task DeleteAllByOwner_ExistingOwner_ReturnsNoContentAndDeletesAllProducts()
        {
            var ownerToDelete = Guid.NewGuid();
            await SeedProduct(ownerToDelete, "Product 1 to Delete", 10m);
            await SeedProduct(ownerToDelete, "Product 2 to Delete", 20m);
            await SeedProduct(_testOwnerId, "Unrelated Product", 50m);

            var response = await _client.DeleteAsync($"{BaseUrl}/owner/{ownerToDelete}/delete");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var deletedProducts = await _context.Set<Product>()
                .AsNoTracking()
                .Where(p => p.OwnerId == ownerToDelete)
                .ToListAsync();

            Assert.Empty(deletedProducts);

            var unaffectedProduct = await _context.Set<Product>()
                .AsNoTracking()
                .SingleOrDefaultAsync(p => p.OwnerId == _testOwnerId);

            Assert.NotNull(unaffectedProduct);
            Assert.Equal("Unrelated Product", unaffectedProduct.Name);
        }

        [Fact]
        public async Task DeleteAllByOwner_NonExistingOwner_ReturnsNotFound()
        {
            var nonExistentOwnerId = Guid.NewGuid();

            var response = await _client.DeleteAsync($"{BaseUrl}/owner/{nonExistentOwnerId}/delete");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
    }
}