using Moq;
using Products.Application.Features.Queries.GetProductById;
using Products.Domain.Entities;
using Products.Domain.Interfaces;

namespace Products.Tests.Unit_Tests.Handlers
{
    public class GetProductByIdHandlerTests
    {
        private readonly Mock<IProductRepository> _repositoryMock;
        private readonly GetProductByIdHandler _handler;
        private readonly Guid _productId = Guid.NewGuid();
        private readonly GetProductByIdQuery _query;
        private readonly Guid _ownerId = Guid.NewGuid();

        public GetProductByIdHandlerTests()
        {
            _repositoryMock = new Mock<IProductRepository>();
            _handler = new GetProductByIdHandler(_repositoryMock.Object);
            _query = new GetProductByIdQuery(_productId);
        }

        [Fact]
        public async Task Handle_ProductFoundAndActive_ReturnsProductDto()
        {
            var product = new Product(_productId, "Active Item", "Desc", 100, _ownerId);
            _repositoryMock.Setup(r => r.GetByIdAsync(_productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            var result = await _handler.Handle(_query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(_productId, result.Id);
        }

        [Fact]
        public async Task Handle_ProductIsDeleted_ReturnsNull()
        {
            var deletedProduct = new Product(_productId, "Deleted Item", "Desc", 50, _ownerId);
            deletedProduct.SoftDelete();

            _repositoryMock.Setup(r => r.GetByIdAsync(_productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(deletedProduct);

            var result = await _handler.Handle(_query, CancellationToken.None);

            Assert.Null(result);
        }

        [Fact]
        public async Task Handle_ProductNotFound_ReturnsNull()
        {
            _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product)null);

            var result = await _handler.Handle(_query, CancellationToken.None);

            Assert.Null(result);
        }
    }
}
