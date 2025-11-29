using Microsoft.EntityFrameworkCore;
using Moq;
using System.Net;
using System.Text.Json;
using Users.Application.DTOs;
using Users.Domain.Entities;
using Users.Domain.Enums;

namespace Users.Tests.Integration_Tests.API
{
    public class AuthControllerTests : IntegrationTestBase
    {
        private const string BaseUrl = "api/auth";

        public AuthControllerTests(CustomWebApplicationFactory factory) : base(factory)
        {
            _factory.MockTokenService.Setup(s => s.GenerateToken(It.IsAny<User>(), It.IsAny<DateTime>())).Returns("TEST_JWT_TOKEN");
        }

        [Fact]
        public async Task Register_ValidData_ReturnsCreatedAndToken()
        {
            var userDto = new CreateUserDto
            {
                Name = "New User",
                Email = "new.user@test.com",
                Password = "P@ssword123"
            };

            var response = await _client.PostAsync($"{BaseUrl}/register", GetStringContent(userDto));

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<AuthResultDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(result);
            Assert.Equal("TEST_JWT_TOKEN", result.Token);

            var createdUser = await _context.Users.SingleOrDefaultAsync(u => u.Email == userDto.Email);
            Assert.NotNull(createdUser);
            Assert.False(createdUser.EmailConfirmed);
        }

        [Fact]
        public async Task Register_ExistingEmail_Returns400BadRequest()
        {
            var existingUser = await SeedUser("existing.email@test.com");
            var userDto = new CreateUserDto
            {
                Name = "New User",
                Email = existingUser.Email,
                Password = "P@ssword123"
            };

            var response = await _client.PostAsync($"{BaseUrl}/register", GetStringContent(userDto));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOk()
        {
            var user = await SeedUser("login.test@test.com");
            var loginDto = new LoginDto
            {
                Email = "login.test@test.com",
                Password = "P@ssword123"
            };

            var response = await _client.PostAsync($"{BaseUrl}/login", GetStringContent(loginDto));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<AuthResultDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.NotNull(result);
            Assert.NotEmpty(result.Token);
        }

        [Fact]
        public async Task ConfirmEmail_ValidToken_ReturnsOk()
        {
            var userEmail = "confirm.email.valid.1@test.com";
            var tokenValue = "VALID_TEST_TOKEN_123";
            var userId = Guid.NewGuid();

            var unconfirmedUser = new User(
                userId,
                "Token Test User",
                userEmail,
                "password_hash_placeholder",
                Role.User);

            unconfirmedUser.SetEmailConfirmationToken(tokenValue, DateTime.UtcNow.AddHours(1));

            await _context.Users.AddAsync(unconfirmedUser);
            await _context.SaveChangesAsync();

            var response = await _client.GetAsync($"{BaseUrl}/confirm-email?email={userEmail}&token={tokenValue}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Ваш аккаунт успешно подтвержден", content);

            var updatedUser = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);

            Assert.True(updatedUser!.EmailConfirmed);

            Assert.Null(updatedUser.EmailConfirmationToken);
            Assert.Null(updatedUser.EmailConfirmationTokenExpiry);
        }

        [Fact]
        public async Task ForgotPassword_UserExists_ReturnsOkAndSendsEmail()
        {
            var user = await SeedUser("forgot.password@test.com");
            var dto = new ForgotPasswordDto(user.Email);

            var response = await _client.PostAsync($"{BaseUrl}/forgot-password", GetStringContent(dto));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("ссылка для сброса была отправлена на почту", content);

            _factory.MockEmailService.Verify(m => m.SendEmailAsync(
                It.Is<string>(e => e == user.Email),
                It.IsAny<string>(),
                It.IsAny<string>()),
                Times.Once);
        }
    }
}
