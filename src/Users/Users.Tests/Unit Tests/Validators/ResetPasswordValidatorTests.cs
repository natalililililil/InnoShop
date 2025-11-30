using Xunit;
using FluentValidation.TestHelper;
using Users.Application.DTOs;
using Users.Application.Features.Commands.ResetPassword;

namespace Users.Tests.Unit_Tests.Validation
{
    public class ResetPasswordValidatorTests
    {
        private readonly ResetPasswordValidator _validator;

        public ResetPasswordValidatorTests()
        {
            _validator = new ResetPasswordValidator();
        }

        [Fact]
        public void Should_Not_Have_Validation_Errors_When_Data_Is_Valid()
        {
            var dto = new ResetPasswordDto("validToken", "valid@test.com", "ValidPass123");
            var command = new ResetPasswordCommand(dto);

            _validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null, "Email не может быть пустым")]
        [InlineData("bad-format", "Некорректный формат Email")]
        public void Should_Have_Error_When_Email_Is_Invalid(string email, string expectedMessage)
        {
            var dto = new ResetPasswordDto("token", email, "ValidPass123");
            var command = new ResetPasswordCommand(dto);

            _validator.TestValidate(command).ShouldHaveValidationErrorFor(c => c.Dto.Email)
                .WithErrorMessage(expectedMessage);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Should_Have_Error_When_Token_Is_Empty(string token)
        {
            var dto = new ResetPasswordDto(token, "valid@test.com", "ValidPass123");
            var command = new ResetPasswordCommand(dto);

            _validator.TestValidate(command).ShouldHaveValidationErrorFor(c => c.Dto.Token)
                .WithErrorMessage("Токен сброса не может быть пустым");
        }

        [Theory]
        [InlineData(null, "Новый пароль не может быть пустым")]
        [InlineData("", "Новый пароль не может быть пустым")]
        [InlineData("short", "Пароль должен содержать не менее 6 символов")]
        public void Should_Have_Error_When_NewPassword_Is_Invalid(string password, string expectedMessage)
        {
            var dto = new ResetPasswordDto("token", "valid@test.com", password);
            var command = new ResetPasswordCommand(dto);

            _validator.TestValidate(command).ShouldHaveValidationErrorFor(c => c.Dto.NewPassword)
                .WithErrorMessage(expectedMessage);
        }
    }
}