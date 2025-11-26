using Moq;
using Products.Application.DTOs;
using Products.Application.Features.Queries.FilterProducts;
using Products.Domain.Entities;
using Products.Domain.Interfaces;

namespace Products.Tests.Unit_Tests.Handlers
{
    public class FilterProductsHandlerTests
    {
        private readonly Mock<IProductRepository> _repositoryMock;
        private readonly FilterProductsHandler _handler;
        private readonly Guid _ownerId = Guid.NewGuid();

        public FilterProductsHandlerTests()
        {
            _repositoryMock = new Mock<IProductRepository>();
            _handler = new FilterProductsHandler(_repositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ValidFilters_CallsRepositoryWithCorrectParametersAndReturnsDtos()
        {
            decimal minPrice = 10;
            decimal maxPrice = 100;
            bool isAvailable = true;
            var query = new FilterProductsQuery(minPrice, maxPrice, isAvailable);

            var products = new List<Product>
            {
                new Product("Product Test 1", "Description 1", 50, _ownerId),
            };

            _repositoryMock.Setup(r => r.FilterAsync(
                minPrice, maxPrice, isAvailable, It.IsAny<CancellationToken>()))
                .ReturnsAsync(products);

            var result = await _handler.Handle(query, CancellationToken.None);

            _repositoryMock.Verify(r => r.FilterAsync(
                minPrice,
                maxPrice,
                isAvailable,
                It.IsAny<CancellationToken>()),
                Times.Once);

            Assert.Single(result);
            Assert.IsType<ProductDto>(result.First());
        }

        [Fact]
        public async Task Handle_NullFilters_CallsRepositoryWithNullParameters()
        {
            var query = new FilterProductsQuery(null, null, null);

            _repositoryMock.Setup(r => r.FilterAsync(
                null, null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Product>());

            await _handler.Handle(query, CancellationToken.None);

            _repositoryMock.Verify(r => r.FilterAsync(
                null,
                null,
                null,
                It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
