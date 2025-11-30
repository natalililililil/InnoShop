using Moq;
using Products.Application.Features.Commands.SoftRestoreProducts;
using Products.Domain.Entities;
using Products.Domain.Interfaces;

namespace Products.Tests.Unit_Tests.Handlers
{
    public class SoftRestoreAllProductsByOwnerHandlerTests
    {
        private readonly Mock<IProductRepository> _repositoryMock;
        private readonly SoftRestoreAllProductsByOwnerHandler _handler;
        private readonly Guid _ownerId = Guid.NewGuid();
        private readonly SoftRestoreAllProductsByOwnerCommand _command;

        public SoftRestoreAllProductsByOwnerHandlerTests()
        {
            _repositoryMock = new Mock<IProductRepository>();
            _handler = new SoftRestoreAllProductsByOwnerHandler(_repositoryMock.Object);
            _command = new SoftRestoreAllProductsByOwnerCommand(_ownerId);
        }

        [Fact]
        public async Task Handle_ProductsExist_MarksAllAsRestoreAndSavesChanges()
        {
            var products = new List<Product>
            {
                new Product("Product Test 1", "Description 1", 10, _ownerId),
                new Product("Product Test 2", "Description 2", 20, _ownerId)
            };

            _repositoryMock.Setup(r => r.GetByOwnerIdAsync(_ownerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(products);

            var result = await _handler.Handle(_command, CancellationToken.None);

            Assert.True(result);
            Assert.All(products, p => Assert.True(p.IsDeleted == false));
            _repositoryMock.Verify(r => r.Update(It.IsAny<Product>()), Times.Exactly(2));
            _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NoProductsFound_ReturnsFalseAndNoChangesSaved()
        {
            _repositoryMock.Setup(r => r.GetByOwnerIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Product>());

            var result = await _handler.Handle(_command, CancellationToken.None);

            Assert.False(result);
            _repositoryMock.Verify(r => r.Update(It.IsAny<Product>()), Times.Never);
            _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
