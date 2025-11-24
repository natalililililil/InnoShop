using FluentValidation;

namespace Products.Application.Features.Commands.CreateProduct
{
    public class CreateProductValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductValidator()
        {
            RuleFor(x => x.Product.Name).NotEmpty().MaximumLength(150);
            RuleFor(x => x.Product.Description).MaximumLength(2000);
            RuleFor(x => x.Product.Price).GreaterThanOrEqualTo(0);
        }
    }
}
