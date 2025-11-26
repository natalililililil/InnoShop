using FluentValidation.TestHelper;
using Products.Application.DTOs;
using Products.Application.Features.Commands.UpdateProduct;

namespace Products.Tests.Unit_Tests.Validators
{
    public class UpdateProductValidatorTests
    {
        private readonly UpdateProductValidator _validator;
        private readonly Guid _productId = Guid.NewGuid();
        private readonly Guid _ownerId = Guid.NewGuid();

        public UpdateProductValidatorTests()
        {
            _validator = new UpdateProductValidator();
        }
        [Fact]
        public void Should_Not_Have_Error_When_Data_Is_Valid()
        {
            var invalidDto = new UpdateProductDto { Name = "Acer", Price = 1000 };
            var command = new UpdateProductCommand(_productId, _ownerId, invalidDto);

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Not_Have_Error_When_Price_Is_Zero()
        {
            var validDto = new UpdateProductDto { Name = "Zero Price", Price = 0 };
            var command = new UpdateProductCommand(_productId, _ownerId, validDto);

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Have_Error_When_Name_Is_Empty_On_Update()
        {
            var invalidDto = new UpdateProductDto { Name = string.Empty, Price = 10 };
            var command = new UpdateProductCommand(_productId, _ownerId, invalidDto);

            _validator.TestValidate(command)
                .ShouldHaveValidationErrorFor(cmd => cmd.Product.Name)
                .WithErrorMessage("Название продукта обязательно");
        }

        [Fact]
        public void Should_Have_Error_When_Description_Exceeds_MaxLength_On_Update()
        {
            var longDescription = new string('D', 2001);
            var invalidDto = new UpdateProductDto { Name = "Valid", Description = longDescription, Price = 10 };
            var command = new UpdateProductCommand(_productId, _ownerId, invalidDto);

            _validator.TestValidate(command)
                .ShouldHaveValidationErrorFor(cmd => cmd.Product.Description)
                .WithErrorMessage("Описание не может превышать 2000 символов");
        }

        [Fact]
        public void Should_Have_Error_When_Price_Is_Negative_On_Update()
        {
            var invalidDto = new UpdateProductDto { Name = "Valid", Price = -1 };
            var command = new UpdateProductCommand(_productId, _ownerId, invalidDto);

            _validator.TestValidate(command)
                .ShouldHaveValidationErrorFor(cmd => cmd.Product.Price)
                .WithErrorMessage("Цена не может быть отрицательной");
        }
    }
}
