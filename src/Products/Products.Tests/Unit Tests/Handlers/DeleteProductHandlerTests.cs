using Moq;
using Products.Application.Features.Commands.DeleteProduct;
using Products.Domain.Entities;
using Products.Domain.Interfaces;

namespace Products.Tests.Unit_Tests.Handlers
{
    public class DeleteProductHandlerTests
    {
        private readonly Mock<IProductRepository> _repositoryMock;
        private readonly DeleteProductHandler _handler;
        private readonly Guid _productId = Guid.NewGuid();
        private readonly Guid _ownerId = Guid.NewGuid();
        private readonly DeleteProductCommand _command;
        private readonly Product _product;

        public DeleteProductHandlerTests()
        {
            _repositoryMock = new Mock<IProductRepository>();
            _handler = new DeleteProductHandler(_repositoryMock.Object);
            _command = new DeleteProductCommand(_productId, _ownerId);
            _product = new Product(_productId, "Test Product", "Description", 100, _ownerId);
        }

        [Fact]
        public async Task Handle_ProductExistsAndOwnerMatches_DeletesProductAndSavesChanges()
        {
            _repositoryMock.Setup(r => r.GetByIdAsync(_productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_product);

            var result = await _handler.Handle(_command, CancellationToken.None);

            Assert.True(result);
            _repositoryMock.Verify(r => r.Delete(_product), Times.Once);
            _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ProductNotFound_ReturnsFalseAndNoChangesSaved()
        {
            _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product)null); 

            var result = await _handler.Handle(_command, CancellationToken.None);

            Assert.False(result);
            _repositoryMock.Verify(r => r.Delete(It.IsAny<Product>()), Times.Never);
            _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_OwnerMismatch_ThrowsUnauthorizedAccessException()
        {
            var anotherOwnerId = Guid.NewGuid();
            var productOwnedByAnother = new Product(_productId, "Other", "Desc", 50, anotherOwnerId);

            _repositoryMock.Setup(r => r.GetByIdAsync(_productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(productOwnedByAnother);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _handler.Handle(_command, CancellationToken.None));

            _repositoryMock.Verify(r => r.Delete(It.IsAny<Product>()), Times.Never);
            _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
