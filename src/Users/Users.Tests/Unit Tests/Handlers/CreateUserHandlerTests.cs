using Moq;
using Users.Domain.Interfaces;
using Users.Application.Features.Commands.CreateUser;
using Users.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Users.Application.Services;
using Users.Application.DTOs;
using Users.Domain.Enums;
using System.Net.Mail;
using FluentValidation;
using Microsoft.Extensions.Configuration;

namespace Users.Tests.Unit_Tests.Handlers
{
    public class CreateUserHandlerTests
    {
        private readonly Mock<IUserRepository> _mockRepo;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<IPasswordHasher<object>> _mockPasswordHasher;
        private readonly Mock<IConfiguration> _mockConfiguration;

        private readonly CreateUserHandler _handler;

        private readonly string TestPassword = "TestPassword123";
        private readonly string HashedPassword = "HASHED_PASSWORD";

        public CreateUserHandlerTests()
        {
            _mockRepo = new Mock<IUserRepository>();
            _mockEmailService = new Mock<IEmailService>();
            _mockPasswordHasher = new Mock<IPasswordHasher<object>>();
            _mockConfiguration = new Mock<IConfiguration>();

            _mockPasswordHasher.Setup(h => h.HashPassword(null!, TestPassword)).Returns(HashedPassword);

            _handler = new CreateUserHandler(_mockRepo.Object, _mockEmailService.Object, _mockPasswordHasher.Object, _mockConfiguration.Object);
        }

        [Fact]
        public async Task Handle_ValidData_CreatesUserSendsEmailAndReturnsUser()
        {
            var dto = new CreateUserDto { Name = "NewUser", Email = "new@test.com", Password = TestPassword, Role = "User" };
            var command = new CreateUserCommand(dto);

            _mockRepo.Setup(r => r.GetByEmailAsync(dto.Email, It.IsAny<CancellationToken>()))
                     .ReturnsAsync((User)null!);

            var createdUser = await _handler.Handle(command, CancellationToken.None);

            Assert.NotNull(createdUser);
            Assert.Equal(dto.Email, createdUser.Email);
            Assert.Equal(HashedPassword, createdUser.PasswordHash);
            Assert.Equal(Role.User, createdUser.Role);

            Assert.NotNull(createdUser.EmailConfirmationToken);

            _mockRepo.Verify(r => r.Create(createdUser), Times.Once);
            _mockRepo.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);

            _mockEmailService.Verify(e => e.SendEmailAsync(
                createdUser.Email,
                It.IsAny<string>(),
                It.Is<string>(body => body.Contains(createdUser.EmailConfirmationToken!))),
                Times.Once
            );
        }

        [Fact]
        public async Task Handle_ExistingEmail_ThrowsValidationException()
        {
            var dto = new CreateUserDto { Name = "NewUser", Email = "existing@test.com", Password = TestPassword, Role = "User" };
            var command = new CreateUserCommand(dto);

            _mockRepo.Setup(r => r.GetByEmailAsync(dto.Email, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new User("OldUser", dto.Email, "oldhash", Role.User));

            await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command, CancellationToken.None));

            _mockRepo.Verify(r => r.Create(It.IsAny<User>()), Times.Never);
            _mockRepo.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
            _mockEmailService.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_InvalidRole_ThrowsValidationException()
        {
            var dto = new CreateUserDto { Name = "NewUser", Email = "new@test.com", Password = TestPassword, Role = "SuperUser" };
            var command = new CreateUserCommand(dto);

            await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_SmtpFailedRecipientException_ThrowsValidationException()
        {
            var dto = new CreateUserDto { Name = "NewUser", Email = "bad@test.com", Password = TestPassword, Role = "User" };
            var command = new CreateUserCommand(dto);

            _mockRepo.Setup(r => r.GetByEmailAsync(dto.Email, It.IsAny<CancellationToken>()))
                     .ReturnsAsync((User)null!);

            _mockEmailService.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new SmtpFailedRecipientException(
                    SmtpStatusCode.BadCommandSequence,
                    "bad@test.com"              
                ));

            var ex = await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Contains("Указанный адрес электронной почты не существует", ex.Message);

            _mockRepo.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}