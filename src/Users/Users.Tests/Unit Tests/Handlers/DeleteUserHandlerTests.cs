using Xunit;
using Moq;
using Users.Domain.Interfaces;
using Users.Application.Features.Commands.DeleteUser;
using Users.Domain.Entities;
using Users.Domain.Enums;
using System.Net;
using Moq.Protected;

namespace Users.Tests.Unit_Tests.Handlers
{
    public class DeleteUserHandlerTests
    {
        private readonly Mock<IUserRepository> _mockRepo;
        private readonly DeleteUserHandler _handler;
        private readonly Guid TestUserId = Guid.NewGuid();
        private readonly Mock<IHttpClientFactory> _mockhttpClientFactory;

        public DeleteUserHandlerTests()
        {
            _mockRepo = new Mock<IUserRepository>();
            _mockhttpClientFactory = new Mock<IHttpClientFactory>();
            SetupHttpClientFactory(HttpStatusCode.NoContent);

            _handler = new DeleteUserHandler(_mockRepo.Object, _mockhttpClientFactory.Object);
        }
        private void SetupHttpClientFactory(HttpStatusCode expectedStatusCode)
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Delete &&
                        req.RequestUri!.ToString().Contains($"/api/products/owner/{TestUserId}/delete")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = expectedStatusCode,
                    Content = new StringContent(string.Empty)
                });

            var client = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("http://productsapi/")
            };

            _mockhttpClientFactory
                .Setup(f => f.CreateClient("ProductsApi"))
                .Returns(client);
        }

        [Fact]
        public async Task Handle_ExistingUser_ReturnsTrueAndCallsDelete()
        {
            var user = new User(TestUserId, "Test", "test@mail.com", "hash", Role.User);
            var command = new DeleteUserCommand(TestUserId);

            _mockRepo.Setup(r => r.GetByIdAsync(TestUserId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(user);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result);

            _mockRepo.Verify(r => r.Delete(user), Times.Once);
            _mockRepo.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NonExistingUser_ReturnsFalse()
        {
            var command = new DeleteUserCommand(TestUserId);

            _mockRepo.Setup(r => r.GetByIdAsync(TestUserId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync((User)null!);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.False(result);

            _mockRepo.Verify(r => r.Delete(It.IsAny<User>()), Times.Never);
            _mockRepo.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ProductsApiFailure_ReturnsFalseAndDoesNotDeleteUser()
        {
            var user = new User(TestUserId, "Test", "test@mail.com", "hash", Role.User);
            var command = new DeleteUserCommand(TestUserId);

            _mockRepo.Setup(r => r.GetByIdAsync(TestUserId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(user);

            SetupHttpClientFactory(HttpStatusCode.InternalServerError);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.False(result);

            _mockRepo.Verify(r => r.Delete(user), Times.Never);
            _mockRepo.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}