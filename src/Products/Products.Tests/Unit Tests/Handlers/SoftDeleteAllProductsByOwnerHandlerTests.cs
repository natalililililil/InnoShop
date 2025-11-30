using Moq;
using Products.Application.Features.Commands.SoftDeteleProducts;
using Products.Domain.Entities;
using Products.Domain.Interfaces;

namespace Products.Tests.Unit_Tests.Handlers
{
    public class SoftDeleteAllProductsByOwnerHandlerTests
    {
        private readonly Mock<IProductRepository> _repositoryMock;
        private readonly SoftDeleteAllProductsByOwnerHandler _handler;
        private readonly Guid _ownerId = Guid.NewGuid();
        private readonly SoftDeleteAllProductsByOwnerCommand _command;

        public SoftDeleteAllProductsByOwnerHandlerTests()
        {
            _repositoryMock = new Mock<IProductRepository>();
            _handler = new SoftDeleteAllProductsByOwnerHandler(_repositoryMock.Object);
            _command = new SoftDeleteAllProductsByOwnerCommand(_ownerId);
        }

        [Fact]
        public async Task Handle_ProductsExist_MarksAllAsDeletedAndSavesChanges()
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
            Assert.All(products, p => Assert.True(p.IsDeleted == true));
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
