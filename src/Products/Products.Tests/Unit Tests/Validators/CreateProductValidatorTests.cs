using FluentValidation.TestHelper;
using Products.Application.DTOs;
using Products.Application.Features.Commands.CreateProduct;

namespace Products.Tests.Unit_Tests.Validators
{
    public class CreateProductValidatorTests
    {
        private readonly CreateProductValidator _validator;
        private readonly Guid _ownerId = Guid.NewGuid();

        public CreateProductValidatorTests()
        {
            _validator = new CreateProductValidator();
        }

        [Fact]
        public void Should_Not_Have_Error_When_Data_Is_Valid()
        {
            var validDto = new CreateProductDto { Name = "Valid Name", Price = 101 };
            var command = new CreateProductCommand(_ownerId, validDto);

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Not_Have_Error_When_Price_Is_Zero()
        {
            var validDto = new CreateProductDto { Name = "Zero Price", Price = 0 };
            var command = new CreateProductCommand(_ownerId, validDto);

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Have_Error_When_Name_Is_Empty()
        {
            var invalidDto = new CreateProductDto { Name = string.Empty, Price = 1.7m };
            var command = new CreateProductCommand(_ownerId, invalidDto);

            _validator.TestValidate(command)
                .ShouldHaveValidationErrorFor(cmd => cmd.Product.Name)
                .WithErrorMessage("Название продукта обязательно");
        }

        [Fact]
        public void Should_Have_Error_When_Name_Exceeds_MaxLength()
        {
            var longName = new string('A', 151);
            var invalidDto = new CreateProductDto { Name = longName, Price = 10 };
            var command = new CreateProductCommand(_ownerId, invalidDto);

            _validator.TestValidate(command)
                .ShouldHaveValidationErrorFor(cmd => cmd.Product.Name)
                .WithErrorMessage("Название продукта не может превышать 150 символов");
        }

        [Fact]
        public void Should_Have_Error_When_Price_Is_Negative()
        {
            var invalidDto = new CreateProductDto { Name = "Negative Price", Price = -0.01m };
            var command = new CreateProductCommand(_ownerId, invalidDto);

            _validator.TestValidate(command)
                .ShouldHaveValidationErrorFor(cmd => cmd.Product.Price)
                .WithErrorMessage("Цена не может быть отрицательной");
        }
    }
}
