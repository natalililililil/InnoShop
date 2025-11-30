using Microsoft.EntityFrameworkCore;
using Moq;
using Products.Application.DTOs;
using Products.Application.Features.Commands.CreateProduct;
using Products.Domain.Entities;
using Products.Domain.Interfaces;

namespace Products.Tests.Unit_Tests.Handlers
{
    public class CreateProductHandlerTests
    {
        private readonly Mock<IProductRepository> _repositoryMock;
        private readonly CreateProductHandler _handler;
        private readonly Guid _ownerId = Guid.NewGuid();
        private readonly CreateProductCommand _command;

        public CreateProductHandlerTests()
        {
            _repositoryMock = new Mock<IProductRepository>();
            _handler = new CreateProductHandler(_repositoryMock.Object);

            var dto = new CreateProductDto
            {
                Name = "Test Product",
                Description = "Description",
                Price = 10,
                IsAvailable = true
            };
            _command = new CreateProductCommand(_ownerId, dto);
        }

        [Fact]
        public async Task Handle_ValidCommand_CreatesProductAndSavesChanges()
        {
            var resultId = await _handler.Handle(_command, CancellationToken.None);

            _repositoryMock.Verify(r => r.CreateAsync(
                It.Is<Product>(p 
                => p.Name == _command.Product.Name && p.OwnerId == _ownerId),
                It.IsAny<CancellationToken>()),Times.Once);

            _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            Assert.NotEqual(Guid.Empty, resultId);
        }

        [Fact]
        public async Task Handle_RepositoryFailsOnCreation_ThrowsException()
        {
            var expectedException = new InvalidOperationException("Ошибка базы данных при создании");

            _repositoryMock.Setup(r => r.CreateAsync(
                It.IsAny<Product>(),
                It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedException);

            var actualException = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _handler.Handle(_command, CancellationToken.None));

            Assert.Equal(expectedException.Message, actualException.Message);

            _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_RepositoryFailsOnSaving_ThrowsException()
        {
            var expectedException = new DbUpdateException("Ошибка при сохранении изменений");

            _repositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedException);

            var actualException = await Assert.ThrowsAsync<DbUpdateException>(() =>
                _handler.Handle(_command, CancellationToken.None));

            Assert.Equal(expectedException.Message, actualException.Message);

            _repositoryMock.Verify(r => r.CreateAsync(
                It.IsAny<Product>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
