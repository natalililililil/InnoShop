using Moq;
using Products.Application.DTOs;
using Products.Application.Features.Queries.GetAllActiveProducts;
using Products.Domain.Entities;
using Products.Domain.Interfaces;

namespace Products.Tests.Unit_Tests.Handlers
{
    public class GetAllActiveProductsHandlerTests
    {
        private readonly Mock<IProductRepository> _repositoryMock;
        private readonly GetAllActiveProductsHandler _handler;
        private readonly Guid _ownerId = Guid.NewGuid();

        public GetAllActiveProductsHandlerTests()
        {
            _repositoryMock = new Mock<IProductRepository>();
            _handler = new GetAllActiveProductsHandler(_repositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ProductsExist_ReturnsAllMappedDtos()
        {
            var query = new GetAllActiveProductsQuery();

            var mockProducts = new List<Product>
            {
                new Product(Guid.NewGuid(), "Active P1", "D1", 10m, _ownerId),
                new Product(Guid.NewGuid(), "Active P2", "D2", 20m, _ownerId)
            };

            _repositoryMock.Setup(r => r.GetAllActiveProductsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockProducts);

            var result = await _handler.Handle(query, CancellationToken.None);

            _repositoryMock.Verify(r => r.GetAllActiveProductsAsync(
                It.IsAny<CancellationToken>()),
                Times.Once);

            var productDtos = Assert.IsAssignableFrom<IEnumerable<ProductDto>>(result);
            Assert.Equal(2, productDtos.Count());

            Assert.Equal("Active P1", productDtos.First().Name);
        }

        [Fact]
        public async Task Handle_NoActiveProductsExist_ReturnsEmptyList()
        {
            var query = new GetAllActiveProductsQuery();

            _repositoryMock.Setup(r => r.GetAllActiveProductsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Product>());

            var result = await _handler.Handle(query, CancellationToken.None);

            _repositoryMock.Verify(r => r.GetAllActiveProductsAsync(
                It.IsAny<CancellationToken>()),
                Times.Once);

            var productDtos = Assert.IsAssignableFrom<IEnumerable<ProductDto>>(result);
            Assert.Empty(productDtos);
        }
    }
}
