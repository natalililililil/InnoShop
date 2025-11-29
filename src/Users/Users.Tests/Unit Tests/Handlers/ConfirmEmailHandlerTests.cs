using Moq;
using Users.Domain.Interfaces;
using Users.Application.Features.Commands.ConfirmEmail;
using Users.Domain.Entities;
using Users.Domain.Enums;

namespace Users.Tests.Unit_Tests.Handlers
{
    public class ConfirmEmailHandlerTests
    {
        private readonly Mock<IUserRepository> _mockRepo;
        private readonly ConfirmEmailHandler _handler;

        private readonly string ValidEmail = "valid@test.com";
        private readonly string ValidToken = "validToken123";

        public ConfirmEmailHandlerTests()
        {
            _mockRepo = new Mock<IUserRepository>();
            _handler = new ConfirmEmailHandler(_mockRepo.Object);
        }

        [Fact]
        public async Task Handle_Success_ReturnsTrueAndConfirmsEmail()
        {
            var user = new User(Guid.NewGuid(), "Test", ValidEmail, "hash", Role.User);
            user.SetEmailConfirmationToken(ValidToken, DateTime.UtcNow.AddHours(1));

            var command = new ConfirmEmailCommand(ValidEmail, ValidToken);

            _mockRepo.Setup(r => r.GetByEmailAsync(ValidEmail, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(user);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result);
            Assert.True(user.EmailConfirmed);
            Assert.Null(user.EmailConfirmationToken);

            _mockRepo.Verify(r => r.Update(user), Times.Once);
            _mockRepo.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_UserNotFound_ReturnsTrue()
        {
            var command = new ConfirmEmailCommand(ValidEmail, ValidToken);

            _mockRepo.Setup(r => r.GetByEmailAsync(ValidEmail, It.IsAny<CancellationToken>()))
                     .ReturnsAsync((User)null!);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result);
            _mockRepo.Verify(r => r.Update(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task Handle_AlreadyConfirmed_ReturnsTrue()
        {
            var user = new User(Guid.NewGuid(), "Test", ValidEmail, "hash", Role.User);
            user.ConfirmEmail();

            var command = new ConfirmEmailCommand(ValidEmail, ValidToken);

            _mockRepo.Setup(r => r.GetByEmailAsync(ValidEmail, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(user);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result);
            _mockRepo.Verify(r => r.Update(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task Handle_InvalidToken_ThrowsException()
        {
            var user = new User(Guid.NewGuid(), "Test", ValidEmail, "hash", Role.User);
            user.SetEmailConfirmationToken("DIFFERENT_TOKEN", DateTime.UtcNow.AddHours(1));

            var command = new ConfirmEmailCommand(ValidEmail, ValidToken);

            _mockRepo.Setup(r => r.GetByEmailAsync(ValidEmail, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(user);

            var ex = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Equal("Неверный токен подтверждения", ex.Message);

            _mockRepo.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ExpiredToken_ThrowsExceptionAndClearsToken()
        {
            var user = new User(Guid.NewGuid(), "Test", ValidEmail, "hash", Role.User);
            user.SetEmailConfirmationToken(ValidToken, DateTime.UtcNow.AddMinutes(-5));

            var command = new ConfirmEmailCommand(ValidEmail, ValidToken);

            _mockRepo.Setup(r => r.GetByEmailAsync(ValidEmail, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(user);

            var ex = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Equal("Срок действия токена подтверждения истек", ex.Message);

            _mockRepo.Verify(r => r.Update(user), Times.Once);
            _mockRepo.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
            Assert.Null(user.EmailConfirmationToken);
        }
    }
}