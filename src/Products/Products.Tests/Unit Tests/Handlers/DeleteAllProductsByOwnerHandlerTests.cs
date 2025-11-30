using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Products.Domain.Entities;
using Products.Domain.Interfaces;
using Products.Application.Features.Commands.DeleteAllProducts;

namespace Products.Tests.Unit_Tests.Handlers
{
    public class DeleteAllProductsByOwnerHandlerTests
    {
        private readonly Mock<IProductRepository> _mockRepo;
        private readonly DeleteAllProductsByOwnerHandler _handler;
        private readonly Guid TestOwnerId = new Guid("11111111-2222-3333-4444-555555555555");

        public DeleteAllProductsByOwnerHandlerTests()
        {
            _mockRepo = new Mock<IProductRepository>();
            _handler = new DeleteAllProductsByOwnerHandler(_mockRepo.Object);
        }

        [Fact]
        public async Task Handle_ExistingProducts_ReturnsTrueAndCallsDeleteRange()
        {
            var products = new List<Product>
            {
                new Product(Guid.NewGuid(), "Name 1", "Description", 50, TestOwnerId),
                new Product(Guid.NewGuid(), "Name 2", "Description", 50, TestOwnerId)
            };
            var command = new DeleteAllProductsByOwnerCommand(TestOwnerId);

            _mockRepo.Setup(r => r.GetByOwnerIdAsync(TestOwnerId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(products);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result);

            _mockRepo.Verify(r => r.DeleteRange(It.Is<IEnumerable<Product>>(
                list => list.Count() == products.Count)), Times.Once);

            _mockRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NoExistingProducts_ReturnsTrueAndDoesNotCallDeleteRange()
        {
            var products = new List<Product>();
            var command = new DeleteAllProductsByOwnerCommand(TestOwnerId);

            _mockRepo.Setup(r => r.GetByOwnerIdAsync(TestOwnerId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(products);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result);

            _mockRepo.Verify(r => r.DeleteRange(It.IsAny<IEnumerable<Product>>()), Times.Never);

            _mockRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ProductsIsNull_ReturnsTrueAndDoesNotCallDeleteRange()
        {
            var command = new DeleteAllProductsByOwnerCommand(TestOwnerId);

            _mockRepo.Setup(r => r.GetByOwnerIdAsync(TestOwnerId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync((IEnumerable<Product>)null!);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result);

            _mockRepo.Verify(r => r.DeleteRange(It.IsAny<IEnumerable<Product>>()), Times.Never);
            _mockRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}