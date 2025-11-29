using Moq;
using Moq.Protected;
using Users.Domain.Interfaces;
using Users.Domain.Entities;
using System.Net;
using Users.Domain.Enums;
using Users.Application.Features.Commands.ChangeUserStatusBase.ActivateUser;
using Users.Application.Features.Commands.ChangeUserStatusBase.DeactivateUser;

namespace Users.Tests.Unit_Tests.Handlers
{
    public class UserStatusHandlerTests
    {
        private readonly Mock<IUserRepository> _mockRepo;
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly Guid TestUserId = Guid.NewGuid();

        public UserStatusHandlerTests()
        {
            _mockRepo = new Mock<IUserRepository>();
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        }

        private HttpClient SetupMockHttpClient()
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Patch),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("http://dummy.api/")
            };

            return httpClient;
        }


        [Fact]
        public async Task Activate_ExistingUser_ReturnsTrueAndActivatesState()
        {
            var user = new User(TestUserId, "Test", "test@mail.com", "hash", Role.User);
            user.Deactivate();
            var command = new ActivateUserCommand(TestUserId);

            _mockRepo.Setup(r => r.GetByIdAsync(TestUserId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(user);

            _mockHttpClientFactory.Setup(f => f.CreateClient("ProductsApi")).Returns(SetupMockHttpClient());
            var handler = new ActivateUserHandler(_mockRepo.Object, _mockHttpClientFactory.Object);

            var result = await handler.Handle(command, CancellationToken.None);

            Assert.True(result);
            Assert.True(user.IsActive);

            _mockRepo.Verify(r => r.Update(user), Times.Once);
            _mockRepo.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mockHttpClientFactory.Verify(f => f.CreateClient("ProductsApi"), Times.Once);
        }

        [Fact]
        public async Task Activate_NonExistingUser_ReturnsFalse()
        {
            var command = new ActivateUserCommand(TestUserId);

            _mockRepo.Setup(r => r.GetByIdAsync(TestUserId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync((User)null!);

            _mockHttpClientFactory.Setup(f => f.CreateClient("ProductsApi")).Returns(SetupMockHttpClient());
            var handler = new ActivateUserHandler(_mockRepo.Object, _mockHttpClientFactory.Object);

            var result = await handler.Handle(command, CancellationToken.None);

            Assert.False(result);

            _mockRepo.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
            _mockHttpClientFactory.Verify(f => f.CreateClient(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Deactivate_ExistingUser_ReturnsTrueAndDeactivatesState()
        {
            var user = new User(TestUserId, "Test", "test@mail.com", "hash", Role.User);
            var command = new DeactivateUserCommand(TestUserId);

            _mockRepo.Setup(r => r.GetByIdAsync(TestUserId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(user);

            _mockHttpClientFactory.Setup(f => f.CreateClient("ProductsApi")).Returns(SetupMockHttpClient());
            var handler = new DeactivateUserHandler(_mockRepo.Object, _mockHttpClientFactory.Object);

            var result = await handler.Handle(command, CancellationToken.None);

            Assert.True(result);
            Assert.False(user.IsActive);

            _mockRepo.Verify(r => r.Update(user), Times.Once);
            _mockRepo.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mockHttpClientFactory.Verify(f => f.CreateClient("ProductsApi"), Times.Once); 
        }

        [Fact]
        public async Task Deactivate_NonExistingUser_ReturnsFalse()
        {
            var command = new DeactivateUserCommand(TestUserId);

            _mockRepo.Setup(r => r.GetByIdAsync(TestUserId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync((User)null!);

            _mockHttpClientFactory.Setup(f => f.CreateClient("ProductsApi")).Returns(SetupMockHttpClient());
            var handler = new DeactivateUserHandler(_mockRepo.Object, _mockHttpClientFactory.Object);

            var result = await handler.Handle(command, CancellationToken.None);

            Assert.False(result);

            _mockRepo.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
            _mockHttpClientFactory.Verify(f => f.CreateClient(It.IsAny<string>()), Times.Never);
        }
    }
}