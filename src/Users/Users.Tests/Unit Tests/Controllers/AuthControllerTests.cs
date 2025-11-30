using Moq;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Users.Api.Controllers;
using Users.Application.DTOs;
using Users.Application.Services;
using Users.Domain.Entities;
using Users.Application.Features.Commands.CreateUser;
using Users.Application.Features.Commands.LoginUser;
using Users.Application.Features.Commands.ConfirmEmail;
using Users.Application.Features.Commands.ForgotPassword;
using Users.Application.Features.Commands.ResetPassword;
using Users.Domain.Enums;
using Users.Tests.Unit_Tests.DTOs;
using Users.Application.Exceptions;

namespace Users.Tests.Unit_Tests.Controlles
{
    public class AuthControllerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockTokenService = new Mock<ITokenService>();
            _mockConfiguration = new Mock<IConfiguration>();

            var jwtSettings = new Mock<IConfigurationSection>();
            jwtSettings.Setup(s => s["ExpiryMinutes"]).Returns("60");
            _mockConfiguration.Setup(c => c.GetSection("JwtSettings")).Returns(jwtSettings.Object);

            _controller = new AuthController(_mockMediator.Object, _mockTokenService.Object, _mockConfiguration.Object);
        }

        [Fact]
        public async Task Register_ValidData_ReturnsCreatedAction()
        {
            var userDto = new CreateUserDto { Email = "test@example.com", Name = "Test" };
            var createdUser = new User("Test", userDto.Email, "hash", Role.User);
            var expectedToken = "mock.jwt.token";

            _mockMediator.Setup(m => m.Send(It.IsAny<CreateUserCommand>(), default))
                         .ReturnsAsync(createdUser);

            _mockTokenService.Setup(t => t.GenerateToken(createdUser, It.IsAny<DateTime>()))
                             .Returns(expectedToken);

            var result = await _controller.Register(userDto);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.Login), createdResult.ActionName);

            var authResult = Assert.IsType<AuthResultDto>(createdResult.Value);
            Assert.Equal(expectedToken, authResult.Token);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkWithUserId()
        {
            var loginDto = new LoginDto { Email = "test@example.com", Password = "Password123" };
            var expectedResultDto = new AuthResultDto(Token: "mock.token", ExpiryDate: DateTime.UtcNow.AddHours(1));

            _mockMediator.Setup(m => m.Send(It.IsAny<LoginUserCommand>(), default))
                         .ReturnsAsync(expectedResultDto);

            var result = await _controller.Login(loginDto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedDto = Assert.IsType<AuthResultDto>(okResult.Value);

            Assert.Equal(expectedResultDto.Token, returnedDto.Token);
        }

        [Fact]
        public async Task Login_InvalidCredentials_ThrowsAuthenticationException()
        {
            var exceptionMessage = "Неверный логин или пароль.";

            _mockMediator.Setup(m => m.Send(It.IsAny<LoginUserCommand>(), default))
                         .ThrowsAsync(new AuthenticationException(exceptionMessage));

            var exception = await Assert.ThrowsAsync<AuthenticationException>(
                () => _controller.Login(new LoginDto())
            );

            Assert.Equal(exceptionMessage, exception.Message);
        }

        [Theory]
        [InlineData("", "tokenValue")]
        [InlineData("email@test.com", "")]
        public async Task ConfirmEmail_MissingParams_ReturnsBadRequest(string email, string token)
        {
            var result = await _controller.ConfirmEmail(email, token);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var returnedDto = Assert.IsType<MessageDto>(badRequestResult.Value);

            Assert.Equal("Некорректная ссылка подтверждения", returnedDto.Message);
        }

        [Fact]
        public async Task ConfirmEmail_Success_ReturnsOk()
        {
            string successMessage = "Ваш аккаунт успешно подтвержден! Теперь вы можете войти";

            _mockMediator.Setup(m => m.Send(It.IsAny<ConfirmEmailCommand>(), default))
                         .ReturnsAsync(true);

            var result = await _controller.ConfirmEmail("valid@test.com", "validToken");

            var okResult = Assert.IsType<OkObjectResult>(result);

            var returnedDto = Assert.IsType<MessageDto>(okResult.Value);

            Assert.Equal(successMessage, returnedDto.Message);
        }

        [Fact]
        public async Task ConfirmEmail_Failure_ReturnsBadRequest()
        {
            string failureMessage = "Не удалось подтвердить аккаунт";

            _mockMediator.Setup(m => m.Send(It.IsAny<ConfirmEmailCommand>(), default))
                         .ReturnsAsync(false);

            var result = await _controller.ConfirmEmail("invalid@test.com", "badToken");

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

            var returnedDto = Assert.IsType<MessageDto>(badRequestResult.Value);

            Assert.Equal(failureMessage, returnedDto.Message);
        }

        [Fact]
        public async Task ForgotPassword_Success_ReturnsOkMessage()
        {
            var dto = new ForgotPasswordDto("email@test.com");
            _mockMediator.Setup(m => m.Send(It.IsAny<ForgotPasswordCommand>(), default))
                         .ReturnsAsync("Success");

            var result = await _controller.ForgotPassword(dto);

            var okResult = Assert.IsType<OkObjectResult>(result);

            var returnedDto = Assert.IsType<MessageDto>(okResult.Value);

            Assert.Contains("ссылка для сброса была отправлена", returnedDto.Message);
        }

        [Fact]
        public async Task ResetPassword_Success_ReturnsOk()
        {
            var dto = new ResetPasswordDto("token", "email@test.com", "NewPass123");
            _mockMediator.Setup(m => m.Send(It.IsAny<ResetPasswordCommand>(), default))
                         .ReturnsAsync(true);

            var result = await _controller.ResetPassword(dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedDto = Assert.IsType<MessageDto>(okResult.Value);

            Assert.Equal("Пароль успешно сброшен.", returnedDto.Message);
        }

        [Fact]
        public async Task ResetPassword_Failure_ReturnsBadRequest()
        {
            var dto = new ResetPasswordDto("token", "email@test.com", "NewPass123");
            _mockMediator.Setup(m => m.Send(It.IsAny<ResetPasswordCommand>(), default))
                         .ReturnsAsync(false);

            var result = await _controller.ResetPassword(dto);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

            var returnedDto = Assert.IsType<MessageDto>(badRequestResult.Value);

            Assert.Equal("Не удалось сбросить пароль.", returnedDto.Message);
        }
    }
}