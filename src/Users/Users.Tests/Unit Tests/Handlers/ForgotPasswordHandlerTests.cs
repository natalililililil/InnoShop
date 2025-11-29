using Moq;
using Users.Domain.Interfaces;
using Users.Application.Features.Commands.ForgotPassword;
using Users.Domain.Entities;
using Users.Application.DTOs;
using Users.Application.Services;
using Users.Domain.Enums;

namespace Users.Tests.Unit_Tests.Handlers
{
    public class ForgotPasswordHandlerTests
    {
        private readonly Mock<IUserRepository> _mockRepo;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly ForgotPasswordHandler _handler;

        private readonly string TestEmail = "test@user.com";

        public ForgotPasswordHandlerTests()
        {
            _mockRepo = new Mock<IUserRepository>();
            _mockEmailService = new Mock<IEmailService>();
            _handler = new ForgotPasswordHandler(_mockRepo.Object, _mockEmailService.Object);
        }

        [Fact]
        public async Task Handle_ExistingActiveUser_GeneratesTokenSendsEmailAndSaves()
        {
            var user = new User(Guid.NewGuid(), "Test", TestEmail, "hash", Role.User);
            user.Activate();
            var dto = new ForgotPasswordDto(TestEmail);
            var command = new ForgotPasswordCommand(dto);

            _mockRepo.Setup(r => r.GetByEmailAsync(TestEmail, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(user);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Contains("Токен сброса:", result);

            Assert.NotNull(user.PasswordResetToken);
            Assert.NotNull(user.PasswordResetTokenExpiry);

            _mockRepo.Verify(r => r.Update(user), Times.Once);
            _mockRepo.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);

            _mockEmailService.Verify(e => e.SendEmailAsync(
                TestEmail,
                It.IsAny<string>(),
                It.Is<string>(body => body.Contains(user.PasswordResetToken!))),
                Times.Once
            );
        }

        [Fact]
        public async Task Handle_UserNotFound_ReturnsGenericMessageAndDoesNotSave()
        {
            var dto = new ForgotPasswordDto("nonexistent@user.com");
            var command = new ForgotPasswordCommand(dto);

            _mockRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync((User)null!);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Contains("Если пользователь существует", result);

            _mockRepo.Verify(r => r.Update(It.IsAny<User>()), Times.Never);
            _mockRepo.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);

            _mockEmailService.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ExistingInactiveUser_ReturnsGenericMessageAndDoesNotSave()
        {
            var user = new User(Guid.NewGuid(), "Test", TestEmail, "hash", Role.User);
            user.Deactivate();
            var dto = new ForgotPasswordDto(TestEmail);
            var command = new ForgotPasswordCommand(dto);

            _mockRepo.Setup(r => r.GetByEmailAsync(TestEmail, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(user);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Contains("Если пользователь существует", result);

            _mockRepo.Verify(r => r.Update(It.IsAny<User>()), Times.Never);
            _mockRepo.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);

            _mockEmailService.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}