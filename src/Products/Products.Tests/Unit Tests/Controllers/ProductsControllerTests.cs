using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Products.Api.Controllers;
using Products.Application.DTOs;
using Products.Application.Features.Commands.CreateProduct;
using Products.Application.Features.Commands.DeleteProduct;
using Products.Application.Features.Commands.UpdateProduct;
using Products.Application.Features.Queries.GetProductById;
using System.Security.Claims;


namespace Products.Tests.Unit_Tests.Controllers
{
    public class ProductsControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly ProductsController _controller;
        private readonly Guid _ownerId = Guid.NewGuid();
        private readonly Guid _productId = Guid.NewGuid();

        public ProductsControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _controller = new ProductsController(_mediatorMock.Object);
        }

        [Fact]
        public async Task CreateProduct_ValidCommand_ReturnsCreatedAtActionWithId()
        {
            var createDto = new CreateProductDto { Name = "New Phone", Price = 999m };
            var expectedId = Guid.NewGuid();

            SetupUserIdentity(_ownerId);

            _mediatorMock.Setup(m => m.Send(
                It.IsAny<CreateProductCommand>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedId);

            var result = await _controller.Create(createDto);

            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);

            Assert.Equal(expectedId, createdAtActionResult.Value);
        }

        [Fact]
        public async Task UpdateProduct_ExistingProduct_ReturnsNoContent()
        {
            var updateDto = new UpdateProductDto { Name = "Updated Phone", Price = 1099m, IsAvailable = true };

            SetupUserIdentity(_ownerId);

            _mediatorMock.Setup(m => m.Send(
                It.IsAny<UpdateProductCommand>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _controller.Update(_productId, updateDto);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdateProduct_ProductNotFound_ReturnsNotFound()
        {
            var updateDto = new UpdateProductDto { Name = "Updated Phone", Price = 1099m, IsAvailable = true };

            SetupUserIdentity(_ownerId);

            _mediatorMock.Setup(m => m.Send(
                It.IsAny<UpdateProductCommand>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _controller.Update(_productId, updateDto);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteProduct_ExistingProduct_ReturnsNoContent()
        {
            SetupUserIdentity(_ownerId);

            _mediatorMock.Setup(m => m.Send(
                It.IsAny<DeleteProductCommand>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _controller.Delete(_productId);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteProduct_ProductNotFound_ReturnsNotFound()
        {
            SetupUserIdentity(_ownerId);

            _mediatorMock.Setup(m => m.Send(
                It.IsAny<DeleteProductCommand>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _controller.Delete(_productId);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetProductById_ProductExists_ReturnsOkWithDto()
        {
            var expectedDto = new ProductDto(_productId, "Fetched Item", "Desc", 100m, true, _ownerId, DateTime.UtcNow);

            _mediatorMock.Setup(m => m.Send(
                It.IsAny<GetProductByIdQuery>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            var result = await _controller.GetById(_productId);

            var okResult = Assert.IsType<OkObjectResult>(result);

            var actualDto = Assert.IsType<ProductDto>(okResult.Value);
            Assert.Equal(expectedDto.Id, actualDto.Id);
        }

        [Fact]
        public async Task GetProductById_ProductNotFound_ReturnsNotFound()
        {
            ProductDto? nullDto = null;

            _mediatorMock.Setup(m => m.Send(
                It.IsAny<GetProductByIdQuery>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(nullDto);

            var result = await _controller.GetById(_productId);

            Assert.IsType<NotFoundResult>(result);
        }

        private void SetupUserIdentity(Guid ownerId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, ownerId.ToString()) 
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var user = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext { User = user };

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext,
            };
        }
    }
}
