using Moq;
using Products.Application.DTOs;
using Products.Application.Features.Commands.UpdateProduct;
using Products.Domain.Entities;
using Products.Domain.Interfaces;

namespace Products.Tests.Unit_Tests.Handlers
{
    public class UpdateProductHandlerTests
    {
        private readonly Mock<IProductRepository> _repositoryMock;
        private readonly UpdateProductHandler _handler;
        private readonly UpdateProductCommand _command;
        private readonly Guid _productId = Guid.NewGuid();
        private readonly Guid _ownerId = Guid.NewGuid();
        private readonly Product _product;
        private readonly UpdateProductDto _updateProductDto;
        public UpdateProductHandlerTests()
        {
            _repositoryMock = new Mock<IProductRepository>();
            _handler = new UpdateProductHandler(_repositoryMock.Object);
            _product = new Product(_productId, "Old Name", "Old Description", 50, _ownerId);

            _updateProductDto = new UpdateProductDto
            {
                Name = "New Name",
                Description = "New Description",
                Price = 1000,
                IsAvailable = false,
            };

            _command = new UpdateProductCommand(_productId, _ownerId, _updateProductDto);
        }

        [Fact]
        public async Task Handle_ProductExistsAndOwnerMatches_UpdatesProductAndSavesChanges()
        {
            var command = new UpdateProductCommand(_productId, _ownerId, _updateProductDto);

            _repositoryMock.Setup(r => r.GetByIdAsync(_productId, It.IsAny<CancellationToken>())).ReturnsAsync(_product);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result);
            _repositoryMock.Verify(r => r.Update(_product), Times.Once);
            _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            Assert.Equal(_updateProductDto.Name, _product.Name);
            Assert.Equal(_updateProductDto.Price, _product.Price);
            Assert.Equal(_updateProductDto.IsAvailable, _product.IsAvailable);
        }

        [Fact]
        public async Task Handle_OwnerMismatch_ThrowsUnauthorizedAccessException()
        {
            var anotherOwnerId = Guid.NewGuid();
            
            var command = new UpdateProductCommand(_productId, _ownerId, _updateProductDto);

            var productOwnedByAnother = new Product(_productId, "Other", "Description", 50, anotherOwnerId);

            _repositoryMock.Setup(r => r.GetByIdAsync(_productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(productOwnedByAnother);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _handler.Handle(command, CancellationToken.None));

            _repositoryMock.Verify(r => r.Update(It.IsAny<Product>()), Times.Never);
            _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ProductNotFound_ReturnsFalseAndNoChangesSaved()
        {
            _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product)null);

            var result = await _handler.Handle(_command, CancellationToken.None);

            Assert.False(result);
            _repositoryMock.Verify(r => r.Update(It.IsAny<Product>()), Times.Never);
            _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
