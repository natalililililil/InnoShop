using Xunit;
using FluentValidation.TestHelper;
using Users.Application.DTOs;
using Users.Application.Features.Commands.ForgotPassword;

namespace Users.Tests.Unit_Tests.Validation
{
    public class ForgotPasswordValidatorTests
    {
        private readonly ForgotPasswordValidator _validator;

        public ForgotPasswordValidatorTests()
        {
            _validator = new ForgotPasswordValidator();
        }

        [Fact]
        public void Should_Not_Have_Validation_Errors_When_Email_Is_Valid()
        {
            var dto = new ForgotPasswordDto("valid@test.com");
            var command = new ForgotPasswordCommand(dto);

            _validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null, "Email не может быть пустым")]
        [InlineData("", "Email не может быть пустым")]
        [InlineData("not-email", "Некорректный формат Email")]
        public void Should_Have_Error_When_Email_Is_Invalid(string email, string expectedMessage)
        {
            var dto = new ForgotPasswordDto(email);
            var command = new ForgotPasswordCommand(dto);

            _validator.TestValidate(command).ShouldHaveValidationErrorFor(c => c.Dto.Email)
                .WithErrorMessage(expectedMessage);
        }
    }
}