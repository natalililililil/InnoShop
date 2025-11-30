using Moq;
using Users.Domain.Interfaces;
using Users.Application.Features.Commands.ResetPassword;
using Users.Domain.Entities;
using Users.Application.DTOs;
using Microsoft.AspNetCore.Identity;
using Users.Domain.Enums;

namespace Users.Tests.Unit_Tests.Handlers
{
    public class ResetPasswordHandlerTests
    {
        private readonly Mock<IUserRepository> _mockRepo;
        private readonly Mock<IPasswordHasher<object>> _mockPasswordHasher;
        private readonly ResetPasswordHandler _handler;

        private readonly string TestEmail = "reset@test.com";
        private readonly string ValidToken = "VALID_RESET_TOKEN";
        private readonly string NewPassword = "NewSecurePassword123";
        private readonly string NewPasswordHash = "NEW_HASHED_PASSWORD_MOCK";

        public ResetPasswordHandlerTests()
        {
            _mockRepo = new Mock<IUserRepository>();
            _mockPasswordHasher = new Mock<IPasswordHasher<object>>();

            _mockPasswordHasher.Setup(h => h.HashPassword(null!, NewPassword)).Returns(NewPasswordHash);

            _handler = new ResetPasswordHandler(_mockRepo.Object, _mockPasswordHasher.Object);
        }

        [Fact]
        public async Task Handle_ValidTokenAndPassword_ResetsPasswordAndReturnsTrue()
        {
            var user = new User(Guid.NewGuid(), "Test", TestEmail, "old_hash", Role.User);
            user.Activate();
            user.SetPasswordResetToken(ValidToken, DateTime.UtcNow.AddHours(1));

            var dto = new ResetPasswordDto(ValidToken, TestEmail, NewPassword);
            var command = new ResetPasswordCommand(dto);

            _mockRepo.Setup(r => r.GetByEmailAsync(TestEmail, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(user);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result);

            Assert.Equal(NewPasswordHash, user.PasswordHash);

            Assert.Null(user.PasswordResetToken);

            _mockRepo.Verify(r => r.Update(user), Times.Once);
            _mockRepo.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("InactiveUser")]
        public async Task Handle_UserNotFoundOrInactive_ThrowsException(string status)
        {
            User user = status == null ? null! : new User(Guid.NewGuid(), "Test", TestEmail, "hash", Role.User);
            if (user != null) user.Deactivate(); 

            var dto = new ResetPasswordDto(ValidToken, TestEmail, NewPassword);
            var command = new ResetPasswordCommand(dto);

            _mockRepo.Setup(r => r.GetByEmailAsync(TestEmail, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(user);

            var ex = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Equal("Неверный запрос или пользователь не найден.", ex.Message);

            _mockRepo.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_InvalidToken_ThrowsException()
        {   
            var user = new User(Guid.NewGuid(), "Test", TestEmail, "old_hash", Role.User);
            user.Activate();
            user.SetPasswordResetToken("DIFFERENT_TOKEN", DateTime.UtcNow.AddHours(1));

            var dto = new ResetPasswordDto(ValidToken, TestEmail, NewPassword); 
            var command = new ResetPasswordCommand(dto);

            _mockRepo.Setup(r => r.GetByEmailAsync(TestEmail, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(user);

            var ex = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Equal("Неверный или просроченный токен сброса.", ex.Message);

            _mockRepo.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ExpiredToken_ThrowsExceptionAndClearsToken()
        {
            var user = new User(Guid.NewGuid(), "Test", TestEmail, "old_hash", Role.User);
            user.Activate();
            user.SetPasswordResetToken(ValidToken, DateTime.UtcNow.AddMinutes(-5));

            var dto = new ResetPasswordDto(ValidToken, TestEmail, NewPassword);
            var command = new ResetPasswordCommand(dto);

            _mockRepo.Setup(r => r.GetByEmailAsync(TestEmail, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(user);

            var ex = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Equal("Срок действия токена сброса истек.", ex.Message);

            _mockRepo.Verify(r => r.Update(user), Times.Once);
            _mockRepo.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
            Assert.Null(user.PasswordResetToken);
        }
    }
}