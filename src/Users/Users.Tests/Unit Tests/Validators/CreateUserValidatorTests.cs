using Xunit;
using FluentValidation.TestHelper;
using Users.Application.DTOs;
using Users.Application.Features.Commands.CreateUser;

namespace Users.Tests.Unit_Tests.Validation
{
    public class CreateUserValidatorTests
    {
        private readonly CreateUserValidator _validator;

        public CreateUserValidatorTests()
        {
            _validator = new CreateUserValidator();
        }

        [Fact]
        public void Should_Not_Have_Validation_Errors_When_Data_Is_Valid()
        {
            var dto = new CreateUserDto { Name = "ValidName", Email = "valid@test.com", Password = "ValidPassword123" };
            var command = new CreateUserCommand(dto);

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Should_Have_Error_When_Name_Is_Empty(string name)
        {
            var dto = new CreateUserDto { Name = name, Email = "valid@test.com", Password = "ValidPassword123" };
            var command = new CreateUserCommand(dto);

            _validator.TestValidate(command).ShouldHaveValidationErrorFor(c => c.User.Name)
                .WithErrorMessage("Имя не может быть пустым");
        }

        [Fact]
        public void Should_Have_Error_When_Name_Exceeds_MaxLength()
        {
            var longName = new string('A', 51);
            var dto = new CreateUserDto { Name = longName, Email = "valid@test.com", Password = "ValidPassword123" };
            var command = new CreateUserCommand(dto);

            _validator.TestValidate(command).ShouldHaveValidationErrorFor(c => c.User.Name);
        }

        [Theory]
        [InlineData(null, "Email не может быть пустым")]
        [InlineData("", "Email не может быть пустым")]
        [InlineData("bad-format", "Некорректный формат Email")]
        public void Should_Have_Error_When_Email_Is_Invalid(string email, string expectedMessage)
        {
            var dto = new CreateUserDto { Name = "ValidName", Email = email, Password = "ValidPassword123" };
            var command = new CreateUserCommand(dto);

            _validator.TestValidate(command).ShouldHaveValidationErrorFor(c => c.User.Email)
                .WithErrorMessage(expectedMessage);
        }

        [Theory]
        [InlineData(null, "Пароль не может быть пустым")]
        [InlineData("", "Пароль не может быть пустым")]
        [InlineData("short", "Пароль должен быть не менее 6 символов")]
        public void Should_Have_Error_When_Password_Is_Invalid(string password, string expectedMessage)
        {
            var dto = new CreateUserDto { Name = "ValidName", Email = "valid@test.com", Password = password };
            var command = new CreateUserCommand(dto);

            _validator.TestValidate(command).ShouldHaveValidationErrorFor(c => c.User.Password)
                .WithErrorMessage(expectedMessage);
        }
    }
}