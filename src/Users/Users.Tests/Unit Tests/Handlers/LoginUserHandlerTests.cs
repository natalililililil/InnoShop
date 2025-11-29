using Moq;
using Users.Domain.Interfaces;
using Users.Application.Features.Commands.LoginUser;
using Users.Domain.Entities;
using Users.Application.DTOs;
using Users.Application.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Users.Domain.Enums;

namespace Users.Tests.Unit_Tests.Handlers
{
    public class LoginUserHandlerTests
    {
        private readonly Mock<IUserRepository> _mockRepo;
        private readonly Mock<IPasswordHasher<object>> _mockPasswordHasher;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly LoginUserHandler _handler;

        private readonly string TestEmail = "user@test.com";
        private readonly string TestPassword = "Password123";
        private readonly string HashedPassword = "HASHED_PASSWORD_MOCK";
        private readonly string GeneratedToken = "JWT_TOKEN_MOCK";

        public LoginUserHandlerTests()
        {
            _mockRepo = new Mock<IUserRepository>();
            _mockPasswordHasher = new Mock<IPasswordHasher<object>>();
            _mockTokenService = new Mock<ITokenService>();
            _mockConfiguration = new Mock<IConfiguration>();

            var jwtSection = new Mock<IConfigurationSection>();
            jwtSection.SetupGet(x => x[It.Is<string>(s => s == "ExpiryMinutes")]).Returns("60");
            _mockConfiguration.Setup(c => c.GetSection("JwtSettings")).Returns(jwtSection.Object);

            _handler = new LoginUserHandler(_mockRepo.Object, _mockPasswordHasher.Object,
                                             _mockConfiguration.Object, _mockTokenService.Object);
        }

        [Fact]
        public async Task Handle_ValidCredentials_ReturnsAuthResultDto()
        {
            var user = new User(Guid.NewGuid(), "Test", TestEmail, HashedPassword, Role.User);
            user.ConfirmEmail();
            user.Activate();

            var dto = new LoginDto { Email = TestEmail, Password = TestPassword };
            var command = new LoginUserCommand(dto);

            _mockRepo.Setup(r => r.GetByEmailAsync(TestEmail, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            _mockPasswordHasher.Setup(h => h.VerifyHashedPassword(null!, HashedPassword, TestPassword))
                               .Returns(PasswordVerificationResult.Success);

            _mockTokenService.Setup(t => t.GenerateToken(user, It.IsAny<DateTime>())).Returns(GeneratedToken);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(GeneratedToken, result.Token);

            _mockTokenService.Verify(t => t.GenerateToken(user, It.IsAny<DateTime>()), Times.Once);
        }

        [Fact]
        public async Task Handle_PasswordNeedsRehash_LogsInSuccessfully()
        {
            var user = new User(Guid.NewGuid(), "Test", TestEmail, HashedPassword, Role.User);
            user.ConfirmEmail();
            user.Activate();

            var dto = new LoginDto { Email = TestEmail, Password = TestPassword };
            var command = new LoginUserCommand(dto);

            _mockPasswordHasher.Setup(h => h.VerifyHashedPassword(null!, HashedPassword, TestPassword))
                               .Returns(PasswordVerificationResult.SuccessRehashNeeded);

            _mockRepo.Setup(r => r.GetByEmailAsync(TestEmail, It.IsAny<CancellationToken>())).ReturnsAsync(user);
            _mockTokenService.Setup(t => t.GenerateToken(user, It.IsAny<DateTime>())).Returns(GeneratedToken);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.NotNull(result);
            _mockTokenService.Verify(t => t.GenerateToken(user, It.IsAny<DateTime>()), Times.Once);
        }

        [Fact]
        public async Task Handle_FailedPasswordVerification_ThrowsException()
        {
            var user = new User(Guid.NewGuid(), "Test", TestEmail, HashedPassword, Role.User);
            user.ConfirmEmail();
            user.Activate();

            var dto = new LoginDto { Email = TestEmail, Password = "WrongPassword" };
            var command = new LoginUserCommand(dto);

            _mockRepo.Setup(r => r.GetByEmailAsync(TestEmail, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            _mockPasswordHasher.Setup(h => h.VerifyHashedPassword(null!, HashedPassword, "WrongPassword"))
                               .Returns(PasswordVerificationResult.Failed);

            var ex = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Equal("Неверный логин или пароль.", ex.Message);

            _mockTokenService.Verify(t => t.GenerateToken(It.IsAny<User>(), It.IsAny<DateTime>()), Times.Never);
        }

        [Fact]
        public async Task Handle_UserNotFound_ThrowsException()
        {
            var dto = new LoginDto { Email = "nonexistent@test.com", Password = TestPassword };
            var command = new LoginUserCommand(dto);

            _mockRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync((User)null!);

            var ex = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Equal("Неверный логин или пароль.", ex.Message);
        }

        [Fact]
        public async Task Handle_UserInactive_ThrowsException()
        {
            var user = new User(Guid.NewGuid(), "Test", TestEmail, HashedPassword, Role.User);
            user.Deactivate();

            var dto = new LoginDto { Email = TestEmail, Password = TestPassword };
            var command = new LoginUserCommand(dto);

            _mockRepo.Setup(r => r.GetByEmailAsync(TestEmail, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            var ex = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Equal("Неверный логин или пароль.", ex.Message);
        }

        [Fact]
        public async Task Handle_EmailNotConfirmed_ThrowsException()
        {
            var user = new User(Guid.NewGuid(), "Test", TestEmail, HashedPassword, Role.User);
            user.Activate();

            var dto = new LoginDto { Email = TestEmail, Password = TestPassword };
            var command = new LoginUserCommand(dto);

            _mockRepo.Setup(r => r.GetByEmailAsync(TestEmail, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            _mockPasswordHasher.Setup(h => h.VerifyHashedPassword(null!, HashedPassword, TestPassword))
                               .Returns(PasswordVerificationResult.Success);

            var ex = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Contains("Аккаунт не подтвержден.", ex.Message);

            _mockTokenService.Verify(t => t.GenerateToken(It.IsAny<User>(), It.IsAny<DateTime>()), Times.Never);
        }
    }
}