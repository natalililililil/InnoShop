using Xunit;
using FluentValidation.TestHelper;
using Users.Application.DTOs;
using Users.Application.Features.Commands.LoginUser;

namespace Users.Tests.Unit_Tests.Validation
{
    public class LoginUserValidatorTests
    {
        private readonly LoginUserValidator _validator;

        public LoginUserValidatorTests()
        {
            _validator = new LoginUserValidator();
        }

        [Fact]
        public void Should_Not_Have_Validation_Errors_When_Data_Is_Valid()
        {
            var dto = new LoginDto { Email = "valid@test.com", Password = "AnyPassword" };
            var command = new LoginUserCommand(dto);

            _validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null, "Email не может быть пустым")]
        [InlineData("", "Email не может быть пустым")]
        [InlineData("bad-email", "Некорректный формат Email")]
        public void Should_Have_Error_When_Email_Is_Invalid(string email, string expectedMessage)
        {
            var dto = new LoginDto { Email = email, Password = "AnyPassword" };
            var command = new LoginUserCommand(dto);

            _validator.TestValidate(command).ShouldHaveValidationErrorFor(c => c.Login.Email)
                .WithErrorMessage(expectedMessage);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Should_Have_Error_When_Password_Is_Empty(string password)
        {
            var dto = new LoginDto { Email = "valid@test.com", Password = password };
            var command = new LoginUserCommand(dto);

            _validator.TestValidate(command).ShouldHaveValidationErrorFor(c => c.Login.Password)
                .WithErrorMessage("Пароль не может быть пустым");
        }
    }
}