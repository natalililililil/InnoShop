using Moq;
using Products.Application.DTOs;
using Products.Application.Features.Queries.SearchProducts;
using Products.Domain.Entities;
using Products.Domain.Interfaces;


namespace Products.Tests.Unit_Tests.Handlers
{
    public class SearchProductsHandlerTests
    {
        private readonly Mock<IProductRepository> _repositoryMock;
        private readonly SearchProductsHandler _handler;
        private readonly Guid _ownerId = Guid.NewGuid();

        public SearchProductsHandlerTests()
        {
            _repositoryMock = new Mock<IProductRepository>();
            _handler = new SearchProductsHandler(_repositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ValidQuery_CallsRepositoryAndReturnsMappedDtos()
        {
            var searchQuery = "Laptop";
            var query = new SearchProductsQuery(searchQuery);

            var mockProducts = new List<Product>
            {
                new Product(Guid.NewGuid(), "HP Laptop", "Fast", 1200m, _ownerId),
                new Product(Guid.NewGuid(), "Dell Laptop Pro", "Powerful", 1500m, _ownerId)
            };

            _repositoryMock.Setup(r => r.SearchAsync(
                searchQuery,
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockProducts);

            var result = await _handler.Handle(query, CancellationToken.None);


            _repositoryMock.Verify(r => r.SearchAsync(
                searchQuery,
                It.IsAny<CancellationToken>()),
                Times.Once);

            var productDtos = Assert.IsAssignableFrom<IEnumerable<ProductDto>>(result);
            Assert.Equal(2, productDtos.Count());

            Assert.Equal("HP Laptop", productDtos.First().Name);
        }

        [Fact]
        public async Task Handle_NoResultsFound_ReturnsEmptyList()
        {
            var searchQuery = "NonExistent";
            var query = new SearchProductsQuery(searchQuery);

            _repositoryMock.Setup(r => r.SearchAsync(
                searchQuery,
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Product>());

            var result = await _handler.Handle(query, CancellationToken.None);

            _repositoryMock.Verify(r => r.SearchAsync(
                searchQuery,
                It.IsAny<CancellationToken>()),
                Times.Once);

            var productDtos = Assert.IsAssignableFrom<IEnumerable<ProductDto>>(result);
            Assert.Empty(productDtos);
        }
    }
}    